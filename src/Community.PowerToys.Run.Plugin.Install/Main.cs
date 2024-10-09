using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using Community.PowerToys.Run.Plugin.Install.Models;
using LazyCache;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure.Storage;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Install
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, ISettingProvider, ISavable, IReloadable, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        public Main()
        {
            Storage = new PluginJsonStorage<InstallSettings>();
            Settings = Storage.Load();
            Handler = new InstallHandler(Settings, new CachingService());
        }

        internal Main(InstallSettings settings, IInstallHandler handler)
        {
            Storage = new PluginJsonStorage<InstallSettings>();
            Settings = settings;
            Handler = handler;
        }

        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "1678AF8C90A341628E5135CD3C004F46";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "Install";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Install awesome plugins";

        /// <summary>
        /// Additional options for the plugin.
        /// </summary>
        public IEnumerable<PluginAdditionalOption> AdditionalOptions => Settings.GetAdditionalOptions();

        private PluginJsonStorage<InstallSettings> Storage { get; }

        private InstallSettings Settings { get; }

        private IInstallHandler Handler { get; }

        private PluginInitContext? Context { get; set; }

        private string? IconPath { get; set; }

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            if (!Handler.IsValid())
            {
                return Invalid();
            }

            var results = Handler.Query(query);

            return results.Select(Map).ToList();

            Result Map(Pair pair) => new()
            {
                IcoPath = GetIconPath(pair),
                Title = pair.Plugin.Name,
                SubTitle = pair.Plugin.Description,
                ToolTipData = new ToolTipData(pair.ToolTipTitle, pair.ToolTipText),
                Action = _ => Handler.OpenWebsite(pair),
                Score = pair.Score,
                ContextData = pair,
            };

            List<Result> Invalid() =>
            [
                new()
                {
                    IcoPath = GetIconPathByName("Warning"),
                    Title = "Invalid plugin source",
                    SubTitle = "Make sure the URL or path to the plugin source file is valid",
                    ToolTipData = new ToolTipData("Help", "1. Open PowerToys Settings\n2. Click PowerToys Run\n3. Scroll down and expand the Install plugin\n4. Double check the Plugin Source\n5. Clear the field to reset to the default value"),
                },
            ];

            string GetIconPath(Pair pair)
            {
                if (pair.IsNotInstalled)
                {
                    return IconPath!;
                }

                var theme = Context?.API.GetCurrentTheme();
                return theme == Theme.Light || theme == Theme.HighContrastWhite ?
                    Path.Combine(pair.Metadata!.PluginDirectory, pair.Metadata.IcoPathLight) :
                    Path.Combine(pair.Metadata!.PluginDirectory, pair.Metadata.IcoPathDark);
            }

            string GetIconPathByName(string name)
            {
                var theme = Context?.API.GetCurrentTheme();
                return theme == Theme.Light || theme == Theme.HighContrastWhite ?
                    @$"Images\{name}.light.png" :
                    @$"Images\{name}.dark.png";
            }
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());

            Handler.Init(Context);
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult?.ContextData is not Pair pair)
            {
                return [];
            }

            var install = new ContextMenuResult
            {
                PluginName = Name,
                Title = "Install plugin (Ctrl+Enter)",
                FontFamily = "Segoe MDL2 Assets",
                Glyph = "\xE896", // Download
                AcceleratorKey = Key.Enter,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ => Handler.InstallPlugin(pair),
            };
            var update = new ContextMenuResult
            {
                PluginName = Name,
                Title = "Update plugin (Ctrl+Enter)",
                FontFamily = "Segoe MDL2 Assets",
                Glyph = "\xE777", // UpdateRestore
                AcceleratorKey = Key.Enter,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ => Handler.UpdatePlugin(pair),
            };
            var uninstall = new ContextMenuResult
            {
                PluginName = Name,
                Title = "Uninstall plugin (Ctrl+Del)",
                FontFamily = "Segoe MDL2 Assets",
                Glyph = "\xE74D", // Delete
                AcceleratorKey = Key.Delete,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ => Handler.UninstallPlugin(pair),
            };
            var repo = new ContextMenuResult
            {
                PluginName = Name,
                Title = "Open repo (Enter)",
                FontFamily = "Segoe MDL2 Assets",
                Glyph = "\xE82D", // Dictionary
                /*AcceleratorKey = Key.Enter,*/
                Action = _ => Handler.OpenRepo(pair),
            };
            var releaseNotes = new ContextMenuResult
            {
                PluginName = Name,
                Title = "Open release notes (Ctrl+N)",
                FontFamily = "Segoe MDL2 Assets",
                Glyph = "\xF000", // KnowledgeArticle
                AcceleratorKey = Key.N,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ => Handler.OpenReleaseNotes(pair),
            };
            var folder = new ContextMenuResult
            {
                PluginName = Name,
                Title = "Open plugin folder (Ctrl+F)",
                FontFamily = "Segoe MDL2 Assets",
                Glyph = "\xED25", // OpenFolderHorizontal
                AcceleratorKey = Key.F,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ => Handler.OpenPluginFolder(pair),
            };

            return GetResults().ToList();

            IEnumerable<ContextMenuResult> GetResults()
            {
                if (pair.IsInstallAvailable)
                {
                    yield return install;
                }

                if (pair.IsUpdateAvailable)
                {
                    yield return update;
                }

                if (pair.IsInstalled)
                {
                    yield return uninstall;
                }

                if (pair.HasRepository)
                {
                    yield return repo;
                }

                if (pair.HasReleaseNotes)
                {
                    yield return releaseNotes;
                }

                if (pair.IsInstalled)
                {
                    yield return folder;
                }
            }
        }

        /// <summary>
        /// Creates setting panel.
        /// </summary>
        /// <returns>The control.</returns>
        /// <exception cref="NotImplementedException">method is not implemented.</exception>
        public Control CreateSettingPanel() => throw new NotImplementedException();

        /// <summary>
        /// Updates settings.
        /// </summary>
        /// <param name="settings">The plugin settings.</param>
        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            Settings.SetAdditionalOptions(settings.AdditionalOptions);
            Save();
        }

        /// <summary>
        /// Saves data.
        /// </summary>
        public void Save()
        {
            Storage.Save();
        }

        /// <summary>
        /// Reinitialize the plugin.
        /// </summary>
        public void ReloadData()
        {
            Handler.Init(Context!);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/install.light.png" : "Images/install.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }
}
