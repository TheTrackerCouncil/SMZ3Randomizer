using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Randomizer.Shared;

namespace Randomizer.SMZ3
{
    /// <summary>
    /// Represents a player's inventory across both games.
    /// </summary>
    public class Progression : ICollection<ItemType>
    {
        public Progression(LogicConfig logicConfig)
        {
            Items = new();
            LogicConfig = logicConfig;
        }

        public Progression(IEnumerable<ItemType> items, LogicConfig logicConfig)
        {
            Items = new(items);
            LogicConfig = logicConfig;
        }

        public Progression(IEnumerable<Item> items, LogicConfig logicConfig)
        {
            Items = new(items.Select(x => x.Type));
            LogicConfig = logicConfig;
        }

        
        public bool BigKeyEP => Contains(ItemType.BigKeyEP);
        public bool BigKeyDP => Contains(ItemType.BigKeyDP);
        public bool BigKeyTH => Contains(ItemType.BigKeyTH);
        public bool BigKeyPD => Contains(ItemType.BigKeyPD);
        public bool BigKeySP => Contains(ItemType.BigKeySP);
        public bool BigKeySW => Contains(ItemType.BigKeySW);
        public bool BigKeyTT => Contains(ItemType.BigKeyTT);
        public bool BigKeyIP => Contains(ItemType.BigKeyIP);
        public bool BigKeyMM => Contains(ItemType.BigKeyMM);
        public bool BigKeyTR => Contains(ItemType.BigKeyTR);
        public bool BigKeyGT => Contains(ItemType.BigKeyGT);
        public bool KeyHC => Contains(ItemType.KeyHC);
        public bool KeyDP => Contains(ItemType.KeyDP);
        public bool KeyTH => Contains(ItemType.KeyTH);
        public bool KeySP => Contains(ItemType.KeySP);
        public bool KeyTT => Contains(ItemType.KeyTT);
        public int KeyCT => GetCount(ItemType.KeyCT);
        public int KeyPD => GetCount(ItemType.KeyPD);
        public int KeySW => GetCount(ItemType.KeySW);
        public int KeyIP => GetCount(ItemType.KeyIP);
        public int KeyMM => GetCount(ItemType.KeyMM);
        public int KeyTR => GetCount(ItemType.KeyTR);
        public int KeyGT => GetCount(ItemType.KeyGT);
        public bool CardCrateriaL1 => Contains(ItemType.CardCrateriaL1);
        public bool CardCrateriaL2 => Contains(ItemType.CardCrateriaL2);
        public bool CardCrateriaBoss => Contains(ItemType.CardCrateriaBoss);
        public bool CardBrinstarL1 => Contains(ItemType.CardBrinstarL1);
        public bool CardBrinstarL2 => Contains(ItemType.CardBrinstarL2);
        public bool CardBrinstarBoss => Contains(ItemType.CardBrinstarBoss);
        public bool CardNorfairL1 => Contains(ItemType.CardNorfairL1);
        public bool CardNorfairL2 => Contains(ItemType.CardNorfairL2);
        public bool CardNorfairBoss => Contains(ItemType.CardNorfairBoss);
        public bool CardMaridiaL1 => Contains(ItemType.CardMaridiaL1);
        public bool CardMaridiaL2 => Contains(ItemType.CardMaridiaL2);
        public bool CardMaridiaBoss => Contains(ItemType.CardMaridiaBoss);
        public bool CardWreckedShipL1 => Contains(ItemType.CardWreckedShipL1);
        public bool CardWreckedShipBoss => Contains(ItemType.CardWreckedShipBoss);
        public bool CardLowerNorfairL1 => Contains(ItemType.CardLowerNorfairL1);
        public bool CardLowerNorfairBoss => Contains(ItemType.CardLowerNorfairBoss);
        public bool CanBlockLasers => Contains(ItemType.CardWreckedShipBoss, 3);
        public bool Sword => Contains(ItemType.ProgressiveSword);
        public bool MasterSword => Contains(ItemType.ProgressiveSword, 2);
        public bool Bow => Contains(ItemType.Bow);
        public bool Hookshot => Contains(ItemType.Hookshot);
        public bool Mushroom => Contains(ItemType.Mushroom);
        public bool Powder => Contains(ItemType.Powder);
        public bool FireRod => Contains(ItemType.Firerod);
        public bool IceRod => Contains(ItemType.Icerod);
        public bool Bombos => Contains(ItemType.Bombos);
        public bool Ether => Contains(ItemType.Ether);
        public bool Quake => Contains(ItemType.Quake);
        public bool Lamp => Contains(ItemType.Lamp);
        public bool Hammer => Contains(ItemType.Hammer);
        public bool Shovel => Contains(ItemType.Shovel);
        public bool Flute => Contains(ItemType.Flute);
        public bool Book => Contains(ItemType.Book);
        public bool Bottle => Contains(ItemType.Bottle);
        public bool Somaria => Contains(ItemType.Somaria);
        public bool Byrna => Contains(ItemType.Byrna);
        public bool Cape => Contains(ItemType.Cape);
        public bool Mirror => Contains(ItemType.Mirror);
        public bool Boots => Contains(ItemType.Boots);
        public bool Glove => Contains(ItemType.ProgressiveGlove);
        public bool Mitt => Contains(ItemType.ProgressiveGlove, 2);
        public bool Flippers => Contains(ItemType.Flippers);
        public bool MoonPearl => Contains(ItemType.MoonPearl);
        public bool HalfMagic => Contains(ItemType.HalfMagic);
        public bool Grapple => Contains(ItemType.Grapple);
        public bool Charge => Contains(ItemType.Charge);
        public bool Ice => Contains(ItemType.Ice);
        public bool Wave => Contains(ItemType.Wave);
        public bool Plasma => Contains(ItemType.Plasma);
        public bool Varia => Contains(ItemType.Varia);
        public bool Gravity => Contains(ItemType.Gravity);
        public bool Morph => Contains(ItemType.Morph);
        public bool Bombs => Contains(ItemType.Bombs);
        public bool SpringBall => Contains(ItemType.SpringBall);
        public bool ScrewAttack => Contains(ItemType.ScrewAttack);
        public bool HiJump => Contains(ItemType.HiJump);
        public bool SpaceJump => Contains(ItemType.SpaceJump);
        public bool SpeedBooster => Contains(ItemType.SpeedBooster);
        public bool Missile => Contains(ItemType.Missile);
        public bool Super => Contains(ItemType.Super);
        public bool PowerBomb => Contains(ItemType.PowerBomb);
        public bool TwoPowerBombs => Contains(ItemType.PowerBomb, 2);
        public int ETank => GetCount(ItemType.ETank);
        public int ReserveTank => GetCount(ItemType.ETank);
        public int Rupees => GetRupeeCount();
        public int Count => Items.Count;
        public bool SafeScrewAttack => ScrewAttack && (Morph || !LogicConfig.PreventScrewAttackSoftLock);
        public bool IsReadOnly => false;

