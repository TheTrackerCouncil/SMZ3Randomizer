using Randomizer.Data.Options;

namespace Randomizer.PatchBuilder;

public class PatchBuilderConfig
{
    public PatchFlags PatchFlags { get; set; } = new();
    public PatchEnvironmentSettings EnvironmentSettings { get; set; } = new();
    public PatchOptions PatchOptions { get; set; } = new();
    public PlandoConfig PlandoConfig { get; set; } = new();
}

public class PatchEnvironmentSettings
{
    public string MetroidRomPath { get; set; } = "";
    public string Z3RomPath { get; set; } = "";
    public string OutputPath { get; set; } = "";
    public string TestRomFileName { get; set; } = "";
    public string PatchBuildScriptPath { get; set; } = "";
    public string LaunchApplication { get; set; } = "";
    public string LaunchArguments { get; set; } = "";
}

public class PatchFlags
{
    public bool CreatePatches { get; set; }
    public bool CopyPatchesToProject { get; set; }
    public bool GenerateTestRom { get; set; }
    public bool LaunchTestRom { get; set; }
}
