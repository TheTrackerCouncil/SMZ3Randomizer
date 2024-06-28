using System.IO;

namespace TrackerCouncil.Smz3.UI.Legacy;

public static class EmbeddedResource
{
    public static Stream? GetStream<T>(string resourceName)
    {
        var assembly = typeof(T).Assembly;
        return assembly.GetManifestResourceStream(typeof(T), resourceName);
    }
}
