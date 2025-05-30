using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TrackerCouncil.Smz3.Data.Services;

/// <summary>
/// Service for loading tracker speech sprites
/// </summary>
/// <param name="optionsFactory"></param>
/// <param name="logger"></param>
public class TrackerSpriteService(OptionsFactory optionsFactory, ILogger<TrackerSpriteService> logger)
{
    private List<TrackerSpeechImagePack> _packs = [];

    public void LoadSprites()
    {
        InitUserTrackerSpriteFolder();
        _packs = [];
        LoadFolderPacks(Directories.TrackerSpritePath, false);
        LoadFolderPacks(Directories.UserTrackerSpritePath, true);
        logger.LogInformation("{Count} Tracker Sprite Packs", _packs.Count);
    }

    private void InitUserTrackerSpriteFolder()
    {
        if (Directory.Exists(Directories.UserTrackerSpritePath))
        {
            return;
        }

        Directory.CreateDirectory(Directories.UserTrackerSpritePath);
        logger.LogInformation("Created user tracker sprite folder {Path}", Directories.UserTrackerSpritePath);

        var deprecratedDirectory = Path.Combine(Directories.AppDataFolder, "TrackerSprites");

        if (!Directory.Exists(deprecratedDirectory))
        {
            return;
        }

        foreach (var oldPath in Directory.GetDirectories(deprecratedDirectory))
        {
            var name = new DirectoryInfo(oldPath).Name;
            var newPath = Path.Combine(Directories.UserTrackerSpritePath, name);
            Directory.Move(oldPath, newPath);
            logger.LogInformation("Migrated user tracker sprite folder {Name} from {Old} to {New}", name, oldPath, newPath);
        }
    }

    public Dictionary<string, string> GetPackOptions()
    {
        return _packs.OrderByDescending(x => x.Name == "Default")
            .ThenByDescending(x => x.Name == "Vanilla")
            .ThenBy(x => x.Name)
            .ToDictionary(x => x.Name, x => x.Name);
    }

    public TrackerSpeechImagePack? GetPack(string? packName = null)
    {
        packName ??= optionsFactory.Create().GeneralOptions.TrackerSpeechImagePack;
        return _packs.FirstOrDefault(x => x.Name == packName) ??
               _packs.FirstOrDefault(x => x.Name == "Default") ?? _packs.FirstOrDefault();
    }

    private void LoadFolderPacks(string folder, bool isUserPack)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        var packFolders = Directory.EnumerateDirectories(folder);
        foreach (var packFolder in packFolders)
        {
            LoadPack(packFolder, isUserPack);
        }
    }

    private void LoadPack(string folder, bool isUserPack)
    {
        var packName = Path.GetFileName(folder);

        if (string.IsNullOrEmpty(packName) || !File.Exists(Path.Combine(folder, "default_idle.png")) ||
            !File.Exists(Path.Combine(folder, "default_talk.png")))
        {
            return;
        }

        Dictionary<string, TrackerSpeechReactionImages> reactions = [];

        foreach (var idleImage in Directory.GetFiles(folder, "*_idle.png"))
        {
            var talkingImage = idleImage.Replace("_idle.png", "_talk.png");
            if (!File.Exists(talkingImage))
            {
                continue;
            }

            var reactionName = Path.GetFileName(idleImage).Replace("_idle.png", "").ToLower();
            reactions[reactionName] = new TrackerSpeechReactionImages
            {
                IdleImage = idleImage, TalkingImage = talkingImage,
            };
        }

        if (_packs.Any(x => x.Name == packName) && isUserPack)
        {
            packName += " (Custom)";
        }

        var config = GetConfig(Path.Combine(folder, "config.yml"))
                     ?? GetConfig(Path.Combine(folder, "config.yaml"));

        _packs.Add(new TrackerSpeechImagePack
        {
            Name = packName,
            Default = new TrackerSpeechReactionImages
            {
                IdleImage = Path.Combine(folder, "default_idle.png"),
                TalkingImage = Path.Combine(folder, "default_talk.png"),
            },
            Reactions = reactions,
            ProfileConfig = config,
        });
    }

    private TrackerProfileConfig? GetConfig(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var yml = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        try
        {
            return deserializer.Deserialize<TrackerProfileConfig>(yml);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to parse tracker profile config {Path}", path);
            return null;
        }
    }
}
