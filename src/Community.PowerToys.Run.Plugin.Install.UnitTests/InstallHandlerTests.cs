using Community.PowerToys.Run.Plugin.Abstractions.Infrastructure;
using Community.PowerToys.Run.Plugin.Abstractions.Plugin;
using Community.PowerToys.Run.Plugin.Install.Models;
using FluentAssertions;
using LazyCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Install.UnitTests
{
    [TestClass]
    public class InstallHandlerTests
    {
        private IAppCache _cache;
        private IHelper _helper;
        private InstallHandler _subject;

        [TestInitialize]
        public void TestInitialize()
        {
            _cache = Substitute.For<IAppCache>();
            _helper = Substitute.For<IHelper>();
            _subject = new InstallHandler(new() { PluginSource = @"..\..\..\..\..\..\awesome.json" }, _cache, new PluginMetadata(), Substitute.For<IPublicAPI>(), _helper, Substitute.For<IDefaultBrowserInfo>(), Substitute.For<ILog>(), new StringMatcher());
        }

        [TestMethod]
        public void Init_should_add_cache()
        {
            var api = Substitute.For<IPublicAPI>();
            api.GetAllPlugins().Returns([]);
            var context = new PluginInitContext
            {
                API = api,
            };

            _subject.Init(context);

            _cache.ReceivedWithAnyArgs().Add(default, (List<Pair>)default, default);
        }

        [TestMethod]
        public void Query_should_only_return_valid_pairs()
        {
            var pairs = new List<Pair>
            {
                new() { Plugin = new() { Repository = new() { Url = "https://github.com/hlaueriksson/GEmojiSharp" } } },
                new(),
            };
            _cache.Get<List<Pair>>(InstallHandler.CacheKey).Returns(pairs);

            var result = _subject.Query(new Query(""));
            result.Should().HaveCount(1);
        }

        [TestMethod]
        public void Query_should_score_pairs()
        {
            var pairs = new List<Pair>
            {
                new() { Plugin = new() { Repository = new() { Url = "https://github.com/hlaueriksson/GEmojiSharp" } } },
            };
            _cache.Get<List<Pair>>(InstallHandler.CacheKey).Returns(pairs);

            var result = _subject.Query(new Query(""));
            result.First().Score.Should().Be(1);
        }

        [TestMethod]
        public void InstallPlugin_should_run_PowerShell()
        {
            _helper.OpenInShell(default, default, default, Helper.ShellRunAsType.Administrator).ReturnsForAnyArgs(true);
            var pair = new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        Assets =
                        [
                            new()
                            {
                                Url = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip",
                                Name = "GEmojiSharp.PowerToysRun-4.0.0-x64.zip",
                            },
                        ],
                    },
                },
            };

            var result = _subject.InstallPlugin(pair);
            result.Should().BeTrue();
            _helper.ReceivedWithAnyArgs().OpenInShell(default, default, default, Helper.ShellRunAsType.Administrator);

            Action act = () => _subject.InstallPlugin(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void UpdatePlugin_should_run_PowerShell()
        {
            _helper.OpenInShell(default, default, default, Helper.ShellRunAsType.Administrator).ReturnsForAnyArgs(true);
            var pair = new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        Assets =
                        [
                            new()
                            {
                                Url = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip",
                                Name = "GEmojiSharp.PowerToysRun-4.0.0-x64.zip",
                            },
                        ],
                    },
                },
                Metadata = new()
                {
                    PluginDirectory = @"C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp\",
                },
            };

            var result = _subject.UpdatePlugin(pair);
            result.Should().BeTrue();
            _helper.ReceivedWithAnyArgs().OpenInShell(default, default, default, Helper.ShellRunAsType.Administrator);

            Action act = () => _subject.UpdatePlugin(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void UninstallPlugin_should_run_PowerShell()
        {
            _helper.OpenInShell(default, default, default, Helper.ShellRunAsType.Administrator).ReturnsForAnyArgs(true);
            var pair = new Pair
            {
                Metadata = new()
                {
                    PluginDirectory = @"C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp\",
                },
            };

            var result = _subject.UninstallPlugin(pair);
            result.Should().BeTrue();
            _helper.ReceivedWithAnyArgs().OpenInShell(default, default, default, Helper.ShellRunAsType.Administrator);

            Action act = () => _subject.UninstallPlugin(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void OpenWebsite_should_open_browser()
        {
            _helper.OpenCommandInShell(default, default, default).ReturnsForAnyArgs(true);
            var pair = new Pair
            {
                Plugin = new()
                {
                    Website = "https://github.com/hlaueriksson/GEmojiSharp",
                },
            };

            var result = _subject.OpenWebsite(pair);
            result.Should().BeTrue();
            _helper.ReceivedWithAnyArgs().OpenCommandInShell(default, default, default);

            Action act = () => _subject.OpenWebsite(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void OpenRepo_should_open_browser()
        {
            _helper.OpenCommandInShell(default, default, default).ReturnsForAnyArgs(true);
            var pair = new Pair
            {
                Plugin = new()
                {
                    Repository = new()
                    {
                        Url = "https://github.com/hlaueriksson/GEmojiSharp",
                    },
                },
            };

            var result = _subject.OpenRepo(pair);
            result.Should().BeTrue();
            _helper.ReceivedWithAnyArgs().OpenCommandInShell(default, default, default);

            Action act = () => _subject.OpenRepo(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void OpenReleaseNotes_should_open_browser()
        {
            _helper.OpenCommandInShell(default, default, default).ReturnsForAnyArgs(true);
            var pair = new Pair
            {
                Plugin = new()
                {
                    Release = new()
                    {
                        Url = "https://github.com/hlaueriksson/GEmojiSharp/releases/tag/v4.0.0",
                    },
                },
            };

            var result = _subject.OpenReleaseNotes(pair);
            result.Should().BeTrue();
            _helper.ReceivedWithAnyArgs().OpenCommandInShell(default, default, default);

            Action act = () => _subject.OpenReleaseNotes(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void OpenPluginFolder_should_open_FileExplorer()
        {
            _helper.OpenInShell(default, default).ReturnsForAnyArgs(true);
            var pair = new Pair
            {
                Metadata = new()
                {
                    PluginDirectory = @"C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp\",
                },
            };

            var result = _subject.OpenPluginFolder(pair);
            result.Should().BeTrue();
            _helper.ReceivedWithAnyArgs().OpenInShell(default, default);

            Action act = () => _subject.OpenPluginFolder(null);
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
