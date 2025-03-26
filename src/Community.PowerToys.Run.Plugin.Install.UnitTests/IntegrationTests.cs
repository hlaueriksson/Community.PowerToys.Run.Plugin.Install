using System.Text.Json;
using Community.PowerToys.Run.Plugin.Install.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.Install.UnitTests
{
    [TestClass]
    [Ignore, TestCategory("Integration")]
    public class IntegrationTests
    {
        [TestMethod]
        public async Task Awesome_Plugins_Release_Asset()
        {
            var url = "https://install.ptrun.net/awesome.json";
            var content = url.GetContent();
            var awesomePlugins = JsonSerializer.Deserialize<Awesome>(content);

            foreach (var plugin in awesomePlugins.Plugins)
            {
                Console.WriteLine($"{plugin.Name} {plugin.Repository.Url}");
                plugin.Release.Asset.Should().NotBeNull();
            }
        }
    }
}
