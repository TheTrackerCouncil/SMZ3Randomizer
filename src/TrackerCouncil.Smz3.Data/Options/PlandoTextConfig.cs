using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Options;

public class PlandoTextConfig
{
    /// <summary>
    /// Text for first dropping down into Ganon's room.
    /// </summary>
    public string? GanonIntro { get; init; }

    /// <summary>
    /// Text for first dropping down into Ganon's room when you have not met your objective
    /// </summary>
    public string? GanonIntroGoalsNotMet { get; init; }

    /// <summary>
    /// Test for blind
    /// </summary>
    public string? BlindIntro { get; init; }

    /// <summary>
    /// Text for the won game screen in Zelda
    /// </summary>
    public string? TriforceRoom { get; init; }

    /// <summary>
    /// Line for King Zora saying what he has
    /// </summary>
    public string? KingZora { get; init; }

    /// <summary>
    /// Line for the bottle merchant saying what he has
    /// </summary>
    public string? BottleMerchant { get; init; }

    /// <summary>
    /// Text for Ganon giving a hint to the silvers location
    /// </summary>
    public string? GanonSilversHint { get; init; }

    /// <summary>
    /// Text for the sign in front of Ganon's Tower with the required crystal count to enter
    /// </summary>
    public string? GanonsTowerGoalSign { get; init; }

    /// <summary>
    /// Text for the sign on the pyramid with the required crystal count to defeat Ganon
    /// </summary>
    public string? GanonGoalSign { get; init; }

    /// <summary>
    /// Text for Sahasrahla's green pendant dungeon reveal text
    /// </summary>
    public string? SahasrahlaReveal { get; init; }

    /// <summary>
    /// Text for the bomb shop red crystal dungeon reveal text
    /// </summary>
    public string? BombShopReveal { get; init; }

    /// <summary>
    /// Text for the guy in the Kak bar that has a bunch of jokes in vanilla
    /// </summary>
    public string? TavernMan { get; init; }

    public string? MasterSwordPedestal { get; init; }

    public string? EtherTablet { get; init; }

    public string? BombosTablet { get; init; }

    public string? HintTileEasternPalace { get; init; }

    public string? HintTileTowerOfHeraFloor4 { get; init; }

    public string? HintTileSpectacleRock { get; init; }

    public string? HintTileSwampEntrance { get; init; }

    public string? HintTileThievesTownUpstairs { get; init; }

    public string? HintTileMiseryMire { get; init; }

    public string? HintTilePalaceOfDarkness { get; init; }

    public string? HintTileDesertBonkTorchRoom { get; init; }

    public string? HintTileCastleTower { get; init; }

    public string? HintTileIceLargeRoom { get; init; }

    public string? HintTileTurtleRock { get; init; }

    public string? HintTileIceEntrance { get; init; }

    public string? HintTileIceStalfosKnightsRoom { get; init; }

    public string? HintTileTowerOfHeraEntrance { get; init; }

    public string? HintTileSouthEastDarkworldCave { get; init; }

    [YamlIgnore, Newtonsoft.Json.JsonIgnore]
    public bool HasHintTileText => !string.IsNullOrEmpty(HintTileEasternPalace) ||
                                   !string.IsNullOrEmpty(HintTileTowerOfHeraFloor4) ||
                                   !string.IsNullOrEmpty(HintTileSpectacleRock) ||
                                   !string.IsNullOrEmpty(HintTileSwampEntrance) ||
                                   !string.IsNullOrEmpty(HintTileThievesTownUpstairs) ||
                                   !string.IsNullOrEmpty(HintTileMiseryMire) ||
                                   !string.IsNullOrEmpty(HintTilePalaceOfDarkness) ||
                                   !string.IsNullOrEmpty(HintTileDesertBonkTorchRoom) ||
                                   !string.IsNullOrEmpty(HintTileCastleTower) ||
                                   !string.IsNullOrEmpty(HintTileIceLargeRoom) ||
                                   !string.IsNullOrEmpty(HintTileTurtleRock) ||
                                   !string.IsNullOrEmpty(HintTileIceEntrance) ||
                                   !string.IsNullOrEmpty(HintTileIceStalfosKnightsRoom) ||
                                   !string.IsNullOrEmpty(HintTileTowerOfHeraEntrance) ||
                                   !string.IsNullOrEmpty(HintTileSouthEastDarkworldCave);

}
