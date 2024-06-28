using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

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
