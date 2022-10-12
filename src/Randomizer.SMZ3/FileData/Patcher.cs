using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.SMZ3.FileData.Patches;
using Randomizer.SMZ3.Text;

using static Randomizer.SMZ3.FileData.DropPrize;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Services;

namespace Randomizer.SMZ3.FileData
{
    public class Patcher
    {
        private readonly List<World> _allWorlds;
        private readonly World _myWorld;
        private readonly string _seedGuid;
        private readonly int _seed;
        private readonly Random _rnd;
        private readonly RomPatchFactory _romPatchFactory;
        private readonly GameLinesConfig _gameLines;
        private readonly IMetadataService _metadataService;
        private StringTable _stringTable;
        private List<(int offset, byte[] bytes)> _patches;
        private Queue<byte> _shuffledSoundtrack;
        private bool _enableMultiworld;

        public Patcher(World myWorld, List<World> allWorlds, string seedGuid, int seed, Random rnd, IMetadataService metadataService, GameLinesConfig gameLines)
        {
            _myWorld = myWorld;
            _allWorlds = allWorlds;
            _seedGuid = seedGuid;
            _seed = seed;
            _rnd = rnd;
            _romPatchFactory = new RomPatchFactory();
            _enableMultiworld = true;
            _gameLines = gameLines;
            _metadataService = metadataService;
        }

        /// <summary>
        /// Returns the PC offset for the specified SNES address.
        /// </summary>
        /// <param name="addr">The SNES address to convert.</param>
        /// <returns>
        /// The PC offset equivalent to the SNES <paramref name="addr"/>.
        /// </returns>
        public static int Snes(int addr)
        {
            addr = addr switch
            {
                /* Redirect hi bank $30 access into ExHiRom lo bank $40 */
                _ when (addr & 0xFF8000) == 0x308000 => 0x400000 | (addr & 0x7FFF),
                /* General case, add ExHi offset for banks < $80, and collapse mirroring */
                _ => (addr < 0x800000 ? 0x400000 : 0) | (addr & 0x3FFFFF),
            };
            if (addr > 0x600000)
                throw new InvalidOperationException($"Unmapped pc address target ${addr:X}");
            return addr;
        }

        /// <summary>
        /// Returns a byte array representing the specified 32-bit unsigned
        /// integer.
        /// </summary>
        /// <param name="value">The 32-bit unsigned integer.</param>
        /// <returns>
        /// A new byte array containing the 32-bit unsigned integer.
        /// </returns>
        public static byte[] UintBytes(int value) => BitConverter.GetBytes((uint)value);

        /// <summary>
        /// Returns a byte array representing the specified 16-bit unsigned
        /// integer.
        /// </summary>
        /// <param name="value">The 16-bit unsigned integer.</param>
        /// <returns>
        /// A new byte array containing the 16-bit unsigned integer.
        /// </returns>
        public static byte[] UshortBytes(int value) => BitConverter.GetBytes((ushort)value);

        /// <summary>
        /// Returns a byte array representing the specified ASCII-encoded text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        /// A new byte array containing the ASCII representation of the
        /// <paramref name="text"/>.
        /// </returns>
        public static byte[] AsAscii(string text) => Encoding.ASCII.GetBytes(text);

        public Dictionary<int, byte[]> CreatePatch(Config config, IEnumerable<string> hints)
        {
            _stringTable = new StringTable();
            _patches = new List<(int, byte[])>();

            WriteMedallions();
            WriteRewards();
            WriteRngBlock();
            WriteDiggingGameRng();
            WritePrizeShuffle();

            WriteDungeonMusic(config);

            WriteRemoveEquipmentFromUncle(_myWorld.HyruleCastle.LinksUncle.Item);

            WriteGanonInvicible(config.GanonInvincible);

            // WritePreopenCurtains();
            WriteQuickSwap();

            WriteSaveAndQuitFromBossRoom();
            WriteWorldOnAgahnimDeath();

            WriteTexts(config, hints);

            WriteSMLocations(_myWorld.Regions.OfType<SMRegion>().SelectMany(x => x.Locations));
            WriteZ3Locations(_myWorld.Regions.OfType<Z3Region>().SelectMany(x => x.Locations));

            WriteStringTable();

            WriteSMKeyCardDoors();
            WriteZ3KeysanityFlags();

            WritePlayerNames();
            WriteSeedData();
            WriteGameTitle();
            WriteCommonFlags();

            foreach (var patch in _romPatchFactory.GetPatches())
            {
                _patches.AddRange(patch.GetChanges(config));
            }

            return _patches.ToDictionary(x => x.offset, x => x.bytes);
        }

        protected void AddPatch(int[] addresses, byte[] values)
        {
            foreach (var address in addresses)
            {
                _patches.Add((Snes(address), values));
            }
        }

        private void WriteMedallions()
        {
            var turtleRockAddresses = new int[] { 0x308023, 0xD020, 0xD0FF, 0xD1DE };
            var miseryMireAddresses = new int[] { 0x308022, 0xCFF2, 0xD0D1, 0xD1B0 };

            var turtleRockValues = _myWorld.TurtleRock.Medallion switch
            {
                ItemType.Bombos => new byte[] { 0x00, 0x51, 0x10, 0x00 },
                ItemType.Ether => new byte[] { 0x01, 0x51, 0x18, 0x00 },
                ItemType.Quake => new byte[] { 0x02, 0x14, 0xEF, 0xC4 },
                var x => throw new InvalidOperationException($"Tried using {x} in place of Turtle Rock medallion")
            };

            var miseryMireValues = _myWorld.MiseryMire.Medallion switch
            {
                ItemType.Bombos => new byte[] { 0x00, 0x51, 0x00, 0x00 },
                ItemType.Ether => new byte[] { 0x01, 0x13, 0x9F, 0xF1 },
                ItemType.Quake => new byte[] { 0x02, 0x51, 0x08, 0x00 },
                var x => throw new InvalidOperationException($"Tried using {x} in place of Misery Mire medallion")
            };

            _patches.AddRange(turtleRockAddresses.Zip(turtleRockValues, (i, b) => (Snes(i), new byte[] { b })));
            _patches.AddRange(miseryMireAddresses.Zip(miseryMireValues, (i, b) => (Snes(i), new byte[] { b })));
        }

