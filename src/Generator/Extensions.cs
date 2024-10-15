using System.Text.RegularExpressions;

namespace Generator
{
    /// <summary>
    /// Generator extensions.
    /// </summary>
#pragma warning disable CA1724 // Type names should not match namespaces
    public static partial class Extensions
#pragma warning restore CA1724 // Type names should not match namespaces
    {
        /// <summary>
        /// Gets the content of a file.
        /// </summary>
        /// <param name="path">The path to a file.</param>
        /// <returns>The content.</returns>
        /// <exception cref="ArgumentException"><paramref name="path" /> is invalid.</exception>
        public static async Task<string> GetContentAsync(this string path)
        {
            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? uri))
            {
                throw new ArgumentException("Invalid path", nameof(path));
            }

            if (!uri.IsAbsoluteUri || uri.IsFile)
            {
                return await File.ReadAllTextAsync(path);
            }
            else
            {
                using var client = new HttpClient();
                return await client.GetStringAsync(uri);
            }
        }

        /// <summary>
        /// Ensures that the directory exists.
        /// </summary>
        /// <param name="path">The path to a file.</param>
        public static void EnsureDirectoryExists(this string path)
        {
            var directory = Path.GetDirectoryName(path);

            ArgumentNullException.ThrowIfNull(directory, nameof(path));

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Gets options from GitHub repository URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The options.</returns>
        public static GitHubRepositoryOptions GetGitHubRepositoryOptions(this string url)
        {
            ArgumentException.ThrowIfNullOrEmpty(url);

            var uri = new Uri(url);
            url = uri.GetLeftPart(UriPartial.Path);

            var result = new GitHubRepositoryOptions();
            var match = GitHubRegex().Match(url);
            if (match.Success)
            {
                result.Owner = match.Groups[1].Value;
                result.Repo = match.Groups[2].Value;
            }

            ArgumentException.ThrowIfNullOrEmpty(result.Owner);
            ArgumentException.ThrowIfNullOrEmpty(result.Repo);

            return result;
        }

        /// <summary>
        /// Maps API data.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>A model.</returns>
        public static Models.Repository? Map(this Repository repository)
        {
            if (repository == null)
            {
                return null;
            }

            return new Models.Repository
            {
                Url = repository.html_url,
                Owner = repository.owner?.login!,
                Name = repository.name,
                FullName = repository.full_name,
                Description = repository.description,
                Topics = repository.topics,
                License = repository.license?.name,
                StargazersCount = repository.stargazers_count,
                WatchersCount = repository.watchers_count,
                ForksCount = repository.forks_count,
                Size = repository.size,
                CreatedAt = repository.created_at,
                UpdatedAt = repository.updated_at,
            };
        }

        /// <summary>
        /// Maps API data.
        /// </summary>
        /// <param name="release">The release.</param>
        /// <param name="plugin">The plugin.</param>
        /// <returns>A model.</returns>
        public static Models.Release? Map(this Release release, Models.Plugin plugin)
        {
            if (release == null)
            {
                return null;
            }

            return new Models.Release
            {
                Url = release.html_url,
                TagName = release.tag_name,
                Name = release.name,
                CreatedAt = release.created_at,
                PublishedAt = release.published_at,
                Assets = GetAssets().Select(Map),
            };

            IEnumerable<Asset> GetAssets()
            {
                var result = release.assets?.Where(IsZipFile);

                if (result?.Count() <= 2)
                {
                    return result;
                }

                return result?.Where(x => HasName(x, plugin.Name)) ?? [];

                bool IsZipFile(Asset asset) => asset.name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
                bool HasName(Asset asset, string name) => asset.name.Contains(name, StringComparison.OrdinalIgnoreCase);
            }
        }

        private static Models.Asset Map(this Asset asset)
        {
            return new Models.Asset
            {
                Url = asset.browser_download_url,
                Name = asset.name,
                Size = asset.size,
                DownloadCount = asset.download_count,
                CreatedAt = asset.created_at,
                UpdatedAt = asset.updated_at,
            };
        }

        [GeneratedRegex(@"^https:\/\/github\.com\/([^\/]+)\/([^\/]+)\/?$")]
        private static partial Regex GitHubRegex();
    }
}
