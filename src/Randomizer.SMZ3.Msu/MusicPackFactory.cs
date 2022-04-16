using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Msu
{
    public class MusicPackFactory
    {
        private readonly ILogger<MusicPackFactory> _logger;

        public MusicPackFactory(ILogger<MusicPackFactory> logger)
        {
            _logger = logger;
        }

        public IEnumerable<MusicPack> AutoDetectAll(string msuPackPath)
        {
            return Directory.EnumerateFiles(msuPackPath, "*.msu", SearchOption.AllDirectories)
                .Select(AutoDetect)
                .Where(x => x.Tracks.Count > 0);
        }

        public MusicPack AutoDetect(string msuFileName)
        {
            var fullPath = Path.GetFullPath(msuFileName);
            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("The path must be a valid file name.", nameof(msuFileName));

            var baseName = Path.GetFileNameWithoutExtension(msuFileName);
            var tracks = Directory.EnumerateFiles(directory, "*.pcm")
                .Select(x => ParseFileName(x, baseName))
                .Where(x => x != null).Cast<PcmFileName>()
                .Select(x => new PcmTrack(x.TrackNumber, x.Suffix, x.Path))
                .OrderBy(x => x.TrackNumber)
                .ThenBy(x => x.Title);

            var title = Path.GetFileName(directory) ?? baseName;
            return CreateMusicPack(title, null, tracks);
        }

        private static MusicPack CreateMusicPack(string title, string? author, IEnumerable<PcmTrack> tracks)
        {
            var trackNumbers = tracks.Select(x => x.TrackNumber).ToList();

            if (trackNumbers.All(x => SuperMetroidMusicPack.IsValidTrackNumber(x)))
                return new SuperMetroidMusicPack(title, author, tracks);

            if (trackNumbers.All(x => ALinkToThePastMusicPack.IsValidTrackNumber(x)))
                return new ALinkToThePastMusicPack(title, author, tracks);

            if (trackNumbers.All(x => Smz3MusicPack.IsValidTrackNumber(x)))
                return new Smz3MusicPack(title, author, tracks);

            return new MusicPack(title, author, tracks);
        }

        private PcmFileName? ParseFileName(string fileName, string baseName)
        {
            var pcmFileName = Path.GetFileNameWithoutExtension(fileName);
            if (!pcmFileName.StartsWith($"{baseName}-"))
                return null;

            var trackIdentifier = pcmFileName.Substring($"{baseName}-".Length);
            var leadingNumberPattern = new Regex(@"^\d+", RegexOptions.Compiled);
            var match = leadingNumberPattern.Match(trackIdentifier);
            if (!match.Success)
            {
                _logger.LogInformation("Could not parse track number in {Name}. Skipping {FileName}.", trackIdentifier, fileName);
                return null;
            }

            var trackNumber = int.Parse(match.Value);
            var suffix = trackIdentifier.Substring(match.Value.Length).Trim('_');
            return new PcmFileName(trackNumber, suffix, fileName);
        }

        private record PcmFileName(int TrackNumber, string Suffix, string Path);
    }
}
