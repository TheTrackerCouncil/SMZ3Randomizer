using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using GitHubReleaseChecker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSURandomizerLibrary;
using Randomizer.Data.Configuration;
using Randomizer.Data.Services;
using Randomizer.Sprites;

namespace Randomizer.SMZ3.Tests;

using Xunit;

public class MetaTests
{
    [Fact]
    public void ValidateVersionNumber()
    {
        // Get csproj file for Randomizer.App
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }
        var projectPath = Path.Combine(directory!.FullName, "src", "Randomizer.App", "Randomizer.App.csproj");

        // Get version from csproj file
        var version = File.ReadAllLines(projectPath)
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
        var latestRelease = serviceProvider.GetRequiredService<IGitHubReleaseService>().GetReleases("Vivelin", "SMZ3Randomizer")?.FirstOrDefault();
        latestRelease.Should().NotBeNull();
        latestRelease!.Tag.Should().NotBeEquivalentTo($"v{version}");
        serviceProvider.GetRequiredService<IGitHubReleaseCheckerService>()
            .IsCurrentVersionOutOfDate(version, latestRelease.Tag).Should().BeFalse("Randomizer.App version of {0} is not greater than GitHub version of {1}", version, latestRelease.Tag);
    }
}
