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

        public PlandoFiller Create(PlandoConfig plandoConfig)
        {
            var logger = _loggerFactory.CreateLogger<PlandoFiller>();
            return new PlandoFiller(plandoConfig, logger);
        }
    }
}
