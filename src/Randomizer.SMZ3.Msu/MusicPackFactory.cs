using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using YamlDotNet.Serialization;

namespace Randomizer.SMZ3.Msu
{
    public class MusicPackFactory
    {
        private readonly ILogger<MusicPackFactory> _logger;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        public MusicPackFactory(ILogger<MusicPackFactory> logger)
        {
            _logger = logger;
            _serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .Build();
            _deserializer = new DeserializerBuilder()
                .Build();
        }

        public IEnumerable<MusicPack> AutoDetectAll(string msuPackPath)
        {
            return Directory.EnumerateFiles(msuPackPath, "*.msu", SearchOption.AllDirectories)
                .Select(AutoDetect)
                .Where(x => x.Tracks.Count > 0);
        }

        public async Task<MusicPack> LoadAsync(string path,
            CancellationToken cancellationToken = default)
        {
            var yml = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            var pack = _deserializer.Deserialize<MusicPack>(yml);
            pack.FileName = Path.GetFullPath(path);
            return pack;
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
                .Select(x => new PcmTrack(Path.GetRelativePath(directory, x.Path))
                {
                    TrackNumber = x.TrackNumber,
                    Title = x.Suffix,
                    IsDefault = string.IsNullOrEmpty(x.Suffix)
                })
                .OrderBy(x => x.TrackNumber)
                .ThenBy(x => x.Title)
                .ToList();

            var title = Path.GetFileName(directory) ?? baseName;
            return CreateMusicPack(msuFileName, title, null, tracks);
        }

        public async Task SaveAsync(MusicPack pack, string fileName,
            CancellationToken cancellationToken = default)
        {
            var yml = _serializer.Serialize(pack);
            await File.WriteAllTextAsync(fileName, yml, cancellationToken);
        }

        private static MusicPack CreateMusicPack(string msuFileName, string title, string? author, IEnumerable<PcmTrack> tracks)
        {
            var trackNumbers = tracks.Select(x => x.TrackNumber).ToList();

            if (trackNumbers.All(x => SuperMetroidMusicPack.IsValidTrackNumber(x)))
                return new SuperMetroidMusicPack(msuFileName, title, author, tracks);

            if (trackNumbers.All(x => ALinkToThePastMusicPack.IsValidTrackNumber(x)))
                return new ALinkToThePastMusicPack(msuFileName, title, author, tracks);

            if (trackNumbers.All(x => Smz3MusicPack.IsValidTrackNumber(x)))
                return new Smz3MusicPack(msuFileName, title, author, tracks);

            return new MusicPack(msuFileName, title, author, tracks);
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
