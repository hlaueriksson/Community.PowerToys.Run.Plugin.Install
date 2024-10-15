using System.Text.Json;
using Generator;
using Generator.Models;

var input = args[0];
var output = args[1];
var pat = args[2];

Console.WriteLine("Generating awesome...");

var content = await input.GetContentAsync();
var awesome = JsonSerializer.Deserialize<Awesome>(content) ?? throw new Exception("Failed to deserialize awesome");

var client = new GitHubClient(new GitHubOptions { PersonalAccessToken = pat });

foreach (var plugin in awesome.Plugins)
{
    Console.WriteLine(plugin.Name);

    var options = plugin.Website.GetGitHubRepositoryOptions();

    var repository = await client.GetRepositoryAsync(options);
    plugin.Repository = repository?.Map();

    var release = await client.GetLatestReleaseAsync(options);
    plugin.Release = release?.Map(plugin);

    foreach (var error in plugin.Validate())
    {
        Console.WriteLine($" {error}");
    }
}

content = JsonSerializer.Serialize(awesome);
output.EnsureDirectoryExists();
await File.WriteAllTextAsync(output, content);
