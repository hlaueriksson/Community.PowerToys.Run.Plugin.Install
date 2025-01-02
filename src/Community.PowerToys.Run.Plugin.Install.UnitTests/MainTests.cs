using Community.PowerToys.Run.Plugin.Install.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Install.UnitTests
{
    [TestClass]
    public class MainTests
    {
        private IInstallHandler _handler;
        private Main _subject;

        [TestInitialize]
        public void TestInitialize()
        {
            _handler = Substitute.For<IInstallHandler>();
            _subject = new Main(new(), _handler);
        }

        [TestMethod]
        public void Query_should_return_actions()
        {
            var query = new Query("");
            _handler.Actions(query).Returns([ActionType.Validate, ActionType.Reload]);

            var results = _subject.Query(query);

            results.Count.Should().Be(2);
        }

        [TestMethod]
        public void Query_should_return_results()
        {
            var query = new Query("");
            _handler.Query(query).Returns([new Pair { Plugin = new() { Name = "Name", Description = "Description" } }]);

            var results = _subject.Query(query);

            results.Count.Should().Be(1);
        }

        [TestMethod]
        public void LoadContextMenus_should_return_results()
        {
            var results = _subject.LoadContextMenus(new Result { ContextData = new Pair() });
            results.Should().BeEmpty();

            results = _subject.LoadContextMenus(new Result
            {
                ContextData = new Pair
                {
                    Plugin = new()
                    {
                        Release = new() { TagName = "v1.0.0", Assets = [new() { Name = "Test-1.0.0-x64.zip" }] },
                    },
                }
            });
            results.Select(x => x.Title).Should().BeEquivalentTo(["Install plugin (Ctrl+Enter)"]);

            results = _subject.LoadContextMenus(new Result
            {
                ContextData = new Pair
                {
                    Plugin = new()
                    {
                        Release = new() { TagName = "v1.1.0", Assets = [new() { Name = "Test-1.1.0-x64.zip" }] },
                    },
                    Metadata = new() { Version = "1.0.0" }
                }
            });
            results.Select(x => x.Title).Should().BeEquivalentTo([
                "Update plugin (Ctrl+Enter)",
                "Uninstall plugin (Ctrl+Del)",
                "Open plugin folder (Ctrl+F)"]);

            results = _subject.LoadContextMenus(new Result
            {
                ContextData = new Pair
                {
                    Plugin = new()
                    {
                        Repository = new() { Url = "https://github.com/hlaueriksson/GEmojiSharp" },
                    },
                }
            });
            results.Select(x => x.Title).Should().BeEquivalentTo(["Open repo (Enter)"]);

            results = _subject.LoadContextMenus(new Result
            {
                ContextData = new Pair
                {
                    Plugin = new()
                    {
                        Release = new() { Url = "https://github.com/hlaueriksson/GEmojiSharp/releases/tag/v4.0.1" },
                    },
                }
            });
            results.Select(x => x.Title).Should().BeEquivalentTo(["Open release notes (Ctrl+N)"]);
        }
    }
}
