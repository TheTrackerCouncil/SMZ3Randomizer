using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Randomizer.Data.Options;
using Randomizer.SMZ3.Contracts;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Randomizer.SMZ3.Generation
{
    public class PlandoConfigLoader : IPlandoConfigLoader
    {
        public async Task<PlandoConfig> LoadAsync(string path,
            CancellationToken cancellationToken = default)
        {
            var serializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            var yaml = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            var config = serializer.Deserialize<PlandoConfig>(yaml);
            config.FileName = Path.GetFileNameWithoutExtension(path);
            return config;
        }
    }
}
