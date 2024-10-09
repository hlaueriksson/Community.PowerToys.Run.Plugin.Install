using System.Net.Http.Json;

namespace Generator
{
    /// <summary>
    /// GitHub API.
    /// </summary>
    public interface IGitHubClient
    {
        /// <summary>
        /// Get repository.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>A repository.</returns>
        Task<Repository?> GetRepositoryAsync(GitHubRepositoryOptions options);

        /// <summary>
        /// Get the latest release.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>A release.</returns>
        Task<Release?> GetLatestReleaseAsync(GitHubRepositoryOptions options);
    }

    /// <inheritdoc/>
    public class GitHubClient : IGitHubClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubClient"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public GitHubClient(GitHubOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            HttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com"),
                Timeout = TimeSpan.FromSeconds(5),
            };
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Community.PowerToys.Run.Plugin.Install");

            if (!string.IsNullOrEmpty(options.PersonalAccessToken))
            {
                // https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-fine-grained-personal-access-token
                HttpClient.DefaultRequestHeaders.Add("Authorization", $"token {options.PersonalAccessToken}");
            }
        }

        private HttpClient HttpClient { get; }

        /// <inheritdoc/>
        public async Task<Repository?> GetRepositoryAsync(GitHubRepositoryOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // https://docs.github.com/en/rest/repos/repos?apiVersion=2022-11-28#get-a-repository
            return await HttpClient.GetFromJsonAsync<Repository>($"/repos/{options.Owner}/{options.Repo}").ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<Release?> GetLatestReleaseAsync(GitHubRepositoryOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28#get-the-latest-release
            return await HttpClient.GetFromJsonAsync<Release>($"/repos/{options.Owner}/{options.Repo}/releases/latest").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Options.
    /// </summary>
    public class GitHubOptions
    {
        public string PersonalAccessToken { get; set; }
    }

    /// <summary>
    /// Repository options.
    /// </summary>
    public class GitHubRepositoryOptions
    {
        /// <summary>
        /// Gets or sets the account owner of the repository.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the name of the repository.
        /// </summary>
        public string Repo { get; set; }
    }

    /*
    {
      "name": "Hello-World",
      "full_name": "octocat/Hello-World",
      "owner": {
        "login": "octocat",
        "avatar_url": "https://github.com/images/error/octocat_happy.gif",
        "type": "User"
      },
      "html_url": "https://github.com/octocat/Hello-World",
      "description": "This your first repo!",
      "forks_count": 9,
      "stargazers_count": 80,
      "watchers_count": 80,
      "size": 108,
      "default_branch": "master",
      "open_issues_count": 0,
      "topics": [
        "octocat",
        "atom",
        "electron",
        "api"
      ],
      "pushed_at": "2011-01-26T19:06:43Z",
      "created_at": "2011-01-26T19:01:12Z",
      "updated_at": "2011-01-26T19:14:43Z",
      "subscribers_count": 42,
      "network_count": 0,
      "license": {
        "name": "MIT License"
      }
    }
     */
    public class Repository
    {
        public string name { get; set; }
        public string full_name { get; set; }
        public Owner owner { get; set; }
        public string html_url { get; set; }
        public string description { get; set; }
        public int forks_count { get; set; }
        public int stargazers_count { get; set; }
        public int watchers_count { get; set; }
        public int size { get; set; }
        public string default_branch { get; set; }
        public int open_issues_count { get; set; }
        public string[] topics { get; set; }
        public DateTime pushed_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int subscribers_count { get; set; }
        public int network_count { get; set; }
        public License? license { get; set; }
    }

    public class Owner
    {
        public string login { get; set; }
        public string avatar_url { get; set; }
        public string type { get; set; }
    }

    public class License
    {
        public string name { get; set; }
    }

    /*
    {
      "html_url": "https://github.com/octocat/Hello-World/releases/v1.0.0",
      "id": 1,
      "tag_name": "v1.0.0",
      "name": "v1.0.0",
      "body": "Description of the release",
      "draft": false,
      "prerelease": false,
      "created_at": "2013-02-27T19:35:32Z",
      "published_at": "2013-02-27T19:35:32Z",
      "assets": [
        {
          "browser_download_url": "https://github.com/octocat/Hello-World/releases/download/v1.0.0/example.zip",
          "id": 1,
          "name": "example.zip",
          "label": "short description",
          "state": "uploaded",
          "content_type": "application/zip",
          "size": 1024,
          "download_count": 42,
          "created_at": "2013-02-27T19:35:32Z",
          "updated_at": "2013-02-27T19:35:32Z"
        }
      ]
    }
     */

    /// <summary>
    /// Release of a GitHub repository.
    /// </summary>
    public class Release
    {
        public string html_url { get; set; }
        public int id { get; set; }
        public string tag_name { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public bool draft { get; set; }
        public bool prerelease { get; set; }
        public DateTime created_at { get; set; }
        public DateTime published_at { get; set; }
        public Asset[] assets { get; set; }
    }

    /// <summary>
    /// Release asset of a GitHub repository.
    /// </summary>
    public class Asset
    {
        public string browser_download_url { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public string state { get; set; }
        public string content_type { get; set; }
        public int size { get; set; }
        public int download_count { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
