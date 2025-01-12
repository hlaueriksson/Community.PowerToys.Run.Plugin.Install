using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Generator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Generator.Tests
{
    [Explicit, Category("Integration")]
    public class IntegrationTests
    {
        [Test]
        public async Task Auth()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await page.GotoAsync("https://github.com");

            // Sign in
            await page.WaitForSelectorAsync(".AppHeader-user", new() { Timeout = 120_000 });

            await context.StorageStateAsync(new()
            {
                Path = "auth.json"
            });
        }

        [Test]
        public async Task awesome_json_vs_awesome_list()
        {
            var awesome = JsonSerializer.Deserialize<Awesome>(File.ReadAllText(@"..\..\..\..\..\awesome.json"));

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync(new()
            {
                StorageStatePath = "auth.json"
            });
            var page = await context.NewPageAsync();
            await page.GotoAsync("https://github.com/hlaueriksson/awesome-powertoys-run-plugins");
            var items = await page.Locator("article > ul:nth-child(12) > li").AllAsync();

            var result = new List<string>();

            foreach (var item in items)
            {
                var link = item.Locator("a");
                var name = await link.InnerTextAsync();
                var url = await link.GetAttributeAsync("href");
                var description = await item.InnerTextAsync();

                var plugin = awesome.Plugins.SingleOrDefault(x => x.Name == name);

                if (plugin == null)
                {
                    result.Add($"Plugin \"{name}\" not found");
                    continue;
                }
                if (!description.EndsWith(plugin.Description, StringComparison.Ordinal))
                {
                    result.Add($"Plugin \"{name}\" description: {description} != {plugin.Description}");
                }
                if (!url.Contains(plugin.Author, StringComparison.Ordinal))
                {
                    result.Add($"Plugin \"{name}\" author: {url} != {plugin.Author}");
                }
                if (!url.StartsWith(plugin.Website, StringComparison.Ordinal))
                {
                    result.Add($"Plugin \"{name}\" URL: {url} != {plugin.Website}");
                }

                var repo = plugin.Website.Replace("https://github.com/", string.Empty);
                var q = Uri.EscapeDataString($"repo:{repo} {plugin.ID}");
                var searchPage = await context.NewPageAsync();
                await searchPage.GotoAsync($"https://github.com/search?q={q}&type=code");
                var count = await searchPage.GetByTestId("results-list").Locator("> div").CountAsync();
                if (count < 2)
                {
                    result.Add($"Plugin \"{name}\" ID not found: {plugin.ID}");
                }
                await searchPage.CloseAsync();
            }

            foreach (var message in result)
            {
                Console.WriteLine(message);
            }

            result.Should().BeEmpty();
        }

        [Test]
        public void awesome_json_lint()
        {
            var awesome = JsonSerializer.Deserialize<Awesome>(File.ReadAllText(@"..\..\..\..\..\awesome.json"));

            var result = new List<string>();

            foreach (var plugin in awesome.Plugins)
            {
                var (ExitCode, StandardOutput, StandardError) = Lint(plugin.Website);

                if (ExitCode != 0)
                {
                    result.Add($"Plugin \"{plugin.Name}\": {ExitCode}\n{StandardOutput}\n{StandardError}");
                }
            }

            foreach (var message in result)
            {
                Console.WriteLine(message);
            }

            result.Should().BeEmpty();
        }

        [Test]
        public async Task awesome_json_VirusTotal()
        {
            // dotnet run --project ./src/Generator -- "awesome.json" "./.pages/awesome.json" "github_pat_"

            var config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .Build();

            var awesome = JsonSerializer.Deserialize<Awesome>(File.ReadAllText(@"..\..\..\..\..\.pages\awesome.json"));

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("accept", "application/json");
            client.DefaultRequestHeaders.Add("x-apikey", config.GetValue<string>("IntegrationTests:VirusTotalApiKey"));
            var url = "https://www.virustotal.com/api/v3/urls";

            var result = new List<string>();

            foreach (var plugin in awesome.Plugins)
            {
                foreach (var asset in plugin.Release.Assets)
                {
                    using var content = new MultipartFormDataContent
                    {
                        { new StringContent(asset.Url), "url" }
                    };
                    var response = await client.PostAsync(url, content);
                    var scan = await response.Content.ReadFromJsonAsync<VirusTotalScanResponse>();

                    VirusTotalAnalysisResponse analysis = null;
                    while (analysis?.data.attributes.status != "completed")
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        response = await client.GetAsync(scan.data.links.self);
                        analysis = await response.Content.ReadFromJsonAsync<VirusTotalAnalysisResponse>();
                    }

                    if (analysis.data.attributes.stats.malicious > 0)
                    {
                        result.Add($"Plugin \"{plugin.Name}\" Asset \"{asset.Name}\": " + JsonSerializer.Serialize(analysis.data.attributes.stats));
                    }
                }
            }

            foreach (var message in result)
            {
                Console.WriteLine(message);
            }

            result.Should().BeEmpty();
        }

        static (int ExitCode, string StandardOutput, string StandardError) Lint(string url)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ptrun-lint",
                Arguments = url,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            using var exeProcess = Process.Start(startInfo);
            exeProcess!.WaitForExit();
            return new(exeProcess.ExitCode, exeProcess.StandardOutput.ReadToEnd(), exeProcess.StandardError.ReadToEnd());
        }
    }

    file class VirusTotalScanResponse
    {
        public Data data { get; set; }
    }

    file class VirusTotalAnalysisResponse
    {
        public Data data { get; set; }
        public Meta meta { get; set; }
    }

    file class Data
    {
        public string id { get; set; }
        public string type { get; set; }
        public Links links { get; set; }
        public Attributes attributes { get; set; }
    }

    file class Links
    {
        public string self { get; set; }
        public string item { get; set; }
    }

    file class Attributes
    {
        public Dictionary<string, Result> results { get; set; }
        public Stats stats { get; set; }
        public int date { get; set; }
        public string status { get; set; }
    }

    file class Result
    {
        public string method { get; set; }
        public string engine_name { get; set; }
        public string category { get; set; }
        public string result { get; set; }
    }

    file class Stats
    {
        public int malicious { get; set; }
        public int suspicious { get; set; }
        public int undetected { get; set; }
        public int harmless { get; set; }
        public int timeout { get; set; }
    }

    file class Meta
    {
        public UrlInfo url_info { get; set; }
        public FileInfo file_info { get; set; }
    }

    file class UrlInfo
    {
        public string id { get; set; }
        public string url { get; set; }
    }

    file class FileInfo
    {
        public string sha256 { get; set; }
    }
}