        private void WriteRewards()
        {
            var crystalsBlue = new[] { 1, 2, 3, 4, 7 }.Shuffle(_rnd);
            var crystalsRed = new[] { 5, 6 }.Shuffle(_rnd);
            var crystalRewards = crystalsBlue.Concat(crystalsRed);

            var pendantRewards = new[] { 1, 2, 3 };

            var regions = _myWorld.Regions.OfType<IHasReward>();
            var crystalRegions = regions.Where(x => x.RewardType == RewardType.CrystalBlue).Concat(regions.Where(x => x.RewardType == RewardType.CrystalRed));
            var pendantRegions = regions.Where(x => x.RewardType is RewardType.PendantGreen or RewardType.PendantRed or RewardType.PendantBlue).OrderBy(r => (int)r.RewardType);

            _patches.AddRange(RewardPatches(crystalRegions, crystalRewards, CrystalValues));
            _patches.AddRange(RewardPatches(pendantRegions, pendantRewards, PendantValues));
        }

        private IEnumerable<(int, byte[])> RewardPatches(IEnumerable<IHasReward> regions, IEnumerable<int> rewards, Func<int, byte[]> rewardValues)
        {
            var addresses = regions.Select(RewardAddresses);
            var values = rewards.Select(rewardValues);
            var associations = addresses.Zip(values, (a, v) => (a, v));
            return associations.SelectMany(x => x.a.Zip(x.v, (i, b) => (Snes(i), new byte[] { b })));
        }

        private int[] RewardAddresses(IHasReward region)
        {
            return region switch
            {
                EasternPalace _ => new[] { 0x2A09D, 0xABEF8, 0xABEF9, 0x308052, 0x30807C, 0x1C6FE },
                DesertPalace _ => new[] { 0x2A09E, 0xABF1C, 0xABF1D, 0x308053, 0x308078, 0x1C6FF },
                TowerOfHera _ => new[] { 0x2A0A5, 0xABF0A, 0xABF0B, 0x30805A, 0x30807A, 0x1C706 },
                PalaceOfDarkness _ => new[] { 0x2A0A1, 0xABF00, 0xABF01, 0x308056, 0x30807D, 0x1C702 },
                SwampPalace _ => new[] { 0x2A0A0, 0xABF6C, 0xABF6D, 0x308055, 0x308071, 0x1C701 },
                SkullWoods _ => new[] { 0x2A0A3, 0xABF12, 0xABF13, 0x308058, 0x30807B, 0x1C704 },
                ThievesTown _ => new[] { 0x2A0A6, 0xABF36, 0xABF37, 0x30805B, 0x308077, 0x1C707 },
                IcePalace _ => new[] { 0x2A0A4, 0xABF5A, 0xABF5B, 0x308059, 0x308073, 0x1C705 },
                MiseryMire _ => new[] { 0x2A0A2, 0xABF48, 0xABF49, 0x308057, 0x308075, 0x1C703 },
                TurtleRock _ => new[] { 0x2A0A7, 0xABF24, 0xABF25, 0x30805C, 0x308079, 0x1C708 },
                var x => throw new InvalidOperationException($"Region {x} should not be a dungeon reward region")
            };
        }

        private byte[] CrystalValues(int crystal)
        {
            return crystal switch
            {
                1 => new byte[] { 0x02, 0x34, 0x64, 0x40, 0x7F, 0x06 },
                2 => new byte[] { 0x10, 0x34, 0x64, 0x40, 0x79, 0x06 },
                3 => new byte[] { 0x40, 0x34, 0x64, 0x40, 0x6C, 0x06 },
                4 => new byte[] { 0x20, 0x34, 0x64, 0x40, 0x6D, 0x06 },
                5 => new byte[] { 0x04, 0x32, 0x64, 0x40, 0x6E, 0x06 },
                6 => new byte[] { 0x01, 0x32, 0x64, 0x40, 0x6F, 0x06 },
                7 => new byte[] { 0x08, 0x34, 0x64, 0x40, 0x7C, 0x06 },
                var x => throw new InvalidOperationException($"Tried using {x} as a crystal number")
            };
        }

        private byte[] PendantValues(int pendant)
        {
            return pendant switch
            {
                1 => new byte[] { 0x04, 0x38, 0x62, 0x00, 0x69, 0x01 },
                2 => new byte[] { 0x01, 0x32, 0x60, 0x00, 0x69, 0x03 },
                3 => new byte[] { 0x02, 0x34, 0x60, 0x00, 0x69, 0x02 },
                var x => throw new InvalidOperationException($"Tried using {x} as a pendant number")
            };
        }

        private void WriteSMLocations(IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                if (_enableMultiworld)
                {
                    _patches.Add((Snes(location.RomAddress), UshortBytes(GetSMItemPLM(location))));
                    _patches.Add(ItemTablePatch(location, GetZ3ItemId(location)));
                }
                else
                {
                    var plmId = GetSMItemPLM(location);
                    _patches.Add((Snes(location.RomAddress), UshortBytes(plmId)));
                    if (plmId >= 0xEFE0)
                        _patches.Add((Snes(location.RomAddress + 5), new byte[] { GetZ3ItemId(location) }));
                }
            }
        }

        private ushort GetSMItemPLM(Location location)
        {
            var plmId = _enableMultiworld ?
                0xEFE0 :
                location.Item.Type switch
                {
                    ItemType.ETank => 0xEED7,
                    ItemType.Missile => 0xEEDB,
                    ItemType.Super => 0xEEDF,
                    ItemType.PowerBomb => 0xEEE3,
                    ItemType.Bombs => 0xEEE7,
                    ItemType.Charge => 0xEEEB,
                    ItemType.Ice => 0xEEEF,
                    ItemType.HiJump => 0xEEF3,
                    ItemType.SpeedBooster => 0xEEF7,
                    ItemType.Wave => 0xEEFB,
                    ItemType.Spazer => 0xEEFF,
                    ItemType.SpringBall => 0xEF03,
                    ItemType.Varia => 0xEF07,
                    ItemType.Plasma => 0xEF13,
                    ItemType.Grapple => 0xEF17,
                    ItemType.Morph => 0xEF23,
                    ItemType.ReserveTank => 0xEF27,
                    ItemType.Gravity => 0xEF0B,
                    ItemType.XRay => 0xEF0F,
                    ItemType.SpaceJump => 0xEF1B,
                    ItemType.ScrewAttack => 0xEF1F,
                    _ => 0xEFE0,
                };

            plmId += plmId switch
            {
                0xEFE0 => location.Type switch
                {
                    LocationType.Chozo => 4,
                    LocationType.Hidden => 8,
                    _ => 0
                },
                _ => location.Type switch
                {
                    LocationType.Chozo => 0x54,
                    LocationType.Hidden => 0xA8,
                    _ => 0
                }
            };

            return (ushort)plmId;
        }

