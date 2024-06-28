using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GitHubReleaseChecker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Configuration;

namespace TrackerCouncil.Smz3.Tests;

using Xunit;

public class MetaTests
{
    [Fact]
    public async Task ValidateVersionNumber()
    {
        // Get csproj file for TrackerCouncil.Smz3.UI
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }
        var projectPath = Path.Combine(directory!.FullName, "src", "TrackerCouncil.Smz3.UI", "TrackerCouncil.Smz3.UI.csproj");

        // Get version from csproj file
        var version = (await File.ReadAllLinesAsync(projectPath))
            .Where(x => x.Trim().StartsWith("<Version>", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Replace("<Version>", "", StringComparison.OrdinalIgnoreCase).Replace("</Version>", "").Trim())
            .First();

        var serviceProvider = new ServiceCollection()
            .AddLogging(x =>
            {
                x.AddConsole();
            })
            .AddConfigs()
            .AddGitHubReleaseCheckerServices()
            .BuildServiceProvider();

        // Get latest version from GitHub and make sure the current version is newer
        var releases = await serviceProvider.GetRequiredService<IGitHubReleaseService>()
            .GetReleasesAsync("TheTrackerCouncil", "SMZ3Randomizer");
        var latestRelease = releases?.FirstOrDefault();
        latestRelease.Should().NotBeNull();
        latestRelease!.Tag.Should().NotBeEquivalentTo($"v{version}");
        serviceProvider.GetRequiredService<IGitHubReleaseCheckerService>()
            .IsCurrentVersionOutOfDate(version, latestRelease.Tag).Should().BeFalse("TrackerCouncil.Smz3.UI version of {0} is not greater than GitHub version of {1}", version, latestRelease.Tag);
    }
}
