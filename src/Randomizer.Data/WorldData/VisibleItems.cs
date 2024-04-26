using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Randomizer.Data.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Randomizer.Data.WorldData;

public class VisibleItems
{
    public List<VisibleItemZelda> ZeldaItems { get; set; } = new();
    public List<VisibleItemMetroid> MetroidItems { get; set; } = new();

    public static VisibleItems GetVisibleItems()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Randomizer.Data.WorldData.VisibleItems.yml") ?? throw new FileNotFoundException("Unable to find ItemSettingOptions.yml file");
        using var reader = new StreamReader(stream);
        var ymlText = reader.ReadToEnd();
        return deserializer.Deserialize<VisibleItems>(ymlText);
    }
}
