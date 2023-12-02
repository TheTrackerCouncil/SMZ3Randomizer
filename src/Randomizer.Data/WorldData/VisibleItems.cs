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
    private static readonly IDeserializer s_deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    public List<VisibleItemZelda> ZeldaItems { get; set; } = new();
    public List<VisibleItemMetroid> MetroidItems { get; set; } = new();

    public static VisibleItems GetVisibleItems()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Randomizer.Data.WorldData.VisibleItems.yml") ?? throw new FileNotFoundException("Unable to find ItemSettingOptions.yml file");
        using var reader = new StreamReader(stream);
        var ymlText = reader.ReadToEnd();
        return s_deserializer.Deserialize<VisibleItems>(ymlText);
    }
}
