using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

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
