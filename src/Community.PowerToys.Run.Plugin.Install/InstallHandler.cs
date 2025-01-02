using System.Text.Json;
using Community.PowerToys.Run.Plugin.Abstractions;
using Community.PowerToys.Run.Plugin.Install.Models;
using LazyCache;
using Wox.Infrastructure;
using Wox.Plugin;
using static Wox.Infrastructure.Helper;
using static Wox.Plugin.Common.DefaultBrowserInfo;

namespace Community.PowerToys.Run.Plugin.Install
{
    /// <summary>
    /// Handles the install/update/uninstall of plugins.
    /// </summary>
    public interface IInstallHandler
    {
        /// <summary>
        /// Initialize the handler with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The plugin context.</param>
        /// <returns><c>true</c> if successfully initialized; otherwise, <c>false</c>.</returns>
        bool Init(PluginInitContext context);

        /// <summary>
        /// Returns a filtered list of actions, based on the given query and plugin state.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        IEnumerable<ActionType> Actions(Query query);

        /// <summary>
        /// Returns a filtered list of awesome plugins, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        IEnumerable<Pair> Query(Query query);

        /// <summary>
        /// Installs a plugin.
        /// </summary>
        /// <param name="pair">The plugin pair.</param>
        /// <returns><c>true</c> if successfully installed; otherwise, <c>false</c>.</returns>
        bool InstallPlugin(Pair pair);

        /// <summary>
        /// Updates a plugin.
        /// </summary>
        /// <param name="pair">The plugin pair.</param>
        /// <returns><c>true</c> if successfully updated; otherwise, <c>false</c>.</returns>
        bool UpdatePlugin(Pair pair);

        /// <summary>
        /// Uninstalls a plugin.
        /// </summary>
        /// <param name="pair">The plugin pair.</param>
        /// <returns><c>true</c> if successfully uninstalled; otherwise, <c>false</c>.</returns>
        bool UninstallPlugin(Pair pair);

        /// <summary>
        /// Opens the plugin website in the default browser.
        /// </summary>
        /// <param name="pair">The plugin pair.</param>
        /// <returns><c>true</c> if successfully opened; otherwise, <c>false</c>.</returns>
        bool OpenWebsite(Pair pair);

        /// <summary>
        /// Opens the plugin repo in the default browser.
        /// </summary>
        /// <param name="pair">The plugin pair.</param>
        /// <returns><c>true</c> if successfully opened; otherwise, <c>false</c>.</returns>
        bool OpenRepo(Pair pair);

        /// <summary>
        /// Opens the plugin release notes in the default browser.
        /// </summary>
        /// <param name="pair">The plugin pair.</param>
        /// <returns><c>true</c> if successfully opened; otherwise, <c>false</c>.</returns>
        bool OpenReleaseNotes(Pair pair);

        /// <summary>
        /// Opens the plugin folder in File Explorer.
        /// </summary>
        /// <param name="pair">The plugin pair.</param>
        /// <returns><c>true</c> if successfully opened; otherwise, <c>false</c>.</returns>
        bool OpenPluginFolder(Pair pair);
    }

    /// <inheritdoc/>
    public sealed class InstallHandler : IInstallHandler
    {
        internal const string CacheKey = "Pairs";

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallHandler"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="cache">The cache.</param>
        public InstallHandler(InstallSettings settings, IAppCache cache)
        {
            Settings = settings;
            Cache = cache;
            Helper = new HelperWrapper();
            DefaultBrowserInfo = new DefaultBrowserInfoWrapper();
            Log = new LogWrapper();
            StringMatcher = StringMatcher.Instance;
        }

        internal InstallHandler(InstallSettings settings, IAppCache cache, PluginMetadata metadata, IPublicAPI api, IHelper helper, IDefaultBrowserInfo defaultBrowserInfo, ILog log, StringMatcher matcher)
        {
            Settings = settings;
            Cache = cache;
            Metadata = metadata;
            Api = api;
            Helper = helper;
            DefaultBrowserInfo = defaultBrowserInfo;
            Log = log;
            StringMatcher = matcher;
        }

        private InstallSettings Settings { get; }

        private IAppCache Cache { get; }

        private PluginMetadata? Metadata { get; set; }

        private IPublicAPI? Api { get; set; }

        private IHelper Helper { get; }

        private IDefaultBrowserInfo DefaultBrowserInfo { get; }

        private ILog Log { get; }

        private StringMatcher StringMatcher { get; }

