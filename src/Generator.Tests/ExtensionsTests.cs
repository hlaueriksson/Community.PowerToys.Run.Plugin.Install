using FluentAssertions;

namespace Generator.Tests
{
    public class ExtensionsTests
    {
        [Test]
        public void GetGitHubRepositoryOptions_should_parse_Website()
        {
            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".GetGitHubRepositoryOptions().Should()
                .BeEquivalentTo(new GitHubRepositoryOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugin.Install" });

            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install/".GetGitHubRepositoryOptions().Should()
                .BeEquivalentTo(new GitHubRepositoryOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugin.Install" });

            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugins?tab=readme-ov-file#bang".GetGitHubRepositoryOptions().Should()
                .BeEquivalentTo(new GitHubRepositoryOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugins" });

            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugins#bang".GetGitHubRepositoryOptions().Should()
                .BeEquivalentTo(new GitHubRepositoryOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugins" });

            Action act = () => "http://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".GetGitHubRepositoryOptions();
            act.Should().Throw<ArgumentException>();

            act = () => "https://github.com/hlaueriksson/".GetGitHubRepositoryOptions();
            act.Should().Throw<ArgumentException>();

            act = () => "https://github.com/hlaueriksson".GetGitHubRepositoryOptions();
            act.Should().Throw<ArgumentException>();

            act = () => "https://github.com/".GetGitHubRepositoryOptions();
            act.Should().Throw<ArgumentException>();

            act = () => "https://github.com".GetGitHubRepositoryOptions();
            act.Should().Throw<ArgumentException>();

            act = () => "https://gitfail.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".GetGitHubRepositoryOptions();
            act.Should().Throw<ArgumentException>();

            act = () => "".GetGitHubRepositoryOptions();
            act.Should().Throw<ArgumentException>();

            act = () => ((string)null!).GetGitHubRepositoryOptions();
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Map_Repository_should_return_model()
        {
            new Repository
            {
                html_url = "Url",
                owner = new Owner { login = "Owner" },
                name = "Name",
                full_name = "FullName",
                description = "Description",
                topics = ["Topics"],
                license = new License { name = "License" },
                stargazers_count = 1,
                watchers_count = 2,
                forks_count = 3,
                size = 4,
                created_at = DateTime.Today.AddDays(-1),
                updated_at = DateTime.Today,
            }.Map().Should().BeEquivalentTo(new Models.Repository
            {
                Url = "Url",
                Owner = "Owner",
                Name = "Name",
                FullName = "FullName",
                Description = "Description",
                Topics = ["Topics"],
                License = "License",
                StargazersCount = 1,
                WatchersCount = 2,
                ForksCount = 3,
                Size = 4,
                CreatedAt = DateTime.Today.AddDays(-1),
                UpdatedAt = DateTime.Today,
            }, options => options.ExcludingMissingMembers());

            new Repository().Map().Should().NotBeNull();

            ((Repository)null!).Map().Should().BeNull();
        }

        [Test]
        public void Map_Release_should_return_model()
        {
            new Release
            {
                html_url = "Url",
                tag_name = "TagName",
                name = "Name",
                created_at = DateTime.Today.AddDays(-1),
                published_at = DateTime.Today,
                assets =
                [
                    new Asset
                    {
                        browser_download_url = "Url",
                        name = "Name.zip",
                        size = 1,
                        download_count = 2,
                        created_at = DateTime.Today.AddDays(-1),
                        updated_at = DateTime.Today,
                    }
                ]
            }.Map(new Models.Plugin { Name = "Name" }).Should().BeEquivalentTo(new Models.Release
            {
                Url = "Url",
                TagName = "TagName",
                Name = "Name",
                CreatedAt = DateTime.Today.AddDays(-1),
                PublishedAt = DateTime.Today,
                Assets =
                [
                    new Models.Asset
                    {
                        Url = "Url",
                        Name = "Name.zip",
                        Size = 1,
                        DownloadCount = 2,
                        CreatedAt = DateTime.Today.AddDays(-1),
                        UpdatedAt = DateTime.Today,
                    }
                ]
            }, options => options.ExcludingMissingMembers());

            new Release
            {
                assets =
                [
                    new Asset { name = "Foo.zip" },
                    new Asset { name = "Bar.zip" },
                ]
            }.Map(new Models.Plugin { Name = "Name" }).Assets.Should().BeEquivalentTo(
                [
                    new Models.Asset { Name = "Foo.zip" },
                    new Models.Asset { Name = "Bar.zip" }
                ]);

            new Release
            {
                assets =
                [
                    new Asset { name = "Foo.zip" },
                    new Asset { name = "Bar.zip" },
                    new Asset { name = "Name-1.0.0-x64.zip" },
                    new Asset { name = "Name-1.0.0-arm64.zip" },
                ]
            }.Map(new Models.Plugin { Name = "Name" }).Assets.Should().BeEquivalentTo(
                [
                    new Models.Asset { Name = "Name-1.0.0-x64.zip" },
                    new Models.Asset { Name = "Name-1.0.0-arm64.zip" }
                ]);

            new Release().Map(new Models.Plugin()).Should().NotBeNull();

            ((Release)null!).Map(new Models.Plugin()).Should().BeNull();
        }
    }
}
