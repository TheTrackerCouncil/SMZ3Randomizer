using System.Collections.Generic;
using System.ComponentModel;
using Randomizer.Shared;
using static Randomizer.Data.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional location information
    /// </summary>
    [Description("Config file for location names and various tracker responses when clearing and marking locations")]
    public class LocationConfig : List<LocationInfo>, IMergeable<LocationInfo>, IConfigFile<LocationConfig>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LocationConfig() : base()
        {
        }

        /// <summary>
        /// Returns default location information
        /// </summary>
        /// <returns></returns>
        public static LocationConfig Default()
        {
            return new LocationConfig
            {
                new()
                {
                    LocationNumber = 0,
                    LocationId = LocationId.CrateriaPowerBomb
                },
                new()
                {
                    LocationNumber = 1,
                    LocationId = LocationId.CrateriaWestOceanFloodedCavern
                },
                new()
                {
                    LocationNumber = 2,
                    LocationId = LocationId.CrateriaWestOceanSky
                },
                new()
                {
                    LocationNumber = 3,
                    LocationId = LocationId.CrateriaWestOceanMorphBallMaze
                },
                new()
                {
                    LocationNumber = 4,
                    LocationId = LocationId.CrateriaMoat
                },
                new()
                {
                    LocationNumber = 5,
                    LocationId = LocationId.CrateriaGauntletEnergyTank
                },
                new()
                {
                    LocationNumber = 6,
                    LocationId = LocationId.CrateriaPit
                },
                new()
                {
                    LocationNumber = 7,
                    LocationId = LocationId.CrateriaBombTorizo
                },
                new()
                {
                    LocationNumber = 8,
                    LocationId = LocationId.CrateriaTerminator
                },
                new()
                {
                    LocationNumber = 9,
                    LocationId = LocationId.CrateriaGauntletShaftRight
                },
                new()
                {
                    LocationNumber = 10,
                    LocationId = LocationId.CrateriaGauntletShaftLeft
                },
                new()
                {
                    LocationNumber = 11,
                    LocationId = LocationId.CrateriaSuper
                },
                new()
                {
                    LocationNumber = 12,
                    LocationId = LocationId.CrateriaFinalMissile
                },
                new()
                {
                    LocationNumber = 13,
                    LocationId = LocationId.GreenBrinstarMainShaft
                },
                new()
                {
                    LocationNumber = 14,
                    LocationId = LocationId.PinkBrinstarSporeSpawnSuper
                },
                new()
                {
                    LocationNumber = 15,
                    LocationId = LocationId.GreenBrinstarEarlySupersBottom
                },
                new()
                {
                    LocationNumber = 16,
                    LocationId = LocationId.GreenBrinstarEarlySupersTop
                },
                new()
                {
                    LocationNumber = 17,
                    LocationId = LocationId.GreenBrinstarReserveTankChozo
                },
                new()
                {
                    LocationNumber = 18,
                    LocationId = LocationId.GreenBrinstarReserveTankHidden
                },
                new()
                {
                    LocationNumber = 19,
                    LocationId = LocationId.GreenBrinstarReserveTankVisible
                },
                new()
                {
                    LocationNumber = 21,
                    LocationId = LocationId.PinkBrinstarPinkShaftTop
                },
                new()
                {
                    LocationNumber = 22,
                    LocationId = LocationId.PinkBrinstarPinkShaftBottom
                },
                new()
                {
                    LocationNumber = 23,
                    LocationId = LocationId.PinkBrinstarPinkShaftChozo
                },
                new()
                {
                    LocationNumber = 24,
                    LocationId = LocationId.PinkBrinstarPowerBomb
                },
                new()
                {
                    LocationNumber = 25,
                    LocationId = LocationId.GreenBrinstarGreenHillZone
                },
                new()
                {
                    LocationNumber = 26,
                    LocationId = LocationId.BlueBrinstarMorphBallRight
                },
                new()
                {
                    LocationNumber = 27,
                    LocationId = LocationId.BlueBrinstarMorphBallLeft
                },
                new()
                {
                    LocationNumber = 28,
                    LocationId = LocationId.BlueBrinstarFirstMissile
                },
                new()
                {
                    LocationNumber = 29,
                    LocationId = LocationId.BlueBrinstarEnergyTankCeiling
                },
                new()
                {
                    LocationNumber = 30,
                    LocationId = LocationId.GreenBrinstarEtecoonEnergyTank
                },
                new()
                {
                    LocationNumber = 31,
                    LocationId = LocationId.GreenBrinstarEtecoonSuper
                },
                new()
                {
                    LocationNumber = 33,
                    LocationId = LocationId.PinkBrinstarWaterwayEnergyTank
                },
                new()
                {
                    LocationNumber = 34,
                    LocationId = LocationId.BlueBrinstarEnergyTankRight
                },
                new()
                {
                    LocationNumber = 35,
                    LocationId = LocationId.PinkBrinstarHoptank
                },
                new()
                {
                    LocationNumber = 36,
                    LocationId = LocationId.BlueBrinstarDoubleMissileVisible
                },
                new()
                {
                    LocationNumber = 37,
                    LocationId = LocationId.BlueBrinstarDoubleMissileHidden
                },
                new()
                {
                    LocationNumber = 38,
                    LocationId = LocationId.RedBrinstarXRayScope
                },
                new()
                {
                    LocationNumber = 39,
                    LocationId = LocationId.RedBrinstarBetaPowerBomb
                },
                new()
                {
                    LocationNumber = 40,
                    LocationId = LocationId.RedBrinstarAlphaPowerBombRight
                },
                new()
                {
                    LocationNumber = 41,
                    LocationId = LocationId.RedBrinstarAlphaPowerBombLeft
                },
                new()
                {
                    LocationNumber = 42,
                    LocationId = LocationId.RedBrinstarSpazer
                },
                new()
                {
                    LocationNumber = 43,
                    LocationId = LocationId.KraidsLairEnergyTank
                },
                new()
                {
                    LocationNumber = 44,
                    LocationId = LocationId.KraidsLairKihunter
                },
                new()
                {
                    LocationNumber = 48,
                    LocationId = LocationId.KraidsLairVariaSuit
                },
                new()
                {
                    LocationNumber = 49,
                    LocationId = LocationId.UpperNorfairCathedral
                },
                new()
                {
                    LocationNumber = 50,
                    LocationId = LocationId.UpperNorfairIceBeam
                },
                new()
                {
                    LocationNumber = 51,
                    LocationId = LocationId.UpperNorfairCrumbleShaft
                },
                new()
                {
                    LocationNumber = 52,
                    LocationId = LocationId.UpperNorfairCrocomire
                },
                new()
                {
                    LocationNumber = 53,
                    LocationId = LocationId.UpperNorfairHiJumpBoots
                },
                new()
                {
                    LocationNumber = 54,
                    LocationId = LocationId.UpperNorfairCrocomireEscape
                },
                new()
                {
                    LocationNumber = 55,
                    LocationId = LocationId.UpperNorfairHiJumpEnergyTankLeft
                },
                new()
                {
                    LocationNumber = 56,
                    LocationId = LocationId.UpperNorfairHiJumpEnergyTankRight
                },
                new()
                {
                    LocationNumber = 57,
                    LocationId = LocationId.UpperNorfairPostCrocomirePowerBomb
                },
                new()
                {
                    LocationNumber = 58,
                    LocationId = LocationId.UpperNorfairPostCrocomireMissile
                },
                new()
                {
                    LocationNumber = 59,
                    LocationId = LocationId.UpperNorfairPostCrocomireJump
                },
                new()
                {
                    LocationNumber = 60,
                    LocationId = LocationId.UpperNorfairGrappleBeam
                },
                new()
                {
                    LocationNumber = 61,
                    LocationId = LocationId.UpperNorfairReserveTankChozo
                },
                new()
                {
                    LocationNumber = 62,
                    LocationId = LocationId.UpperNorfairReserveTankHidden
                },
                new()
                {
                    LocationNumber = 63,
                    LocationId = LocationId.UpperNorfairGreenBubblesMissile
                },
                new()
                {
                    LocationNumber = 64,
                    LocationId = LocationId.UpperNorfairBubbleMountain
                },
                new()
                {
                    LocationNumber = 65,
                    LocationId = LocationId.UpperNorfairSpeedBoosterHall
                },
                new()
                {
                    LocationNumber = 66,
                    LocationId = LocationId.UpperNorfairSpeedBooster
                },
                new()
                {
                    LocationNumber = 67,
                    LocationId = LocationId.UpperNorfairDoubleChamber
                },
                new()
                {
                    LocationNumber = 68,
                    LocationId = LocationId.UpperNorfairWaveBeam
                },
                new()
                {
                    LocationNumber = 70,
                    LocationId = LocationId.LowerNorfairGoldenTorizoVisible
                },
                new()
                {
                    LocationNumber = 71,
                    LocationId = LocationId.LowerNorfairGoldenTorizoHidden
                },
                new()
                {
                    LocationNumber = 73,
                    LocationId = LocationId.LowerNorfairMickeyMouse
                },
                new()
                {
                    LocationNumber = 74,
                    LocationId = LocationId.LowerNorfairSpringBallMaze
                },
                new()
                {
                    LocationNumber = 75,
                    LocationId = LocationId.LowerNorfairEscapePowerBomb
                },
                new()
                {
                    LocationNumber = 76,
                    LocationId = LocationId.LowerNorfairWasteland
                },
                new()
                {
                    LocationNumber = 77,
                    LocationId = LocationId.LowerNorfairThreeMusketeers
                },
                new()
                {
                    LocationNumber = 78,
                    LocationId = LocationId.LowerNorfairRidleyTank
                },
                new()
                {
                    LocationNumber = 79,
                    LocationId = LocationId.LowerNorfairScrewAttack
                },
                new()
                {
                    LocationNumber = 80,
                    LocationId = LocationId.LowerNorfairFireflea
                },
                new()
                {
                    LocationNumber = 128,
                    LocationId = LocationId.WreckedShipMainShaft
                },
                new()
                {
                    LocationNumber = 129,
                    LocationId = LocationId.WreckedShipBowlingAlleyTop
                },
                new()
                {
                    LocationNumber = 130,
                    LocationId = LocationId.WreckedShipBowlingAlleyBottom
                },
                new()
                {
                    LocationNumber = 131,
                    LocationId = LocationId.WreckedShipAssemblyLine
                },
                new()
                {
                    LocationNumber = 132,
                    LocationId = LocationId.WreckedShipEnergyTank
                },
                new()
                {
                    LocationNumber = 133,
                    LocationId = LocationId.WreckedShipWestSuper
                },
                new()
                {
                    LocationNumber = 134,
                    LocationId = LocationId.WreckedShipEastSuper
                },
                new()
                {
                    LocationNumber = 135,
                    LocationId = LocationId.WreckedShipGravitySuit
                },
                new()
                {
                    LocationNumber = 136,
                    LocationId = LocationId.OuterMaridiaMainStreetBottom
                },
                new()
                {
                    LocationNumber = 137,
                    LocationId = LocationId.OuterMaridiaMainStreetTop
                },
                new()
                {
                    LocationNumber = 138,
                    LocationId = LocationId.OuterMaridiaMamaTurtleVisible
                },
                new()
                {
                    LocationNumber = 139,
                    LocationId = LocationId.OuterMaridiaMamaTurtleHidden
                },
                new()
                {
                    LocationNumber = 140,
                    LocationId = LocationId.InnerMaridiaWateringHoleLeft
                },
                new()
                {
                    LocationNumber = 141,
                    LocationId = LocationId.InnerMaridiaWateringHoleRight
                },
                new()
                {
                    LocationNumber = 142,
                    LocationId = LocationId.InnerMaridiaPseudoPlasmaSpark
                },
                new()
                {
                    LocationNumber = 143,
                    LocationId = LocationId.InnerMaridiaPlasma
                },
                new()
                {
                    LocationNumber = 144,
                    LocationId = LocationId.InnerMaridiaWestSandHoleLeft
                },
                new()
                {
                    LocationNumber = 145,
                    LocationId = LocationId.InnerMaridiaWestSandHoleRight
                },
                new()
                {
                    LocationNumber = 146,
                    LocationId = LocationId.InnerMaridiaEastSandHoleLeft
                },
                new()
                {
                    LocationNumber = 147,
                    LocationId = LocationId.InnerMaridiaEastSandHoleRight
                },
                new()
                {
                    LocationNumber = 148,
                    LocationId = LocationId.InnerMaridiaAqueductLeft
                },
                new()
                {
                    LocationNumber = 149,
                    LocationId = LocationId.InnerMaridiaAqueductRight
                },
                new()
                {
                    LocationNumber = 150,
                    LocationId = LocationId.InnerMaridiaSpringBall
                },
                new()
                {
                    LocationNumber = 151,
                    LocationId = LocationId.InnerMaridiaPreciousRoom
                },
                new()
                {
                    LocationNumber = 152,
                    LocationId = LocationId.InnerMaridiaBotwoon
                },
                new()
                {
                    LocationNumber = 154,
                    LocationId = LocationId.InnerMaridiaSpaceJump
                },
                new()
                {
                    LocationNumber = 256,
                    LocationId = LocationId.EtherTablet
                },
                new()
                {
                    LocationNumber = 257,
                    LocationId = LocationId.SpectacleRock
                },
                new()
                {
                    LocationNumber = 258,
                    LocationId = LocationId.SpectacleRockCave
                },
                new()
                {
                    LocationNumber = 259,
                    LocationId = LocationId.OldMan
                },
                new()
                {
                    LocationNumber = 260,
                    LocationId = LocationId.FloatingIsland
                },
                new()
                {
                    LocationNumber = 261,
                    LocationId = LocationId.SpiralCave
                },
                new()
                {
                    LocationNumber = 262,
                    LocationId = LocationId.ParadoxCaveUpperLeft
                },
                new()
                {
                    LocationNumber = 263,
                    LocationId = LocationId.ParadoxCaveUpperRight
                },
                new()
                {
                    LocationNumber = 264,
                    LocationId = LocationId.ParadoxCaveLowerFarLeft
                },
                new()
                {
                    LocationNumber = 265,
                    LocationId = LocationId.ParadoxCaveLowerLeft
                },
                new()
                {
                    LocationNumber = 266,
                    LocationId = LocationId.ParadoxCaveLowerMiddle
                },
                new()
                {
                    LocationNumber = 267,
                    LocationId = LocationId.ParadoxCaveLowerRight
                },
                new()
                {
                    LocationNumber = 268,
                    LocationId = LocationId.ParadoxCaveLowerFarRight
                },
                new()
                {
                    LocationNumber = 269,
                    LocationId = LocationId.MimicCave
                },
                new()
                {
                    LocationNumber = 270,
                    LocationId = LocationId.MasterSwordPedestal
                },
                new()
                {
                    LocationNumber = 271,
                    LocationId = LocationId.Mushroom
                },
                new()
                {
                    LocationNumber = 272,
                    LocationId = LocationId.LostWoodsHideout
                },
                new()
                {
                    LocationNumber = 273,
                    LocationId = LocationId.LumberjackTree
                },
                new()
                {
                    LocationNumber = 274,
                    LocationId = LocationId.PegasusRocks
                },
                new()
                {
                    LocationNumber = 275,
                    LocationId = LocationId.GraveyardLedge
                },
                new()
                {
                    LocationNumber = 276,
                    LocationId = LocationId.KingsTomb
                },
                new()
                {
                    LocationNumber = 277,
                    LocationId = LocationId.KakarikoWellTop
                },
                new()
                {
                    LocationNumber = 278,
                    LocationId = LocationId.KakarikoWellLeft
                },
                new()
                {
                    LocationNumber = 279,
                    LocationId = LocationId.KakarikoWellMiddle
                },
                new()
                {
                    LocationNumber = 280,
                    LocationId = LocationId.KakarikoWellRight
                },
                new()
                {
                    LocationNumber = 281,
                    LocationId = LocationId.KakarikoWellBottom
                },
                new()
                {
                    LocationNumber = 282,
                    LocationId = LocationId.BlindsHideoutTop
                },
                new()
                {
                    LocationNumber = 283,
                    LocationId = LocationId.BlindsHideoutFarLeft
                },
                new()
                {
                    LocationNumber = 284,
                    LocationId = LocationId.BlindsHideoutLeft
                },
                new()
                {
                    LocationNumber = 285,
                    LocationId = LocationId.BlindsHideoutRight
                },
                new()
                {
                    LocationNumber = 286,
                    LocationId = LocationId.BlindsHideoutFarRight
                },
                new()
                {
                    LocationNumber = 287,
                    LocationId = LocationId.BottleMerchant
                },
                new()
                {
                    LocationNumber = 289,
                    LocationId = LocationId.SickKid
                },
                new()
                {
                    LocationNumber = 290,
                    LocationId = LocationId.KakarikoTavern
                },
                new()
                {
                    LocationNumber = 291,
                    LocationId = LocationId.MagicBat
                },
                new()
                {
                    LocationNumber = 292,
                    LocationId = LocationId.KingZora
                },
                new()
                {
                    LocationNumber = 293,
                    LocationId = LocationId.ZorasLedge
                },
                new()
                {
                    LocationNumber = 295,
                    LocationId = LocationId.WaterfallFairyRight
                },
                new()
                {
                    LocationNumber = 296,
                    LocationId = LocationId.PotionShop
                },
                new()
                {
                    LocationNumber = 297,
                    LocationId = LocationId.SahasrahlasHutLeft
                },
                new()
                {
                    LocationNumber = 298,
                    LocationId = LocationId.SahasrahlasHutMiddle
                },
                new()
                {
                    LocationNumber = 299,
                    LocationId = LocationId.SahasrahlasHutRight
                },
                new()
                {
                    LocationNumber = 300,
                    LocationId = LocationId.Sahasrahla
                },
                new()
                {
                    LocationNumber = 301,
                    LocationId = LocationId.MazeRace
                },
                new()
                {
                    LocationNumber = 307,
                    LocationId = LocationId.MiniMoldormCaveFarLeft
                },
                new()
                {
                    LocationNumber = 308,
                    LocationId = LocationId.MiniMoldormCaveLeft
                },
                new()
                {
                    LocationNumber = 309,
                    LocationId = LocationId.MiniMoldormCaveNpc
                },
                new()
                {
                    LocationNumber = 310,
                    LocationId = LocationId.MiniMoldormCaveRight
                },
                new()
                {
                    LocationNumber = 314,
                    LocationId = LocationId.BombosTablet
                },
                new()
                {
                    LocationNumber = 315,
                    LocationId = LocationId.FloodgateChest
                },
                new()
                {
                    LocationNumber = 316,
                    LocationId = LocationId.SunkenTreasure
                },
                new()
                {
                    LocationNumber = 317,
                    LocationId = LocationId.LakeHyliaIsland
                },
                new()
                {
                    LocationNumber = 318,
                    LocationId = LocationId.Hobo
                },
                new()
                {
                    LocationNumber = 319,
                    LocationId = LocationId.IceRodCave
                },
                new()
                {
                    LocationNumber = 320,
                    LocationId = LocationId.SpikeCave
                },
                new()
                {
                    LocationNumber = 321,
                    LocationId = LocationId.HookshotCaveTopRight
                },
                new()
                {
                    LocationNumber = 322,
                    LocationId = LocationId.HookshotCaveTopLeft
                },
                new()
                {
                    LocationNumber = 323,
                    LocationId = LocationId.HookshotCaveBottomLeft
                },
                new()
                {
                    LocationNumber = 324,
                    LocationId = LocationId.HookshotCaveBottomRight
                },
                new()
                {
                    LocationNumber = 325,
                    LocationId = LocationId.SuperbunnyCaveTop
                },
                new()
                {
                    LocationNumber = 326,
                    LocationId = LocationId.SuperbunnyCaveBottom
                },
                new()
                {
                    LocationNumber = 327,
                    LocationId = LocationId.BumperCave
                },
                new()
                {
                    LocationNumber = 328,
                    LocationId = LocationId.ChestGame
                },
                new()
                {
                    LocationNumber = 329,
                    LocationId = LocationId.CShapedHouse
                },
                new()
                {
                    LocationNumber = 330,
                    LocationId = LocationId.Brewery
                },
                new()
                {
                    LocationNumber = 331,
                    LocationId = LocationId.HammerPegs
                },
                new()
                {
                    LocationNumber = 332,
                    LocationId = LocationId.Blacksmith
                },
                new()
                {
                    LocationNumber = 333,
                    LocationId = LocationId.PurpleChest
                },
                new()
                {
                    LocationNumber = 334,
                    LocationId = LocationId.Catfish
                },
                new()
                {
                    LocationNumber = 335,
                    LocationId = LocationId.Pyramid
                },
                new()
                {
                    LocationNumber = 336,
                    LocationId = LocationId.PyramidFairyLeft
                },
                new()
                {
                    LocationNumber = 337,
                    LocationId = LocationId.PyramidFairyRight
                },
                new()
                {
                    LocationNumber = 338,
                    LocationId = LocationId.DiggingGame
                },
                new()
                {
                    LocationNumber = 339,
                    LocationId = LocationId.Stumpy
                },
                new()
                {
                    LocationNumber = 340,
                    LocationId = LocationId.HypeCaveTop
                },
                new()
                {
                    LocationNumber = 341,
                    LocationId = LocationId.HypeCaveMiddleRight
                },
                new()
                {
                    LocationNumber = 342,
                    LocationId = LocationId.HypeCaveMiddleLeft
                },
                new()
                {
                    LocationNumber = 343,
                    LocationId = LocationId.HypeCaveBottom
                },
                new()
                {
                    LocationNumber = 344,
                    LocationId = LocationId.HypeCaveNpc
                },
                new()
                {
                    LocationNumber = 345,
                    LocationId = LocationId.MireShedLeft
                },
                new()
                {
                    LocationNumber = 346,
                    LocationId = LocationId.MireShedRight
                },
                new()
                {
                    LocationNumber = 347,
                    LocationId = LocationId.Sanctuary
                },
                new()
                {
                    LocationNumber = 348,
                    LocationId = LocationId.SewersSecretRoomLeft
                },
                new()
                {
                    LocationNumber = 349,
                    LocationId = LocationId.SewersSecretRoomMiddle
                },
                new()
                {
                    LocationNumber = 350,
                    LocationId = LocationId.SewersSecretRoomRight
                },
                new()
                {
                    LocationNumber = 351,
                    LocationId = LocationId.SewersDarkCross
                },
                new()
                {
                    LocationNumber = 352,
                    LocationId = LocationId.HyruleCastleMapChest
                },
                new()
                {
                    LocationNumber = 353,
                    LocationId = LocationId.HyruleCastleBoomerangChest
                },
                new()
                {
                    LocationNumber = 354,
                    LocationId = LocationId.HyruleCastleZeldasCell
                },
                new()
                {
                    LocationNumber = 355,
                    LocationId = LocationId.LinksUncle
                },
                new()
                {
                    LocationNumber = 356,
                    LocationId = LocationId.SecretPassage
                },
                new()
                {
                    LocationNumber = 357,
                    LocationId = LocationId.CastleTowerFoyer
                },
                new()
                {
                    LocationNumber = 358,
                    LocationId = LocationId.CastleTowerDarkMaze
                },
                new()
                {
                    LocationNumber = 359,
                    LocationId = LocationId.EasternPalaceCannonballChest
                },
                new()
                {
                    LocationNumber = 360,
                    LocationId = LocationId.EasternPalaceMapChest
                },
                new()
                {
                    LocationNumber = 361,
                    LocationId = LocationId.EasternPalaceCompassChest
                },
                new()
                {
                    LocationNumber = 362,
                    LocationId = LocationId.EasternPalaceBigChest
                },
                new()
                {
                    LocationNumber = 363,
                    LocationId = LocationId.EasternPalaceBigKeyChest
                },
                new()
                {
                    LocationNumber = 364,
                    LocationId = LocationId.EasternPalaceArmosKnights
                },
                new()
                {
                    LocationNumber = 365,
                    LocationId = LocationId.DesertPalaceBigChest
                },
                new()
                {
                    LocationNumber = 366,
                    LocationId = LocationId.DesertPalaceTorch
                },
                new()
                {
                    LocationNumber = 367,
                    LocationId = LocationId.DesertPalaceMapChest
                },
                new()
                {
                    LocationNumber = 368,
                    LocationId = LocationId.DesertPalaceBigKeyChest
                },
                new()
                {
                    LocationNumber = 369,
                    LocationId = LocationId.DesertPalaceCompassChest
                },
                new()
                {
                    LocationNumber = 370,
                    LocationId = LocationId.DesertPalaceLanmolas
                },
                new()
                {
                    LocationNumber = 371,
                    LocationId = LocationId.TowerOfHeraBasementCage
                },
                new()
                {
                    LocationNumber = 372,
                    LocationId = LocationId.TowerOfHeraMapChest
                },
                new()
                {
                    LocationNumber = 373,
                    LocationId = LocationId.TowerOfHeraBigKeyChest
                },
                new()
                {
                    LocationNumber = 374,
                    LocationId = LocationId.TowerOfHeraCompassChest
                },
                new()
                {
                    LocationNumber = 375,
                    LocationId = LocationId.TowerOfHeraBigChest
                },
                new()
                {
                    LocationNumber = 376,
                    LocationId = LocationId.TowerOfHeraMoldorm
                },
                new()
                {
                    LocationNumber = 377,
                    LocationId = LocationId.PalaceOfDarknessShooterRoom
                },
                new()
                {
                    LocationNumber = 378,
                    LocationId = LocationId.PalaceOfDarknessBigKeyChest
                },
                new()
                {
                    LocationNumber = 379,
                    LocationId = LocationId.PalaceOfDarknessStalfosBasement
                },
                new()
                {
                    LocationNumber = 380,
                    LocationId = LocationId.PalaceOfDarknessTheArenaBridge
                },
                new()
                {
                    LocationNumber = 381,
                    LocationId = LocationId.PalaceOfDarknessTheArenaLedge
                },
                new()
                {
                    LocationNumber = 382,
                    LocationId = LocationId.PalaceOfDarknessMapChest
                },
                new()
                {
                    LocationNumber = 383,
                    LocationId = LocationId.PalaceOfDarknessCompassChest
                },
                new()
                {
                    LocationNumber = 384,
                    LocationId = LocationId.PalaceOfDarknessHarmlessHellway
                },
                new()
                {
                    LocationNumber = 385,
                    LocationId = LocationId.PalaceOfDarknessDarkBasementLeft
                },
                new()
                {
                    LocationNumber = 386,
                    LocationId = LocationId.PalaceOfDarknessDarkBasementRight
                },
                new()
                {
                    LocationNumber = 387,
                    LocationId = LocationId.PalaceOfDarknessDarkMazeTop
                },
                new()
                {
                    LocationNumber = 388,
                    LocationId = LocationId.PalaceOfDarknessDarkMazeBottom
                },
                new()
                {
                    LocationNumber = 389,
                    LocationId = LocationId.PalaceOfDarknessBigChest
                },
                new()
                {
                    LocationNumber = 390,
                    LocationId = LocationId.PalaceOfDarknessHelmasaurKing
                },
                new()
                {
                    LocationNumber = 391,
                    LocationId = LocationId.SwampPalaceEntrance
                },
                new()
                {
                    LocationNumber = 392,
                    LocationId = LocationId.SwampPalaceMapChest
                },
                new()
                {
                    LocationNumber = 393,
                    LocationId = LocationId.SwampPalaceBigChest
                },
                new()
                {
                    LocationNumber = 394,
                    LocationId = LocationId.SwampPalaceCompassChest
                },
                new()
                {
                    LocationNumber = 395,
                    LocationId = LocationId.SwampPalaceWestChest
                },
                new()
                {
                    LocationNumber = 396,
                    LocationId = LocationId.SwampPalaceBigKeyChest
                },
                new()
                {
                    LocationNumber = 397,
                    LocationId = LocationId.SwampPalaceFloodedRoomLeft
                },
                new()
                {
                    LocationNumber = 398,
                    LocationId = LocationId.SwampPalaceFloodedRoomRight
                },
                new()
                {
                    LocationNumber = 399,
                    LocationId = LocationId.SwampPalaceWaterfallRoom
                },
                new()
                {
                    LocationNumber = 400,
                    LocationId = LocationId.SwampPalaceArrghus
                },
                new()
                {
                    LocationNumber = 401,
                    LocationId = LocationId.SkullWoodsPotPrison
                },
                new()
                {
                    LocationNumber = 402,
                    LocationId = LocationId.SkullWoodsCompassChest
                },
                new()
                {
                    LocationNumber = 403,
                    LocationId = LocationId.SkullWoodsBigChest
                },
                new()
                {
                    LocationNumber = 404,
                    LocationId = LocationId.SkullWoodsMapChest
                },
                new()
                {
                    LocationNumber = 405,
                    LocationId = LocationId.SkullWoodsPinballRoom
                },
                new()
                {
                    LocationNumber = 406,
                    LocationId = LocationId.SkullWoodsBigKeyChest
                },
                new()
                {
                    LocationNumber = 407,
                    LocationId = LocationId.SkullWoodsBridgeRoom
                },
                new()
                {
                    LocationNumber = 408,
                    LocationId = LocationId.SkullWoodsMothula
                },
                new()
                {
                    LocationNumber = 409,
                    LocationId = LocationId.ThievesTownMapChest
                },
                new()
                {
                    LocationNumber = 410,
                    LocationId = LocationId.ThievesTownAmbushChest
                },
                new()
                {
                    LocationNumber = 411,
                    LocationId = LocationId.ThievesTownCompassChest
                },
                new()
                {
                    LocationNumber = 412,
                    LocationId = LocationId.ThievesTownBigKeyChest
                },
                new()
                {
                    LocationNumber = 413,
                    LocationId = LocationId.ThievesTownAttic
                },
                new()
                {
                    LocationNumber = 414,
                    LocationId = LocationId.ThievesTownBlindsCell
                },
                new()
                {
                    LocationNumber = 415,
                    LocationId = LocationId.ThievesTownBigChest
                },
                new()
                {
                    LocationNumber = 416,
                    LocationId = LocationId.ThievesTownBlind
                },
                new()
                {
                    LocationNumber = 417,
                    LocationId = LocationId.IcePalaceCompassChest
                },
                new()
                {
                    LocationNumber = 418,
                    LocationId = LocationId.IcePalaceSpikeRoom
                },
                new()
                {
                    LocationNumber = 419,
                    LocationId = LocationId.IcePalaceMapChest
                },
                new()
                {
                    LocationNumber = 420,
                    LocationId = LocationId.IcePalaceBigKeyChest
                },
                new()
                {
                    LocationNumber = 421,
                    LocationId = LocationId.IcePalaceIcedTRoom
                },
                new()
                {
                    LocationNumber = 422,
                    LocationId = LocationId.IcePalaceFreezorChest
                },
                new()
                {
                    LocationNumber = 423,
                    LocationId = LocationId.IcePalaceBigChest
                },
                new()
                {
                    LocationNumber = 424,
                    LocationId = LocationId.IcePalaceKholdstare
                },
                new()
                {
                    LocationNumber = 425,
                    LocationId = LocationId.MiseryMireMainLobby
                },
                new()
                {
                    LocationNumber = 426,
                    LocationId = LocationId.MiseryMireMapChest
                },
                new()
                {
                    LocationNumber = 427,
                    LocationId = LocationId.MiseryMireBridgeChest
                },
                new()
                {
                    LocationNumber = 428,
                    LocationId = LocationId.MiseryMireSpikeChest
                },
                new()
                {
                    LocationNumber = 429,
                    LocationId = LocationId.MiseryMireCompassChest
                },
                new()
                {
                    LocationNumber = 430,
                    LocationId = LocationId.MiseryMireBigKeyChest
                },
                new()
                {
                    LocationNumber = 431,
                    LocationId = LocationId.MiseryMireBigChest
                },
                new()
                {
                    LocationNumber = 432,
                    LocationId = LocationId.MiseryMireVitreous
                },
                new()
                {
                    LocationNumber = 433,
                    LocationId = LocationId.TurtleRockCompassChest
                },
                new()
                {
                    LocationNumber = 434,
                    LocationId = LocationId.TurtleRockRollerRoomLeft
                },
                new()
                {
                    LocationNumber = 435,
                    LocationId = LocationId.TurtleRockRollerRoomRight
                },
                new()
                {
                    LocationNumber = 436,
                    LocationId = LocationId.TurtleRockChainChomps
                },
                new()
                {
                    LocationNumber = 437,
                    LocationId = LocationId.TurtleRockBigKeyChest
                },
                new()
                {
                    LocationNumber = 438,
                    LocationId = LocationId.TurtleRockBigChest
                },
                new()
                {
                    LocationNumber = 439,
                    LocationId = LocationId.TurtleRockCrystarollerRoom
                },
                new()
                {
                    LocationNumber = 440,
                    LocationId = LocationId.TurtleRockEyeBridgeTopRight
                },
                new()
                {
                    LocationNumber = 441,
                    LocationId = LocationId.TurtleRockEyeBridgeTopLeft
                },
                new()
                {
                    LocationNumber = 442,
                    LocationId = LocationId.TurtleRockEyeBridgeBottomRight
                },
                new()
                {
                    LocationNumber = 443,
                    LocationId = LocationId.TurtleRockEyeBridgeBottomLeft
                },
                new()
                {
                    LocationNumber = 444,
                    LocationId = LocationId.TurtleRockTrinexx
                },
                new()
                {
                    LocationNumber = 445,
                    LocationId = LocationId.GanonsTowerBobsTorch
                },
                new()
                {
                    LocationNumber = 446,
                    LocationId = LocationId.GanonsTowerDMsRoomTopLeft
                },
                new()
                {
                    LocationNumber = 447,
                    LocationId = LocationId.GanonsTowerDMsRoomTopRight
                },
                new()
                {
                    LocationNumber = 448,
                    LocationId = LocationId.GanonsTowerDMsRoomBottomLeft
                },
                new()
                {
                    LocationNumber = 449,
                    LocationId = LocationId.GanonsTowerDMsRoomBottomRight
                },
                new()
                {
                    LocationNumber = 450,
                    LocationId = LocationId.GanonsTowerMapChest
                },
                new()
                {
                    LocationNumber = 451,
                    LocationId = LocationId.GanonsTowerFiresnakeRoom
                },
                new()
                {
                    LocationNumber = 452,
                    LocationId = LocationId.GanonsTowerRandomizerRoomTopLeft
                },
                new()
                {
                    LocationNumber = 453,
                    LocationId = LocationId.GanonsTowerRandomizerRoomTopRight
                },
                new()
                {
                    LocationNumber = 454,
                    LocationId = LocationId.GanonsTowerRandomizerRoomBottomLeft
                },
                new()
                {
                    LocationNumber = 455,
                    LocationId = LocationId.GanonsTowerRandomizerRoomBottomRight
                },
                new()
                {
                    LocationNumber = 456,
                    LocationId = LocationId.GanonsTowerHopeRoomLeft
                },
                new()
                {
                    LocationNumber = 457,
                    LocationId = LocationId.GanonsTowerHopeRoomRight
                },
                new()
                {
                    LocationNumber = 458,
                    LocationId = LocationId.GanonsTowerTileRoom
                },
                new()
                {
                    LocationNumber = 459,
                    LocationId = LocationId.GanonsTowerCompassRoomTopLeft
                },
                new()
                {
                    LocationNumber = 460,
                    LocationId = LocationId.GanonsTowerCompassRoomTopRight
                },
                new()
                {
                    LocationNumber = 461,
                    LocationId = LocationId.GanonsTowerCompassRoomBottomLeft
                },
                new()
                {
                    LocationNumber = 462,
                    LocationId = LocationId.GanonsTowerCompassRoomBottomRight
                },
                new()
                {
                    LocationNumber = 463,
                    LocationId = LocationId.GanonsTowerBobsChest
                },
                new()
                {
                    LocationNumber = 464,
                    LocationId = LocationId.GanonsTowerBigChest
                },
                new()
                {
                    LocationNumber = 465,
                    LocationId = LocationId.GanonsTowerBigKeyChest
                },
                new()
                {
                    LocationNumber = 466,
                    LocationId = LocationId.GanonsTowerBigKeyRoomLeft
                },
                new()
                {
                    LocationNumber = 467,
                    LocationId = LocationId.GanonsTowerBigKeyRoomRight
                },
                new()
                {
                    LocationNumber = 468,
                    LocationId = LocationId.GanonsTowerMiniHelmasaurRoomLeft
                },
                new()
                {
                    LocationNumber = 469,
                    LocationId = LocationId.GanonsTowerMiniHelmasaurRoomRight
                },
                new()
                {
                    LocationNumber = 470,
                    LocationId = LocationId.GanonsTowerPreMoldormChest
                },
                new()
                {
                    LocationNumber = 471,
                    LocationId = LocationId.GanonsTowerMoldormChest
                },
                new()
                {
                    LocationNumber = 496,
                    LocationId = LocationId.Library
                },
                new()
                {
                    LocationNumber = 497,
                    LocationId = LocationId.FluteSpot
                },
                new()
                {
                    LocationNumber = 498,
                    LocationId = LocationId.SouthOfGrove
                },
                new()
                {
                    LocationNumber = 499,
                    LocationId = LocationId.LinksHouse
                },
                new()
                {
                    LocationNumber = 500,
                    LocationId = LocationId.AginahsCave
                },
                new()
                {
                    LocationNumber = 506,
                    LocationId = LocationId.ChickenHouse
                },
                new()
                {
                    LocationNumber = 507,
                    LocationId = LocationId.MiniMoldormCaveFarRight
                },
                new()
                {
                    LocationNumber = 508,
                    LocationId = LocationId.DesertLedge
                },
                new()
                {
                    LocationNumber = 509,
                    LocationId = LocationId.CheckerboardCave
                },
                new()
                {
                    LocationNumber = 510,
                    LocationId = LocationId.WaterfallFairyLeft
                },
            };
        }

        public static object Example()
        {
            return new LocationConfig()
            {
                new()
                {
                    LocationId = LocationId.GreenBrinstarGreenHillZone,
                    LocationNumber = 25,
                    Name =
                        new SchrodingersString("Green Hill Zone", "Jungle Slope",
                            new Possibility("Missile (green Brinstar pipe)", 0.1)),
                    Hints =
                        new SchrodingersString("Vague hint that could be said when asking where an item is"),
                    WhenTrackingJunk =
                        new SchrodingersString(
                            "Message when tracking the location and there's nothing good there"),
                    WhenTrackingProgression =
                        new SchrodingersString(
                            "Message when tracking the location and there's an item that's potentially progression there"),
                    WhenMarkingJunk =
                        new SchrodingersString("Message when marking a junk item at the location"),
                    WhenMarkingProgression =
                        new SchrodingersString(
                            "Message when marking an item that's potentially progression at the location"),
                    OutOfLogic =
                        new SchrodingersString("Message when tracking an item at the location out of logic")
                }
            };
        }
    }
}
