using Community.PowerToys.Run.Plugin.Install.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Install.UnitTests
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void Map_PluginMetadata_should_return_model()
        {
            new PluginMetadata
            {
                ID = "",
                Name = "Name",
                Version = "Version",
                IcoPathDark = "IcoPathDark",
                IcoPathLight = "IcoPathLight",
            }.Map().Should().BeEquivalentTo(new Metadata
            {
                ID = "",
                Name = "Name",
                Version = "Version",
                IcoPathDark = "IcoPathDark",
                IcoPathLight = "IcoPathLight",
            });

            new PluginMetadata().Map().Should().NotBeNull();

            Action act = () => ((PluginMetadata)null!).Map();
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Score_Pair_should_return_FuzzyMatch_Score()
        {
            var subject = new Pair
            {
                Plugin = new()
                {
                    Name = "Name",
                    Description = "Description",
                    Author = "Author",
                    Release = new Release
                    {
                        TagName = "v1.1.0",
                        Assets = [new() { Name = "Test-1.1.0-x64.zip" }],
                    },
                },
                Metadata = new()
                {
                    Version = "1.0.0",
                }
            };
            var matcher = new StringMatcher();

            subject.Score(matcher, "").Should().Be(1);
            subject.Score(matcher, "name").Should().Be(130);
            subject.Score(matcher, "crip").Should().Be(111);
            subject.Score(matcher, "th").Should().Be(86);
            subject.Score(matcher, "outdated").Should().Be(154);
        }

        [DataTestMethod]
        [DataRow(-1, "-1 bytes")]
        [DataRow(0, "0 bytes")]
        [DataRow(1, "1 bytes")]
        [DataRow(1024, "1 KB")]
        [DataRow(1024 * 1024, "1 MB")]
        [DataRow(1024 * 1024 * 1024, "1 GB")]
        [DataRow(int.MaxValue, "2 GB")]
        public void FormatSize_should_format_file_size(int bytes, string expected)
        {
            bytes.FormatSize().Should().Be(expected);
        }
    }
}
