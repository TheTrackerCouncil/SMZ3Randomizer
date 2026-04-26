using System;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.Options;

public class GameModeOptions
{
    public GameModeType SelectedGameModeType { get; set; }

    public bool RandomizeNumericAmounts { get; set; }

    public int GanonCrystalCount { get; set; } = 7;
    public int MinGanonCrystalCount { get; set; } = 7;
    public int MaxGanonCrystalCount { get; set; } = 7;
    public int TourianBossCount { get; set; } = 4;
    public int MinTourianBossCount { get; set; } = 4;
    public int MaxTourianBossCount { get; set; } = 4;

    public int SpazersInPool { get; set; } = 40;
    public int MinSpazersInPool { get; set; } = 25;
    public int MaxSpazersInPool { get; set; } = 50;
    public int SpazersRequired { get; set; } = 30;
    public int MinSpazersRequired { get; set; } = 15;
    public int MaxSpazersRequired { get; set; } = 30;

    public bool LiftOffOnGoalCompletion { get; set; }

    public int GanonsTowerCrystalCount { get; set; } = 7;
    public int MinGanonsTowerCrystalCount { get; set; } = 7;
    public int MaxGanonsTowerCrystalCount { get; set; } = 7;
    public KeysanityMode KeysanityMode { get; set; }
    public TourianBossDoor TourianBossDoor { get; set; }
    public KeysanityGanonsTowerBigKeyLocation KeysanityGanonsTowerBigKeyLocation { get; set; }
    public bool ShuffleMetroidBossTokens { get; set; }
    public PyramidHole PyramidHole { get; set; }

    public GameModeOptions Clone()
    {
        return (GameModeOptions)MemberwiseClone();
    }

    public bool IsAltGameMode()
    {
        return SelectedGameModeType != GameModeType.Vanilla && SelectedGameModeType != GameModeType.AllDungeons;
    }
}
