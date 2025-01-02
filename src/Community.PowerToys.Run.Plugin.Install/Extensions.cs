using System.IO;
using System.Net.Http;
using Community.PowerToys.Run.Plugin.Install.Models;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Install
{
    /// <summary>
    /// Plugin extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the content of a file.
        /// </summary>
        /// <param name="path">The path to a file.</param>
        /// <returns>The content.</returns>
        /// <exception cref="ArgumentException"><paramref name="path" /> is invalid.</exception>
        public static string? GetContent(this string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            else if (IsUrl(path))
            {
                using var client = new HttpClient();
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
#pragma warning disable VSTHRD104 // Offer async methods
                return client.GetStringAsync(path).Result;
#pragma warning restore VSTHRD104 // Offer async methods
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            }
            else
            {
                return null;
            }

            static bool IsUrl(string path)
            {
                if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uri))
                {
                    return uri.Scheme == Uri.UriSchemeHttp ||
                           uri.Scheme == Uri.UriSchemeHttps ||
                           uri.Scheme == Uri.UriSchemeFtp;
                }

                return false;
            }
        }

        /// <summary>
        /// Maps plugin metadata.
        /// </summary>
        /// <param name="plugin">The plugin metadata.</param>
        /// <returns>A model.</returns>
        public static Metadata Map(this PluginMetadata plugin)
        {
            ArgumentNullException.ThrowIfNull(plugin);

            return new Metadata
            {
                ID = plugin.ID,
                Name = plugin.Name,
                Version = plugin.Version,
                PluginDirectory = plugin.PluginDirectory,
                IcoPathDark = plugin.IcoPathDark,
                IcoPathLight = plugin.IcoPathLight,
            };
        }

        /// <summary>
        /// Gets the score of a fuzzy match.
        /// </summary>
        /// <param name="pair">The plugin pair.</param>
        /// <param name="matcher">The fuzzy matcher.</param>
        /// <param name="query">The query.</param>
        /// <returns>The score.</returns>
        public static int Score(this Pair pair, StringMatcher matcher, string query)
        {
            ArgumentNullException.ThrowIfNull(pair);
            ArgumentNullException.ThrowIfNull(matcher);

            if (string.IsNullOrWhiteSpace(query))
            {
                return 1;
            }

            return matcher.FuzzyMatch(query, pair.Queryable).Score;
        }

        /// <summary>
        /// Formats the file size.
        /// </summary>
        /// <param name="bytes">The file size in bytes.</param>
        /// <returns>A formated sting.</returns>
        public static string FormatSize(this int bytes)
        {
            string[] suffixes = ["bytes", "KB", "MB", "GB"];

            if (bytes < 0)
            {
                return "-" + FormatSize(-bytes);
            }

            int i = 0;
            decimal d = bytes;
            while (Math.Round(d) >= 1000)
            {
                d /= 1024;
                i++;
            }

            return $"{d:n0} {suffixes[i]}";
        }
    }
}
