using FluentAssertions;

namespace Generator.Tests
{
    public class GitHubClientTests
    {
        private GitHubClient _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = new GitHubClient(new GitHubOptions());
        }

        [Test]
        public async Task GetRepositoryAsync_should_return_repo()
        {
            var options = new GitHubRepositoryOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugin.Update" };

            var result = await _subject.GetRepositoryAsync(options);

            result.Should().NotBeNull();
        }

        [Test]
        public async Task GetLatestReleaseAsync_should_return_latest_release()
        {
            var options = new GitHubRepositoryOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugin.Update" };

            var result = await _subject.GetLatestReleaseAsync(options);

            result.Should().NotBeNull();
        }

        [Test]
        public async Task GetLatestReleaseAsync_should_throw_if_Owner_is_invalid()
        {
            var options = new GitHubRepositoryOptions { Owner = "userthatdoesnotexist", Repo = "Community.PowerToys.Run.Plugin.Update" };

            Func<Task> act = () => _subject.GetLatestReleaseAsync(options);
            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Test]
        public async Task GetLatestReleaseAsync_should_throw_if_Repo_is_invalid()
        {
            var options = new GitHubRepositoryOptions { Owner = "hlaueriksson", Repo = "repothatdoesnotexist" };

            Func<Task> act = () => _subject.GetLatestReleaseAsync(options);
            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
