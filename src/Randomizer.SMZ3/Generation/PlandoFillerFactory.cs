using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Generation
{
    public class PlandoFillerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public PlandoFillerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public async Task<PlandoFiller> CreateFromFileAsync(string path,
            CancellationToken cancellationToken = default)
        {
            var plandoConfig = await LoadPlandoConfigAsync(path, cancellationToken);
            var logger = _loggerFactory.CreateLogger<PlandoFiller>();
            return new PlandoFiller(plandoConfig, logger);
        }

        private static async Task<PlandoConfig> LoadPlandoConfigAsync(string path,
            CancellationToken cancellationToken)
        {
            var serializer = new SharpYaml.Serialization.Serializer();
            var yaml = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            return serializer.Deserialize<PlandoConfig>(yaml);
        }
    }
}
