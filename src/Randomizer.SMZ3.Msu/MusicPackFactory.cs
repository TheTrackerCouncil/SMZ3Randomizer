using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using static System.Net.Mime.MediaTypeNames;

namespace Randomizer.SMZ3.Msu
{
    public class MusicPackFactory
    {
        private readonly ILogger<MusicPackFactory> _logger;

        public MusicPackFactory(ILogger<MusicPackFactory> logger)
        {
            _logger = logger;
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
                .GroupBy(x => x.TrackNumber)
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, grouping => (ICollection<PcmTrack>)grouping
                    .OrderBy(x => x.Suffix)
                    .Select(x => new PcmTrack(x.TrackNumber, x.Suffix, x.Path))
                    .ToList());

            var title = Path.GetFileName(directory) ?? baseName;
            return new MusicPack(title, null, tracks);
        }

        private PcmFileName? ParseFileName(string fileName, string baseName)
        {
            var pcmFileName = Path.GetFileNameWithoutExtension(fileName);
            var trackIdentifier = pcmFileName.Replace(baseName, "").TrimStart('-');

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