        /// <inheritdoc/>
        public bool Init(PluginInitContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            Metadata = context.CurrentPluginMetadata;
            Api = context.API;
            DefaultBrowserInfo.UpdateIfTimePassed();

            try
            {
                var content = Settings.PluginSource.GetContent();
                if (content == null)
                {
                    Log.Error("Plugin source invalid.", GetType());
                    return false;
                }

                var awesomePlugins = JsonSerializer.Deserialize<Awesome>(content);
                var installedPlugins = Api.GetAllPlugins().ConvertAll(x => x.Metadata.Map());
                var pairs = awesomePlugins?.Plugins?.Select(awesome => new Pair
                {
                    Plugin = awesome,
                    Metadata = installedPlugins.Find(installed => installed.ID.Equals(awesome.ID, StringComparison.OrdinalIgnoreCase)),
                }).ToList() ?? [];

                Cache.Add(CacheKey, pairs, DateTimeOffset.Now.AddDays(1));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.Exception("Init failed.", ex, GetType());
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<ActionType> Actions(Query query)
        {
            var pairs = Cache.Get<List<Pair>>(CacheKey);

            if (pairs == null || pairs.Count == 0)
            {
                yield return ActionType.Validate;
            }

            if (pairs == null || pairs.Count == 0 || query?.Search.Equals("reload", StringComparison.OrdinalIgnoreCase) == true)
            {
                yield return ActionType.Reload;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Pair> Query(Query query)
        {
            var pairs = Cache.Get<List<Pair>>(CacheKey);
            if (pairs == null)
            {
                return [];
            }

            pairs.ForEach(x => x.Score = x.Score(StringMatcher, query.Search));

            return pairs.Where(x => x.HasRepository && x.Score > 0);
        }

        /// <inheritdoc/>
        public bool InstallPlugin(Pair pair)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(pair?.Plugin?.Release?.Asset?.Url);
            return OpenPowershell("Install", assetUrl: pair.Plugin.Release.Asset.Url);
        }

        /// <inheritdoc/>
        public bool UpdatePlugin(Pair pair)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(pair?.Plugin?.Release?.Asset?.Url);
            ArgumentNullException.ThrowIfNullOrEmpty(pair?.Metadata?.PluginDirectory);
            return OpenPowershell("Update", assetUrl: pair.Plugin.Release.Asset.Url, pluginDirectory: pair.Metadata.PluginDirectory);
        }

        /// <inheritdoc/>
        public bool UninstallPlugin(Pair pair)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(pair?.Metadata?.PluginDirectory);
            return OpenPowershell("Uninstall", pluginDirectory: pair.Metadata.PluginDirectory);
        }

        /// <inheritdoc/>
        public bool OpenWebsite(Pair pair)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(pair?.Plugin?.Website);
            return OpenInBrowser(pair.Plugin.Website);
        }

        /// <inheritdoc/>
        public bool OpenRepo(Pair pair)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(pair?.Plugin?.Repository?.Url);
            return OpenInBrowser(pair.Plugin.Repository.Url);
        }

        /// <inheritdoc/>
        public bool OpenReleaseNotes(Pair pair)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(pair?.Plugin?.Release?.Url);
            return OpenInBrowser(pair.Plugin.Release.Url);
        }

        /// <inheritdoc/>
        public bool OpenPluginFolder(Pair pair)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(pair?.Metadata?.PluginDirectory);
            Helper.OpenInShell("explorer.exe", pair.Metadata.PluginDirectory);
            return true;
        }

        private bool OpenPowershell(string command, string? assetUrl = null, string? pluginDirectory = null)
        {
            const string path = "powershell.exe";
            var arguments = $"-ExecutionPolicy Bypass -File \"{Metadata!.PluginDirectory}\\install.ps1\" -command \"{command}\"";

            if (assetUrl != null)
            {
                arguments += $" -assetUrl \"{assetUrl}\"";
            }

            if (pluginDirectory != null)
            {
                arguments += $" -pluginDirectory \"{pluginDirectory}\"";
            }

            Log.Info($"OpenInShell: {path} {arguments}", GetType());
            if (!Helper.OpenInShell(path, arguments, Metadata.PluginDirectory, ShellRunAsType.Administrator))
            {
                Log.Error("Run install.ps1 failed.", GetType());
                Api!.ShowMsg($"Plugin: {Metadata.Name}", "Run install.ps1 failed.");
                return false;
            }

            return true;
        }

        private bool OpenInBrowser(string url)
        {
            if (!Helper.OpenCommandInShell(Path, ArgumentsPattern, url))
            {
                Log.Error("Open default browser failed.", GetType());
                Api!.ShowMsg($"Plugin: {Metadata!.Name}", "Open default browser failed.");
                return false;
            }

            return true;
        }
    }
}
