using Microsoft.Extensions.Logging;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class MultiplayerFillerFactory(ILoggerFactory loggerFactory)
{
    public MultiplayerFiller Create()
    {
        var logger = loggerFactory.CreateLogger<MultiplayerFiller>();
        return new MultiplayerFiller(logger);
    }
}
