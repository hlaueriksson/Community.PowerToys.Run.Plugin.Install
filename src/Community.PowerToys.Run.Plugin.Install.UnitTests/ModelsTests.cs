using Community.PowerToys.Run.Plugin.Install.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.Install.UnitTests
{
    [TestClass]
    public class ModelsTests
    {
        [TestMethod]
        public void Metadata_ToolTipText_should_be_valid()
        {
            new Metadata().ToolTipText.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Pair_IsInstalled_should_consider_Metadata()
        {
            new Pair().IsInstalled.Should().BeFalse();
            new Pair { Metadata = new() }.IsInstalled.Should().BeTrue();
        }

        [TestMethod]
        public void Pair_IsNotInstalled_should_consider_Metadata()
        {
            new Pair().IsNotInstalled.Should().BeTrue();
            new Pair { Metadata = new() }.IsNotInstalled.Should().BeFalse();
        }

        [TestMethod]
        public void Pair_IsInstallAvailable_should_validate_Asset_and_Metadata()
        {
            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        Assets = [new() { Name = "Test-1.0.0-x64.zip" }],
                    },
                },
                Metadata = null,
            }.IsInstallAvailable.Should().BeTrue();

            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        Assets = [],
                    },
                },
                Metadata = null,
            }.IsInstallAvailable.Should().BeFalse();

            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        Assets = [new() { Name = "Test-1.0.0-x64.zip" }],
                    },
                },
                Metadata = new(),
            }.IsInstallAvailable.Should().BeFalse();

            new Pair().IsInstallAvailable.Should().BeFalse();
        }

        [TestMethod]
        public void Pair_IsUpdateAvailable_should_compare_versions()
        {
            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        TagName = "v1.1.0",
                        Assets = [new() { Name = "Test-1.1.0-x64.zip" }],
                    },
                },
                Metadata = new()
                {
                    Version = "1.0.0",
                }
            }.IsUpdateAvailable.Should().BeTrue();

            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        TagName = "1.1.0",
                        Assets = [new() { Name = "Test-1.1.0-x64.zip" }],
                    },
                },
                Metadata = new()
                {
                    Version = "1.0.0",
                }
            }.IsUpdateAvailable.Should().BeTrue();

            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        TagName = "v1.0.0",
                        Assets = [new() { Name = "Test-1.0.0-x64.zip" }],
                    },
                },
                Metadata = new()
                {
                    Version = "foo",
                }
            }.IsUpdateAvailable.Should().BeTrue();

            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        TagName = "v1.1.0",
                        Assets = [],
                    },
                },
                Metadata = new()
                {
                    Version = "1.0.0",
                }
            }.IsUpdateAvailable.Should().BeFalse();

            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        TagName = "v1.0.0",
                        Assets = [new() { Name = "Test-1.0.0-x64.zip" }],
                    },
                },
                Metadata = new()
                {
                    Version = "1.0.0",
                }
            }.IsUpdateAvailable.Should().BeFalse();

            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        TagName = "foo",
                        Assets = [new() { Name = "Test.zip" }],
                    },
                },
                Metadata = new()
                {
                    Version = "1.0.0",
                }
            }.IsUpdateAvailable.Should().BeFalse();

            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        TagName = "foo",
                        Assets = [new() { Name = "Test.zip" }],
                    },
                },
                Metadata = new()
                {
                    Version = "bar",
                }
            }.IsUpdateAvailable.Should().BeFalse();

            new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        TagName = "",
                        Assets = [new() { Name = "Test.zip" }],
                    },
                },
                Metadata = new()
                {
                    Version = "",
                }
            }.IsUpdateAvailable.Should().BeFalse();

            new Pair().IsUpdateAvailable.Should().BeFalse();
        }

        [TestMethod]
        public void Pair_HasRepository_should_validate_Repository_Url()
        {
            new Pair { Plugin = new() { Repository = new() { Url = "https://github.com/hlaueriksson/GEmojiSharp" } } }.HasRepository.Should().BeTrue();
            new Pair().HasRepository.Should().BeFalse();
        }

        [TestMethod]
        public void Pair_HasReleaseNotes_should_validate_Repository_Url()
        {
            new Pair { Plugin = new() { Release = new() { Url = "https://github.com/hlaueriksson/GEmojiSharp/releases/tag/v4.0.0" } } }.HasReleaseNotes.Should().BeTrue();
            new Pair().HasReleaseNotes.Should().BeFalse();
        }

        [TestMethod]
        public void Pair_Queryable_should_be_valid()
        {
            new Pair().Queryable.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Pair_ToolTipTitle_should_be_valid()
        {
            new Pair().ToolTipTitle.Should().BeEmpty();
        }

        [TestMethod]
        public void Pair_ToolTipText_should_be_valid()
        {
            new Pair().ToolTipText.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Release_Asset_should_be_valid()
        {
            new Release { Assets = [new() { Name = "GEmojiSharp.PowerToysRun-4.0.0-x64.zip" }] }.Asset.Should().NotBeNull();
            new Release { Assets = [new() { Name = "GEmojiSharp.PowerToysRun-4.0.0.zip" }] }.Asset.Should().NotBeNull();
            new Release { Assets = [new() { Name = "GEmojiSharp.PowerToysRun-4.0.0-x64.zip" }, new() { Name = "GEmojiSharp.PowerToysRun-4.0.0-arm64.zip" }] }.Asset.Should().NotBeNull();
            new Release { Assets = [new() { Name = "GEmojiSharp.PowerToysRun-4.0.0-arm64.zip" }] }.Asset.Should().BeNull();
            new Release { Assets = [new()] }.Asset.Should().BeNull();
            new Release { Assets = [] }.Asset.Should().BeNull();
            new Release().Asset.Should().BeNull();
        }

        [TestMethod]
        public void Release_ToolTipText_should_be_valid()
        {
            new Release().ToolTipText.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Asset_ToolTipText_should_be_valid()
        {
            new Asset().ToolTipText.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Repository_ToolTipText_should_be_valid()
        {
            new Repository().ToolTipText.Should().NotBeEmpty();
        }
    }
}