        protected LogicConfig LogicConfig { get; }
        protected List<ItemType> Items { get; }

        public bool Contains(ItemType itemType)
            => Items.Contains(itemType);

        public bool Contains(ItemType itemType, int amount)
            => GetCount(itemType) >= amount;

        public int GetCount(ItemType itemType)
            => Items.Count(x => x == itemType);

        public void Add(ItemType item)
            => Items.Add(item);

        public void AddRange(IEnumerable<ItemType> items)
            => Items.AddRange(items);

        public void AddRange(IEnumerable<Item> items)
            => Items.AddRange(items.Select(x => x.Type));

        public void Clear()
            => Items.Clear();

        public Progression Clone()
            => new(Items, LogicConfig);

        public void CopyTo(ItemType[] array, int arrayIndex)
            => Items.CopyTo(array, arrayIndex);

        public bool Remove(ItemType item)
            => Items.Remove(item);

        public IEnumerator<ItemType> GetEnumerator()
            => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private int GetRupeeCount()
        {
            return Items.Sum(x => x switch
            {
                ItemType.OneRupee => 1,
                ItemType.FiveRupees => 5,
                ItemType.TwentyRupees or ItemType.TwentyRupees2 => 20,
                ItemType.FiftyRupees => 50,
                ItemType.OneHundredRupees => 100,
                ItemType.ThreeHundredRupees => 300,
                _ => 0,
            });
        }
    }
}
