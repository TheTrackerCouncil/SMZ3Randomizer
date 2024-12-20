using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.Data.Services;

/// <summary>
/// Service for loading tracker speech sprites
/// </summary>
/// <param name="optionsFactory"></param>
public class TrackerSpriteService(OptionsFactory optionsFactory)
{
    private List<TrackerSpeechImagePack> _packs = [];

    public void LoadSprites()
    {
        _packs = [];
        var path = RandomizerDirectories.TrackerSpritePath;
        var packFolders = Directory.EnumerateDirectories(RandomizerDirectories.TrackerSpritePath);
        foreach (var folder in packFolders)
        {
            var packName = Path.GetFileName(folder);

            if (string.IsNullOrEmpty(packName) || !File.Exists(Path.Combine(folder, "default_idle.png")) ||
                !File.Exists(Path.Combine(folder, "default_talk.png")))
            {
                continue;
            }

            Dictionary<string, TrackerSpeechReactionImages> _reactions = [];

            foreach (var idleImage in Directory.GetFiles(folder, "*_idle.png"))
            {
                var talkingImage = idleImage.Replace("_idle.png", "_talk.png");
                if (!File.Exists(talkingImage))
                {
                    continue;
                }

                var reactionName = Path.GetFileName(idleImage).Replace("_idle.png", "").ToLower();
                _reactions[reactionName] = new TrackerSpeechReactionImages
                {
                    IdleImage = idleImage, TalkingImage = talkingImage,
                };
            }

            _packs.Add(new TrackerSpeechImagePack
            {
                Name = packName,
                Default = new TrackerSpeechReactionImages
                {
                    IdleImage = Path.Combine(folder, "default_idle.png"),
                    TalkingImage = Path.Combine(folder, "default_talk.png"),
                },
                Reactions = _reactions
            });
        }
    }

    public Dictionary<string, string> GetPackOptions()
    {
        return _packs.OrderByDescending(x => x.Name == "Default")
            .ThenByDescending(x => x.Name == "Vanilla")
            .ThenBy(x => x.Name)
            .ToDictionary(x => x.Name, x => x.Name);
    }

    public TrackerSpeechImagePack GetPack(string? packName = null)
    {
        packName ??= optionsFactory.Create().GeneralOptions.TrackerSpeechImagePack;
        return _packs.FirstOrDefault(x => x.Name == packName) ??
               _packs.FirstOrDefault(x => x.Name == "Default") ?? _packs.First();
    }
}
