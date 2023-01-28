using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.Generation
{
    public class MultiplayerFillerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public MultiplayerFillerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public MultiplayerFiller Create()
        {
            var logger = _loggerFactory.CreateLogger<MultiplayerFiller>();
            return new MultiplayerFiller(logger);
        }
    }
}
