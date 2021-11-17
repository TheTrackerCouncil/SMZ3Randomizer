using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3
{
    public class Progression
    {
        private int shield;

        public Progression()
        {
        }

        public Progression(IEnumerable<Item> items)
        {
            Add(items);
        }

        public bool BigKeyEP { get; private set; }
        public bool BigKeyDP { get; private set; }
        public bool BigKeyTH { get; private set; }
        public bool BigKeyPD { get; private set; }
        public bool BigKeySP { get; private set; }
        public bool BigKeySW { get; private set; }
        public bool BigKeyTT { get; private set; }
        public bool BigKeyIP { get; private set; }
        public bool BigKeyMM { get; private set; }
        public bool BigKeyTR { get; private set; }
        public bool BigKeyGT { get; private set; }
        public bool KeyHC { get; private set; }
        public bool KeyDP { get; private set; }
        public bool KeyTH { get; private set; }
        public bool KeySP { get; private set; }
        public bool KeyTT { get; private set; }
        public int KeyCT { get; private set; }
        public int KeyPD { get; private set; }
        public int KeySW { get; private set; }
        public int KeyIP { get; private set; }
        public int KeyMM { get; private set; }
        public int KeyTR { get; private set; }
        public int KeyGT { get; private set; }
        public bool CardCrateriaL1 { get; private set; }
        public bool CardCrateriaL2 { get; private set; }
        public bool CardCrateriaBoss { get; private set; }
        public bool CardBrinstarL1 { get; private set; }
        public bool CardBrinstarL2 { get; private set; }
        public bool CardBrinstarBoss { get; private set; }
        public bool CardNorfairL1 { get; private set; }
        public bool CardNorfairL2 { get; private set; }
        public bool CardNorfairBoss { get; private set; }
        public bool CardMaridiaL1 { get; private set; }
        public bool CardMaridiaL2 { get; private set; }
        public bool CardMaridiaBoss { get; private set; }
        public bool CardWreckedShipL1 { get; private set; }
        public bool CardWreckedShipBoss { get; private set; }
        public bool CardLowerNorfairL1 { get; private set; }
        public bool CardLowerNorfairBoss { get; private set; }
        public bool CanBlockLasers => shield >= 3;
        public bool Sword { get; private set; }
        public bool MasterSword { get; private set; }
        public bool Bow { get; private set; }
        public bool Hookshot { get; private set; }
        public bool Mushroom { get; private set; }
        public bool Powder { get; private set; }
        public bool Firerod { get; private set; }
        public bool Icerod { get; private set; }
        public bool Bombos { get; private set; }
        public bool Ether { get; private set; }
        public bool Quake { get; private set; }
        public bool Lamp { get; private set; }
        public bool Hammer { get; private set; }
        public bool Shovel { get; private set; }
        public bool Flute { get; private set; }
        public bool Book { get; private set; }
        public bool Bottle { get; private set; }
        public bool Somaria { get; private set; }
        public bool Byrna { get; private set; }
        public bool Cape { get; private set; }
        public bool Mirror { get; private set; }
        public bool Boots { get; private set; }
        public bool Glove { get; private set; }
        public bool Mitt { get; private set; }
        public bool Flippers { get; private set; }
        public bool MoonPearl { get; private set; }
        public bool HalfMagic { get; private set; }
        public bool Grapple { get; private set; }
        public bool Charge { get; private set; }
        public bool Ice { get; private set; }
        public bool Wave { get; private set; }
        public bool Plasma { get; private set; }
        public bool Varia { get; private set; }
        public bool Gravity { get; private set; }
        public bool Morph { get; private set; }
        public bool Bombs { get; private set; }
        public bool SpringBall { get; private set; }
        public bool ScrewAttack { get; private set; }
        public bool HiJump { get; private set; }
        public bool SpaceJump { get; private set; }
        public bool SpeedBooster { get; private set; }
        public bool Missile { get; private set; }
        public bool Super { get; private set; }
        public bool PowerBomb { get; private set; }
        public bool TwoPowerBombs { get; private set; }
        public int ETank { get; private set; }
        public int ReserveTank { get; private set; }
        public int Rupees { get; private set; }

        public void Add(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                TrackItem(item.Type);
            }
        }

        public bool TrackItem(ItemType item)
        {
            return TrackSimpleItem(item) || TrackComplexItem(item);
        }

        private bool TrackComplexItem(ItemType item)
        {
            switch (item)
            {
                case ItemType.KeyCT: KeyCT += 1; break;
                case ItemType.KeyPD: KeyPD += 1; break;
                case ItemType.KeySW: KeySW += 1; break;
                case ItemType.KeyIP: KeyIP += 1; break;
                case ItemType.KeyMM: KeyMM += 1; break;
                case ItemType.KeyTR: KeyTR += 1; break;
                case ItemType.KeyGT: KeyGT += 1; break;
                case ItemType.ETank: ETank += 1; break;
                case ItemType.ReserveTank: ReserveTank += 1; break;
                case ItemType.ProgressiveShield: shield += 1; break;
                case ItemType.ProgressiveSword:
                    MasterSword = Sword;
                    Sword = true;
                    break;

                case ItemType.ProgressiveGlove:
                    Mitt = Glove;
                    Glove = true;
                    break;

                case ItemType.PowerBomb:
                    TwoPowerBombs = PowerBomb;
                    PowerBomb = true;
                    break;

                case ItemType.OneRupee: Rupees += 1; break;
                case ItemType.FiveRupees: Rupees += 5; break;
                case ItemType.TwentyRupees:
                case ItemType.TwentyRupees2: Rupees += 20; break;
                case ItemType.FiftyRupees: Rupees += 50; break;
                case ItemType.OneHundredRupees: Rupees += 100; break;
                case ItemType.ThreeHundredRupees: Rupees += 300; break;
                default: return false;
            }

            return true;
        }

        private bool TrackSimpleItem(ItemType item)
        {
            return item switch
            {
                ItemType.BigKeyEP => BigKeyEP = true,
                ItemType.BigKeyDP => BigKeyDP = true,
                ItemType.BigKeyTH => BigKeyTH = true,
                ItemType.BigKeyPD => BigKeyPD = true,
                ItemType.BigKeySP => BigKeySP = true,
                ItemType.BigKeySW => BigKeySW = true,
                ItemType.BigKeyTT => BigKeyTT = true,
                ItemType.BigKeyIP => BigKeyIP = true,
                ItemType.BigKeyMM => BigKeyMM = true,
                ItemType.BigKeyTR => BigKeyTR = true,
                ItemType.BigKeyGT => BigKeyGT = true,
                ItemType.KeyHC => KeyHC = true,
                ItemType.KeyDP => KeyDP = true,
                ItemType.KeyTH => KeyTH = true,
                ItemType.KeySP => KeySP = true,
                ItemType.KeyTT => KeyTT = true,
                ItemType.CardCrateriaL1 => CardCrateriaL1 = true,
                ItemType.CardCrateriaL2 => CardCrateriaL2 = true,
                ItemType.CardCrateriaBoss => CardCrateriaBoss = true,
                ItemType.CardBrinstarL1 => CardBrinstarL1 = true,
                ItemType.CardBrinstarL2 => CardBrinstarL2 = true,
                ItemType.CardBrinstarBoss => CardBrinstarBoss = true,
                ItemType.CardNorfairL1 => CardNorfairL1 = true,
                ItemType.CardNorfairL2 => CardNorfairL2 = true,
                ItemType.CardNorfairBoss => CardNorfairBoss = true,
                ItemType.CardMaridiaL1 => CardMaridiaL1 = true,
                ItemType.CardMaridiaL2 => CardMaridiaL2 = true,
                ItemType.CardMaridiaBoss => CardMaridiaBoss = true,
                ItemType.CardWreckedShipL1 => CardWreckedShipL1 = true,
                ItemType.CardWreckedShipBoss => CardWreckedShipBoss = true,
                ItemType.CardLowerNorfairL1 => CardLowerNorfairL1 = true,
                ItemType.CardLowerNorfairBoss => CardLowerNorfairBoss = true,
                ItemType.Bow => Bow = true,
                ItemType.Hookshot => Hookshot = true,
                ItemType.Mushroom => Mushroom = true,
                ItemType.Powder => Powder = true,
                ItemType.Firerod => Firerod = true,
                ItemType.Icerod => Icerod = true,
                ItemType.Bombos => Bombos = true,
                ItemType.Ether => Ether = true,
                ItemType.Quake => Quake = true,
                ItemType.Lamp => Lamp = true,
                ItemType.Hammer => Hammer = true,
                ItemType.Shovel => Shovel = true,
                ItemType.Flute => Flute = true,
                ItemType.Book => Book = true,
                ItemType.Bottle => Bottle = true,
                ItemType.Somaria => Somaria = true,
                ItemType.Byrna => Byrna = true,
                ItemType.Cape => Cape = true,
                ItemType.Mirror => Mirror = true,
                ItemType.Boots => Boots = true,
                ItemType.Flippers => Flippers = true,
                ItemType.MoonPearl => MoonPearl = true,
                ItemType.HalfMagic => HalfMagic = true,
                ItemType.Grapple => Grapple = true,
                ItemType.Charge => Charge = true,
                ItemType.Ice => Ice = true,
                ItemType.Wave => Wave = true,
                ItemType.Plasma => Plasma = true,
                ItemType.Varia => Varia = true,
                ItemType.Gravity => Gravity = true,
                ItemType.Morph => Morph = true,
                ItemType.Bombs => Bombs = true,
                ItemType.SpringBall => SpringBall = true,
                ItemType.ScrewAttack => ScrewAttack = true,
                ItemType.HiJump => HiJump = true,
                ItemType.SpaceJump => SpaceJump = true,
                ItemType.SpeedBooster => SpeedBooster = true,
                ItemType.Missile => Missile = true,
                ItemType.Super => Super = true,
                _ => false
            };
        }
    }
}