        private void WriteZ3Locations(IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                if (location.Type == LocationType.HeraStandingKey)
                {
                    _patches.Add((Snes(0x9E3BB), location.Item.Type == ItemType.KeyTH ? new byte[] { 0xE4 } : new byte[] { 0xEB }));
                }
                else if (new[] { LocationType.Pedestal, LocationType.Ether, LocationType.Bombos }.Contains(location.Type))
                {
                    var text = Texts.ItemTextbox(location.Item);
                    var dialog = Dialog.Simple(text);
                    if (location.Type == LocationType.Pedestal)
                    {
                        _patches.Add((Snes(0x308300), dialog));
                        _stringTable.SetPedestalText(text);
                    }
                    else if (location.Type == LocationType.Ether)
                    {
                        _patches.Add((Snes(0x308F00), dialog));
                        _stringTable.SetEtherText(text);
                    }
                    else if (location.Type == LocationType.Bombos)
                    {
                        _patches.Add((Snes(0x309000), dialog));
                        _stringTable.SetBombosText(text);
                    }
                }

                if (_enableMultiworld)
                {
                    _patches.Add((Snes(location.RomAddress), new byte[] { (byte)(location.Id - 256) }));
                    _patches.Add(ItemTablePatch(location, GetZ3ItemId(location)));
                }
                else
                {
                    _patches.Add((Snes(location.RomAddress), new byte[] { GetZ3ItemId(location) }));
                }
            }
        }

        private byte GetZ3ItemId(Location location)
        {
            var item = location.Item;
            var value = location.Type == LocationType.NotInDungeon ||
                !(item.IsDungeonItem && location.Region.IsRegionItem(item) && item.World == _myWorld) ? item.Type : item switch
                {
                    _ when item.IsKey => ItemType.Key,
                    _ when item.IsBigKey => ItemType.BigKey,
                    _ when item.IsMap => ItemType.Map,
                    _ when item.IsCompass => ItemType.Compass,
                    _ => throw new InvalidOperationException($"Tried replacing {item} with a dungeon region item"),
                };
            return (byte)value;
        }

        private (int, byte[]) ItemTablePatch(Location location, byte itemId)
        {
            var type = location.Item.World == location.Region.World ? 0 : 1;
            var owner = location.Item.World.Id;
            return (0x386000 + (location.Id * 8), new[] { type, itemId, owner, 0 }.SelectMany(UshortBytes).ToArray());
        }

        private void WriteDungeonMusic(Config config)
        {
            foreach (var region in _myWorld.Regions.OfType<Z3Region>())
            {
                var addresses = GetMusicAddresses(region.GetType());
                if (addresses == null)
                    continue;

                var soundtrack = SelectSoundtrack(region, config.ExtendedMsuSupport, config.ShuffleDungeonMusic);
                if (soundtrack == null)
                    continue;

                AddPatch(addresses, new byte[] { soundtrack.Value });
                Debug.WriteLine($"Set {region.Name} dungeon music to {GetSoundtrackTitle(soundtrack.Value)}");
            }
        }

        private string GetSoundtrackTitle(byte soundtrackValue)
        {
            return Enum.GetName(typeof(ALttpExtendedSoundtrack), soundtrackValue)
                ?? Enum.GetName(typeof(ALttPSoundtrack), soundtrackValue)
                ?? $"Unknown soundtrack (0x{soundtrackValue:X2})";
        }

        private int[] GetMusicAddresses(Type regionType)
        {
            var musicAddresses = regionType.GetField("MusicAddresses",
                BindingFlags.Public | BindingFlags.Static);

            return musicAddresses != null
                ? (int[])musicAddresses.GetValue(null)
                : null;
        }

        private byte? SelectSoundtrack(Region region, bool extended, MusicShuffleMode shuffleMode) => shuffleMode switch
        {
            MusicShuffleMode.Default => GetDefaultSoundtrack(region, extended),
            MusicShuffleMode.ShuffleDungeons => GetRandomSoundtrack(false, extended),
            MusicShuffleMode.ShuffleAll => GetRandomSoundtrack(true, extended),
            _ => throw new InvalidEnumArgumentException(nameof(shuffleMode), (int)shuffleMode, typeof(MusicShuffleMode))
        };

        private byte? GetDefaultSoundtrack(Region region, bool extended)
        {
            if (extended)
            {
                var soundtrack = region switch
                {
                    EasternPalace => ALttpExtendedSoundtrack.EasternPalace,
                    DesertPalace => ALttpExtendedSoundtrack.DesertPalace,
                    SwampPalace => ALttpExtendedSoundtrack.SwampPalace,
                    PalaceOfDarkness => ALttpExtendedSoundtrack.PalaceOfDarkness,
                    MiseryMire => ALttpExtendedSoundtrack.MiseryMire,
                    SkullWoods => ALttpExtendedSoundtrack.SkullWoods,
                    IcePalace => ALttpExtendedSoundtrack.IcePalace,
                    TowerOfHera => ALttpExtendedSoundtrack.TowerOfHera,
                    ThievesTown => ALttpExtendedSoundtrack.ThievesTown,
                    TurtleRock => ALttpExtendedSoundtrack.TurtleRock,
                    GanonsTower => ALttpExtendedSoundtrack.GanonsTower,
                    _ => throw new ArgumentOutOfRangeException(nameof(region),
                        "Region does not support extended soundtrack.")
                };
                return (byte)soundtrack;
            }
            else if (region is IHasReward dungeonRegion)
            {
                ALttPSoundtrack? soundtrack = dungeonRegion.RewardType switch
                {
                    RewardType.PendantGreen => ALttPSoundtrack.LightWorldDungeon,
                    RewardType.PendantRed => ALttPSoundtrack.LightWorldDungeon,
                    RewardType.PendantBlue => ALttPSoundtrack.LightWorldDungeon,
                    RewardType.CrystalBlue => ALttPSoundtrack.DarkWorldDungeon,
                    RewardType.CrystalRed => ALttPSoundtrack.DarkWorldDungeon,
                    _ => null
                };
                return soundtrack != null
                    ? (byte)soundtrack
                    : null;
            }

            return null;
        }

        private byte GetRandomSoundtrack(bool includeNonDungeonMusic, bool extended)
        {
            if (_shuffledSoundtrack == null)
            {
                _shuffledSoundtrack = new Queue<byte>(includeNonDungeonMusic
                    ? ShuffleSoundtrack(extended)
                    : ShuffleDungeonSoundtracks(extended));
            }

            return _shuffledSoundtrack.Dequeue();
        }

        private IEnumerable<byte> ShuffleDungeonSoundtracks(bool extended)
        {
            var list = new List<byte>();
            if (!extended)
            {
                list.AddRange(Enumerable.Repeat((byte)ALttPSoundtrack.LightWorldDungeon, 6));
                list.AddRange(Enumerable.Repeat((byte)ALttPSoundtrack.DarkWorldDungeon, 6));
            }
            else
            {
                list.Add((byte)ALttPSoundtrack.LightWorldDungeon);
                list.Add((byte)ALttPSoundtrack.DarkWorldDungeon);
                list.AddRange(Enumerable.Range(35, 12).Select(x => (byte)x));
            }
            return list.Shuffle(_rnd);
        }

        private IEnumerable<byte> ShuffleSoundtrack(bool extended)
        {
            var list = new List<byte>();
            list.AddRange(Enum.GetValues<ALttPSoundtrack>().Cast<byte>());
            if (extended)
                list.AddRange(Enum.GetValues<ALttpExtendedSoundtrack>().Cast<byte>());
            list.Remove(0x00);
            return list.Shuffle(_rnd);
        }

        private void WritePrizeShuffle()
        {
            const int prizePackItems = 56;
            const int treePullItems = 3;

            IEnumerable<byte> bytes;
            byte drop, final;

            var pool = new DropPrize[] {
                Heart, Heart, Heart, Heart, Green, Heart, Heart, Green,         // pack 1
                Blue, Green, Blue, Red, Blue, Green, Blue, Blue,                // pack 2
                FullMagic, Magic, Magic, Blue, FullMagic, Magic, Heart, Magic,  // pack 3
                Bomb1, Bomb1, Bomb1, Bomb4, Bomb1, Bomb1, Bomb8, Bomb1,         // pack 4
                Arrow5, Heart, Arrow5, Arrow10, Arrow5, Heart, Arrow5, Arrow10, // pack 5
                Magic, Green, Heart, Arrow5, Magic, Bomb1, Green, Heart,        // pack 6
                Heart, Fairy, FullMagic, Red, Bomb8, Heart, Red, Arrow10,       // pack 7
                Green, Blue, Red, // from pull trees
                Green, Red, // from prize crab
                Green, // stunned prize
                Red, // saved fish prize
            }.AsEnumerable();

            var prizes = pool.Shuffle(_rnd).Cast<byte>();

            /* prize pack drop order */
            (bytes, prizes) = prizes.SplitOff(prizePackItems);
            _patches.Add((Snes(0x6FA78), bytes.ToArray()));

            /* tree pull prizes */
            (bytes, prizes) = prizes.SplitOff(treePullItems);
            _patches.Add((Snes(0x1DFBD4), bytes.ToArray()));

            /* crab prizes */
            (drop, final, prizes) = prizes;
            _patches.Add((Snes(0x6A9C8), new[] { drop }));
            _patches.Add((Snes(0x6A9C4), new[] { final }));

            /* stun prize */
            (drop, prizes) = prizes;
            _patches.Add((Snes(0x6F993), new[] { drop }));

            /* fish prize */
            (drop, _) = prizes;
            _patches.Add((Snes(0x1D82CC), new[] { drop }));

            _patches.AddRange(EnemyPrizePackDistribution());

            /* Pack drop chance */
            /* Normal difficulty is 50%. 0 => 100%, 1 => 50%, 3 => 25% */
            const int nrPacks = 7;
            const byte probability = 1;
            _patches.Add((Snes(0x6FA62), Enumerable.Repeat(probability, nrPacks).ToArray()));
        }

        private IEnumerable<(int, byte[])> EnemyPrizePackDistribution()
        {
            var (prizePacks, duplicatePacks) = EnemyPrizePacks();

            var n = prizePacks.Sum(x => x.bytes.Length);
            var randomization = PrizePackRandomization(n, 1);

            var patches = prizePacks.Select(x =>
            {
                IEnumerable<byte> packs;
                (packs, randomization) = randomization.SplitOff(x.bytes.Length);
                return (x.offset, bytes: x.bytes.Zip(packs, (b, p) => (byte)(b | p)).ToArray());
            }).ToList();

            var duplicates =
                from d in duplicatePacks
                from p in patches
                where p.offset == d.src
                select (d.dest, p.bytes);
            patches.AddRange(duplicates.ToList());

            return patches.Select(x => (Snes(x.offset), x.bytes));
        }

        /* Guarantees at least s of each prize pack, over a total of n packs.
         * In each iteration, from the product n * m, use the guaranteed number
         * at k, where k is the "row" (integer division by m), when k falls
         * within the list boundary. Otherwise use the "column" (modulo by m)
         * as the random element.
         */

        private IEnumerable<byte> PrizePackRandomization(int n, int s)
        {
            const int m = 7;
            var g = Enumerable.Repeat(Enumerable.Range(0, m), s)
                .SelectMany(x => x)
                .ToList();

            IEnumerable<int> randomization(int n)
            {
                n = m * n;
                while (n > 0)
                {
                    var r = _rnd.Next(n);
                    var k = r / m;
                    yield return k < g.Count ? g[k] : r % m;
                    if (k < g.Count) g.RemoveAt(k);
                    n -= m;
                }
            }

            return randomization(n).Select(x => (byte)(x + 1)).ToList();
        }

        /* Todo: Deadrock turns into $8F Blob when powdered, but those "onion blobs" always drop prize pack 1. */

        private (IList<(int offset, byte[] bytes)>, IList<(int src, int dest)>) EnemyPrizePacks()
        {
            const int offset = 0xDB632;
            var patches = new[] {
                /* sprite_prep */
                (0x6888D, new byte[] { 0x00 }), // Keese DW
                (0x688A8, new byte[] { 0x00 }), // Rope
                (0x68967, new byte[] { 0x00, 0x00 }), // Crow/Dacto
                (0x69125, new byte[] { 0x00, 0x00 }), // Red/Blue Hardhat Bettle
                /* sprite properties */
                (offset+0x01, new byte[] { 0x90 }), // Vulture
                (offset+0x08, new byte[] { 0x00 }), // Octorok (One Way)
                (offset+0x0A, new byte[] { 0x00 }), // Octorok (Four Way)
                (offset+0x0D, new byte[] { 0x80, 0x90 }), // Buzzblob, Snapdragon
                (offset+0x11, new byte[] { 0x90, 0x90, 0x00 }), // Hinox, Moblin, Mini Helmasaur
                (offset+0x18, new byte[] { 0x90, 0x90 }), // Mini Moldorm, Poe/Hyu
                (offset+0x20, new byte[] { 0x00 }), // Sluggula
                (offset+0x22, new byte[] { 0x80, 0x00, 0x00 }), // Ropa, Red Bari, Blue Bari
                // Blue Soldier/Tarus, Green Soldier, Red Spear Soldier Blue
                // Assault Soldier, Red Assault Spear Soldier/Tarus Blue Archer,
                // Green Archer Red Javelin Soldier, Red Bush Javelin Soldier
                // Red Bomb Soldiers, Green Soldier Recruits, Geldman, Toppo
                (offset+0x41, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x10, 0x90, 0x90, 0x80 }),
                (offset+0x4F, new byte[] { 0x80 }), // Popo 2
                (offset+0x51, new byte[] { 0x80 }), // Armos
                (offset+0x55, new byte[] { 0x00, 0x00 }), // Ku, Zora
                (offset+0x58, new byte[] { 0x90 }), // Crab
                (offset+0x64, new byte[] { 0x80 }), // Devalant (Shooter)
                (offset+0x6A, new byte[] { 0x90, 0x90 }), // Ball N' Chain Trooper, Cannon Soldier
                (offset+0x6D, new byte[] { 0x80, 0x80 }), // Rat/Buzz, (Stal)Rope
                (offset+0x71, new byte[] { 0x80 }), // Leever
                (offset+0x7C, new byte[] { 0x90 }), // Initially Floating Stal
                (offset+0x81, new byte[] { 0xC0 }), // Hover
                // Green Eyegore/Mimic, Red Eyegore/Mimic Detached Stalfos Body,
                // Kodongo
                (offset+0x83, new byte[] { 0x10, 0x10, 0x10, 0x00 }),
                (offset+0x8B, new byte[] { 0x10 }), // Gibdo
                (offset+0x8E, new byte[] { 0x00, 0x00 }), // Terrorpin, Blob
                (offset+0x91, new byte[] { 0x10 }), // Stalfos Knight
                (offset+0x99, new byte[] { 0x10 }), // Pengator
                (offset+0x9B, new byte[] { 0x10 }), // Wizzrobe
                // Blue Zazak, Red Zazak, Stalfos Green Zirro, Blue Zirro, Pikit
                (offset+0xA5, new byte[] { 0x10, 0x10, 0x10, 0x80, 0x80, 0x80 }),
                (offset+0xC7, new byte[] { 0x10 }), // Hokku-Bokku
                (offset+0xC9, new byte[] { 0x10 }), // Tektite
                (offset+0xD0, new byte[] { 0x10 }), // Lynel
                (offset+0xD3, new byte[] { 0x00 }), // Stal
            };
            var duplicates = new[] {
                /* Popo2 -> Popo. Popo is not used in vanilla Z3, but we duplicate from Popo2 just to be sure */
                (offset + 0x4F, offset + 0x4E),
            };
            return (patches, duplicates);
        }

        private void WriteTexts(Config config, IEnumerable<string> hints)
        {
            var regions = _myWorld.Regions.OfType<IHasReward>();

            var greenPendantDungeon = regions
                .Where(x => x.RewardType == RewardType.PendantGreen)
                .Select(x => GetRegionName(x as Region))
                .First();

            var redCrystalDungeons = regions
                .Where(x => x.RewardType == RewardType.CrystalRed)
                .Select(x => GetRegionName(x as Region));

            var sahasrahla = Texts.SahasrahlaReveal(greenPendantDungeon);
            _patches.Add((Snes(0x308A00), Dialog.Simple(sahasrahla)));
            _stringTable.SetSahasrahlaRevealText(sahasrahla);

            var bombShop = Texts.BombShopReveal(redCrystalDungeons);
            _patches.Add((Snes(0x308E00), Dialog.Simple(bombShop)));
            _stringTable.SetBombShopRevealText(bombShop);

            var blind = Dialog.GetGameSafeString(_gameLines.BlindIntro);
            _patches.Add((Snes(0x308800), Dialog.Simple(blind)));
            _stringTable.SetBlindText(blind);

            var tavernMan = Texts.TavernMan(_rnd);
            _patches.Add((Snes(0x308C00), Dialog.Simple(tavernMan)));
            _stringTable.SetTavernManText(tavernMan);

            var ganon = Dialog.GetGameSafeString(_gameLines.GanonIntro);
            _patches.Add((Snes(0x308600), Dialog.Simple(ganon)));
            _stringTable.SetGanonFirstPhaseText(ganon);

            // Have bottle merchant and zora say what they have if requested
            if (config.CasPatches.PreventScams)
            {
                var item = GetItemName(config, _myWorld.LightWorldNorthWest.BottleMerchant.Item);
                _stringTable.SetBottleVendorText(Dialog.GetChoiceText(_gameLines.BottleMerchant.Format(item), _gameLines.ChoiceYes, _gameLines.ChoiceNo));

                item = GetItemName(config, _myWorld.LightWorldNorthEast.ZorasDomain.Zora.Item);
                _stringTable.SetZoraText(Dialog.GetChoiceText(_gameLines.KingZora.Format(item), _gameLines.ChoiceYes, _gameLines.ChoiceNo));
            }

            // Todo: Verify these two are correct if ganon invincible patch is
            // ever added ganon_fall_in_alt in v30
            var ganonFirstPhaseInvincible = "You think you\nare ready to\nface me?\n\nI will not die\n\nunless you\ncomplete your\ngoals. Dingus!";
            _patches.Add((Snes(0x309100), Dialog.Simple(ganonFirstPhaseInvincible)));

            // ganon_phase_3_alt in v30
            var ganonThirdPhaseInvincible = "Got wax in\nyour ears?\nI cannot die!";
            _patches.Add((Snes(0x309200), Dialog.Simple(ganonThirdPhaseInvincible)));
            // ---

            var silversLocation = _allWorlds.SelectMany(world => world.Locations).Where(l => l.ItemIs(ItemType.SilverArrows, _myWorld)).FirstOrDefault();
            if (silversLocation != null)
            {
                var silvers = config.MultiWorld
                    ? Texts.GanonThirdPhaseMulti(silversLocation.Region, _myWorld)
                    : Texts.GanonThirdPhaseSingle(silversLocation.Region);
                _patches.Add((Snes(0x308700), Dialog.Simple(silvers)));
                _stringTable.SetGanonThirdPhaseText(silvers);
            }
            else
            {
                var silvers = Texts.GanonThirdPhraseNone();
                _patches.Add((Snes(0x308700), Dialog.Simple(silvers)));
                _stringTable.SetGanonThirdPhaseText(silvers);
            }

            var triforceRoom = Dialog.GetGameSafeString(_gameLines.TriforceRoom);
            _patches.Add((Snes(0x308400), Dialog.Simple(triforceRoom)));
            _stringTable.SetTriforceRoomText(triforceRoom);

            _stringTable.SetHints(hints.Select(x => Dialog.GetGameSafeString(x)));
        }

        private void WriteStringTable()
        {
            // Todo: v12, base table in asm, use move instructions in seed patch
            _patches.Add((Snes(0x1C8000), _stringTable.GetPaddedBytes()));
        }

        private void WritePlayerNames()
        {
            _patches.AddRange(_allWorlds.Select(world => (0x385000 + (world.Id * 16), PlayerNameBytes(world.Player))));
            _patches.Add((0x385000 + (_allWorlds.Count * 16), PlayerNameBytes("Tracker")));
        }

        private byte[] PlayerNameBytes(string name)
        {
            name = name.Length > 12 ? name[..12].TrimEnd() : name;

            const int width = 12;
            var pad = (width - name.Length) / 2;
            name = name.PadLeft(name.Length + pad);
            name = name.PadRight(width);

            return AsAscii(name).Concat(UintBytes(0)).ToArray();
        }

        private void WriteSeedData()
        {
            var configField =
                ((_myWorld.Config.Race ? 1 : 0) << 15) |
                ((_myWorld.Config.Keysanity ? 1 : 0) << 13) |
                ((_enableMultiworld ? 1 : 0) << 12) |
                (Generation.Smz3Randomizer.Version.Major << 4) |
                (Generation.Smz3Randomizer.Version.Minor << 0);

            _patches.Add((Snes(0x80FF50), UshortBytes(_myWorld.Id)));
            _patches.Add((Snes(0x80FF52), UshortBytes(configField)));
            _patches.Add((Snes(0x80FF54), UintBytes(_seed)));
            /* Reserve the rest of the space for future use */
            _patches.Add((Snes(0x80FF58), Enumerable.Repeat<byte>(0x00, 8).ToArray()));
            _patches.Add((Snes(0x80FF60), AsAscii(_seedGuid)));
            _patches.Add((Snes(0x80FF80), AsAscii(_myWorld.Guid)));
        }

        private void WriteCommonFlags()
        {
            /* Common Combo Configuration flags at [asm]/config.asm */
            //if (_enableMultiworld)
            _patches.Add((Snes(0xF47000), UshortBytes(0x0001)));
            if (_myWorld.Config.Keysanity)
                _patches.Add((Snes(0xF47006), UshortBytes(0x0001)));
        }

        private void WriteGameTitle()
        {
            var title = AsAscii($"SMZ3 Cas' [{_seed:X8}]".PadRight(21)[..21]);
            _patches.Add((Snes(0x00FFC0), title));
            _patches.Add((Snes(0x80FFC0), title));
        }

        private void WriteZ3KeysanityFlags()
        {
            if (_myWorld.Config.ZeldaKeysanity)
            {
                _patches.Add((Snes(0x40003B), new byte[] { 1 })); // MapMode #$00 = Always On (default) - #$01 = Require Map Item
                _patches.Add((Snes(0x400045), new byte[] { 0x0f })); // display ----dcba a: Small Keys, b: Big Key, c: Map, d: Compass
            }
            if (_myWorld.Config.Keysanity)
            {
                _patches.Add((Snes(0x40016A), new byte[] { 1 })); // FreeItemText: db #$01 ; #00 = Off (default) - #$01 = On
            }
        }

        private void WriteSMKeyCardDoors()
        {
            if (!_myWorld.Config.MetroidKeysanity)
                return;

            ushort plaquePLm = 0xd410;

            var doorList = new List<ushort[]> {
                            // RoomId Door Facing yyxx Keycard Event Type Plaque
                            // type yyxx, Address (if 0 a dynamic PLM is
                            // created) Crateria
                new ushort[] { 0x91F8, KeycardDoors.Right,      0x2601, KeycardEvents.CrateriaLevel1,        KeycardPlaque.Level1,   0x2400, 0x0000 },  // Crateria - Landing Site - Door to gauntlet
                new ushort[] { 0x91F8, KeycardDoors.Left,       0x168E, KeycardEvents.CrateriaLevel1,        KeycardPlaque.Level1,   0x148F, 0x801E },  // Crateria - Landing Site - Door to landing site PB
                new ushort[] { 0x948C, KeycardDoors.Left,       0x062E, KeycardEvents.CrateriaLevel2,        KeycardPlaque.Level2,   0x042F, 0x8222 },  // Crateria - Before Moat - Door to moat (overwrite PB door)
                new ushort[] { 0x99BD, KeycardDoors.Left,       0x660E, KeycardEvents.CrateriaBoss,          KeycardPlaque.Boss,     0x640F, 0x8470 },  // Crateria - Before G4 - Door to G4
                new ushort[] { 0x9879, KeycardDoors.Left,       0x062E, KeycardEvents.CrateriaBoss,          KeycardPlaque.Boss,     0x042F, 0x8420 },  // Crateria - Before BT - Door to Bomb Torizo

                // Brinstar
                new ushort[] { 0x9F11, KeycardDoors.Left,       0x060E, KeycardEvents.BrinstarLevel1,        KeycardPlaque.Level1,   0x040F, 0x8784 },  // Brinstar - Blue Brinstar - Door to ceiling e-tank room

                new ushort[] { 0x9AD9, KeycardDoors.Right,      0xA601, KeycardEvents.BrinstarLevel2,        KeycardPlaque.Level2,   0xA400, 0x0000 },  // Brinstar - Green Brinstar - Door to etecoon area
                new ushort[] { 0x9D9C, KeycardDoors.Down,       0x0336, KeycardEvents.BrinstarBoss,          KeycardPlaque.Boss,     0x0234, 0x863A },  // Brinstar - Pink Brinstar - Door to spore spawn
                new ushort[] { 0xA130, KeycardDoors.Left,       0x161E, KeycardEvents.BrinstarLevel2,        KeycardPlaque.Level2,   0x141F, 0x881C },  // Brinstar - Pink Brinstar - Door to wave gate e-tank
                new ushort[] { 0xA0A4, KeycardDoors.Left,       0x062E, KeycardEvents.BrinstarLevel2,        KeycardPlaque.Level2,   0x042F, 0x0000 },  // Brinstar - Pink Brinstar - Door to spore spawn super

                new ushort[] { 0xA56B, KeycardDoors.Left,       0x161E, KeycardEvents.BrinstarBoss,          KeycardPlaque.Boss,     0x141F, 0x8A1A },  // Brinstar - Before Kraid - Door to Kraid

                // Upper Norfair
                new ushort[] { 0xA7DE, KeycardDoors.Right,      0x3601, KeycardEvents.NorfairLevel1,         KeycardPlaque.Level1,   0x3400, 0x8B00 },  // Norfair - Business Centre - Door towards Ice
                new ushort[] { 0xA923, KeycardDoors.Right,      0x0601, KeycardEvents.NorfairLevel1,         KeycardPlaque.Level1,   0x0400, 0x0000 },  // Norfair - Pre-Crocomire - Door towards Ice

                new ushort[] { 0xA788, KeycardDoors.Left,       0x162E, KeycardEvents.NorfairLevel2,         KeycardPlaque.Level2,   0x142F, 0x8AEA },  // Norfair - Lava Missile Room - Door towards Bubble Mountain
                new ushort[] { 0xAF72, KeycardDoors.Left,       0x061E, KeycardEvents.NorfairLevel2,         KeycardPlaque.Level2,   0x041F, 0x0000 },  // Norfair - After frog speedway - Door to Bubble Mountain
                new ushort[] { 0xAEDF, KeycardDoors.Down,       0x0206, KeycardEvents.NorfairLevel2,         KeycardPlaque.Level2,   0x0204, 0x0000 },  // Norfair - Below bubble mountain - Door to Bubble Mountain
                new ushort[] { 0xAD5E, KeycardDoors.Right,      0x0601, KeycardEvents.NorfairLevel2,         KeycardPlaque.Level2,   0x0400, 0x0000 },  // Norfair - LN Escape - Door to Bubble Mountain

                new ushort[] { 0xA923, KeycardDoors.Up,         0x2DC6, KeycardEvents.NorfairBoss,           KeycardPlaque.Boss,     0x2EC4, 0x8B96 },  // Norfair - Pre-Crocomire - Door to Crocomire

                // Lower Norfair
                new ushort[] { 0xB4AD, KeycardDoors.Left,       0x160E, KeycardEvents.LowerNorfairLevel1,    KeycardPlaque.Level1,   0x140F, 0x0000 },  // Lower Norfair - WRITG - Door to Amphitheatre
                new ushort[] { 0xAD5E, KeycardDoors.Left,       0x065E, KeycardEvents.LowerNorfairLevel1,    KeycardPlaque.Level1,   0x045F, 0x0000 },  // Lower Norfair - Exit - Door to "Reverse LN Entry"
                new ushort[] { 0xB37A, KeycardDoors.Right,      0x0601, KeycardEvents.LowerNorfairBoss,      KeycardPlaque.Boss,     0x0400, 0x8EA6 },  // Lower Norfair - Pre-Ridley - Door to Ridley

                // Maridia
                new ushort[] { 0xD0B9, KeycardDoors.Left,       0x065E, KeycardEvents.MaridiaLevel1,         KeycardPlaque.Level1,   0x045F, 0x0000 },  // Maridia - Mt. Everest - Door to Pink Maridia
                new ushort[] { 0xD5A7, KeycardDoors.Right,      0x1601, KeycardEvents.MaridiaLevel1,         KeycardPlaque.Level1,   0x1400, 0x0000 },  // Maridia - Aqueduct - Door towards Beach

                new ushort[] { 0xD617, KeycardDoors.Left,       0x063E, KeycardEvents.MaridiaLevel2,         KeycardPlaque.Level2,   0x043F, 0x0000 },  // Maridia - Pre-Botwoon - Door to Botwoon
                new ushort[] { 0xD913, KeycardDoors.Right,      0x2601, KeycardEvents.MaridiaLevel2,         KeycardPlaque.Level2,   0x2400, 0x0000 },  // Maridia - Pre-Colloseum - Door to post-botwoon

                new ushort[] { 0xD78F, KeycardDoors.Right,      0x2601, KeycardEvents.MaridiaBoss,           KeycardPlaque.Boss,     0x2400, 0xC73B },  // Maridia - Precious Room - Door to Draygon

                new ushort[] { 0xDA2B, KeycardDoors.BossLeft,   0x164E, 0x00f0, /* Door id 0xf0 */           KeycardPlaque.None,     0x144F, 0x0000 },  // Maridia - Change Cac Alley Door to Boss Door (prevents key breaking)

                // Wrecked Ship
                new ushort[] { 0x93FE, KeycardDoors.Left,       0x167E, KeycardEvents.WreckedShipLevel1,     KeycardPlaque.Level1,   0x147F, 0x0000 },  // Wrecked Ship - Outside Wrecked Ship West - Door to Reserve Tank Check
                new ushort[] { 0x968F, KeycardDoors.Left,       0x060E, KeycardEvents.WreckedShipLevel1,     KeycardPlaque.Level1,   0x040F, 0x0000 },  // Wrecked Ship - Outside Wrecked Ship West - Door to Bowling Alley
                new ushort[] { 0xCE40, KeycardDoors.Left,       0x060E, KeycardEvents.WreckedShipLevel1,     KeycardPlaque.Level1,   0x040F, 0x0000 },  // Wrecked Ship - Gravity Suit - Door to Bowling Alley

                new ushort[] { 0xCC6F, KeycardDoors.Left,       0x064E, KeycardEvents.WreckedShipBoss,       KeycardPlaque.Boss,     0x044F, 0xC29D },  // Wrecked Ship - Pre-Phantoon - Door to Phantoon
            };

            ushort doorId = 0x0000;
            var plmTablePos = 0xf800;
            foreach (var door in doorList)
            {
                var doorArgs = door[4] != KeycardPlaque.None ? doorId | door[3] : door[3];
                if (door[6] == 0)
                {
                    // Write dynamic door
                    var doorData = door[0..3].SelectMany(x => UshortBytes(x)).Concat(UshortBytes(doorArgs)).ToArray();
                    _patches.Add((Snes(0x8f0000 + plmTablePos), doorData));
                    plmTablePos += 0x08;
                }
                else
                {
                    // Overwrite existing door
                    var doorData = door[1..3].SelectMany(x => UshortBytes(x)).Concat(UshortBytes(doorArgs)).ToArray();
                    _patches.Add((Snes(0x8f0000 + door[6]), doorData));
                    if ((door[3] == KeycardEvents.BrinstarBoss && door[0] != 0x9D9C)
                        || door[3] == KeycardEvents.LowerNorfairBoss
                        || door[3] == KeycardEvents.MaridiaBoss
                        || door[3] == KeycardEvents.WreckedShipBoss)
                        // Overwrite the extra parts of the Gadora with a PLM
                        // that just deletes itself
                        _patches.Add((Snes(0x8f0000 + door[6] + 0x06), new byte[] { 0x2F, 0xB6, 0x00, 0x00, 0x00, 0x00, 0x2F, 0xB6, 0x00, 0x00, 0x00, 0x00 }));
                }

                // Plaque data
                if (door[4] != KeycardPlaque.None)
                {
                    var plaqueData = UshortBytes(door[0]).Concat(UshortBytes(plaquePLm)).Concat(UshortBytes(door[5])).Concat(UshortBytes(door[4])).ToArray();
                    _patches.Add((Snes(0x8f0000 + plmTablePos), plaqueData));
                    plmTablePos += 0x08;
                }
                doorId += 1;
            }

            _patches.Add((Snes(0x8f0000 + plmTablePos), new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
        }

        private void WriteDiggingGameRng()
        {
            var digs = (byte)(_rnd.Next(30) + 1);
            _patches.Add((Snes(0x308020), new byte[] { digs }));
            _patches.Add((Snes(0x1DFD95), new byte[] { digs }));
        }

        // Removes Sword/Shield from Uncle by moving the tiles for sword/shield
        // to his head and replaces them with his head.
        private void WriteRemoveEquipmentFromUncle(Item item)
        {
            if (item.Type != ItemType.ProgressiveSword)
            {
                _patches.AddRange(new[] {
                    (Snes(0xDD263), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E }),
                    (Snes(0xDD26B), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E }),
                    (Snes(0xDD293), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E }),
                    (Snes(0xDD29B), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x00, 0x0E }),
                    (Snes(0xDD2B3), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x02, 0x0E }),
                    (Snes(0xDD2BB), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x02, 0x0E }),
                    (Snes(0xDD2E3), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x02, 0x0E }),
                    (Snes(0xDD2EB), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x02, 0x0E }),
                    (Snes(0xDD31B), new byte[] { 0x00, 0x00, 0xE4, 0xFF, 0x08, 0x0E }),
                    (Snes(0xDD323), new byte[] { 0x00, 0x00, 0xE4, 0xFF, 0x08, 0x0E }),
                });
            }
            if (item.Type != ItemType.ProgressiveShield)
            {
                _patches.AddRange(new[] {
                    (Snes(0xDD253), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E }),
                    (Snes(0xDD25B), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E }),
                    (Snes(0xDD283), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E }),
                    (Snes(0xDD28B), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x00, 0x0E }),
                    (Snes(0xDD2CB), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x02, 0x0E }),
                    (Snes(0xDD2FB), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x02, 0x0E }),
                    (Snes(0xDD313), new byte[] { 0x00, 0x00, 0xE4, 0xFF, 0x08, 0x0E }),
                });
            }
        }

        private void WriteGanonInvicible(GanonInvincible invincible)
        {
            /* Defaults to $00 (never) at [asm]/z3/randomizer/tables.asm */
            var value = invincible switch
            {
                GanonInvincible.Never => 0x00,
                GanonInvincible.Always => 0x01,
                GanonInvincible.BeforeAllDungeons => 0x02,
                GanonInvincible.BeforeCrystals => 0x03,
                var x => throw new ArgumentException($"Unknown Ganon invincible value {x}", nameof(invincible))
            };
            _patches.Add((Snes(0x30803E), new byte[] { (byte)value }));
        }

        private void WritePreopenCurtains()
        {
            // #$00 = Off (default) - #$01 = On
            _patches.Add((Snes(0x308040), new byte[] { 0x01 }));
        }

        private void WriteQuickSwap()
        {
            // #$00 = Off (default) - #$01 = On
            _patches.Add((Snes(0x30804B), new byte[] { 0x01 }));
        }

        private void WriteRngBlock()
        {
            /* Repoint RNG Block */
            _patches.Add((0x420000, Enumerable.Range(0, 1024).Select(x => (byte)_rnd.Next(0x100)).ToArray()));
        }

        private void WriteSaveAndQuitFromBossRoom()
        {
            /* Defaults to $00 at [asm]/z3/randomizer/tables.asm */
            _patches.Add((Snes(0x308042), new byte[] { 0x01 }));
        }

        private void WriteWorldOnAgahnimDeath()
        {
            /* Defaults to $01 at [asm]/z3/randomizer/tables.asm */
            // Todo: Z3r major glitches disables this, reconsider extending or dropping with glitched logic later.
            //patches.Add((Snes(0x3080A3), new byte[] { 0x01 }));
        }

        private string GetRegionName(Region region)
        {
            return Dialog.GetGameSafeString(_metadataService.Region(region).Name);
        }

        private string GetItemName(Config config, Item item)
        {
            var itemName = _metadataService.Item(item.Type).NameWithArticle;
            if (!config.MultiWorld)
            {
                return itemName;
            }
            else
            {
                return _myWorld == item.World
                    ? $"{itemName} belonging to you"
                    : $"{itemName} belonging to another player"; // Will need to update to player name when multiworld is working
            }
        }
    }
}
