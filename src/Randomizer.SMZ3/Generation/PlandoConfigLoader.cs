using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Generation
{
    public class PlandoConfigLoader : IPlandoConfigLoader
    {
        public async Task<PlandoConfig> LoadAsync(string path,
            CancellationToken cancellationToken = default)
        {
            var serializer = new YamlDotNet.Serialization.Deserializer();
            var yaml = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            var config = serializer.Deserialize<PlandoConfig>(yaml);
            config.FileName = Path.GetFileNameWithoutExtension(path);
            return config;
        }
    }
}
