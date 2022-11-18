using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Randomizer.Data.Options
{
    /// <summary>
    /// Represents user-configurable options for patching and customizing the
    /// randomized ROM.
    /// </summary>
    public class PatchOptions : INotifyPropertyChanged
    {
        private string _msu1Path = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public Sprite LinkSprite { get; set; }
            = Sprite.DefaultLink;

        public Sprite SamusSprite { get; set; }
            = Sprite.DefaultSamus;

        public ShipSprite ShipPatch { get; set; }
            = ShipSprite.DefaultShip;

        public string Msu1Path
        {
            get => _msu1Path;
            set
            {
                if (value != _msu1Path)
                {
                    _msu1Path = value;
                    OnPropertyChanged(nameof(Msu1Path));
                    OnPropertyChanged(nameof(CanEnableExtendedSoundtrack));
                }
            }
        }

        public bool EnableExtendedSoundtrack { get; set; }

        public MusicShuffleMode ShuffleDungeonMusic { get; set; }
            = MusicShuffleMode.Default;

        public bool CanEnableExtendedSoundtrack => File.Exists(Msu1Path);

        public HeartColor HeartColor { get; set; }
            = HeartColor.Red;

        public LowHealthBeepSpeed LowHealthBeepSpeed { get; set; }
            = LowHealthBeepSpeed.Half;

        public MenuSpeed MenuSpeed { get; set; }
            = MenuSpeed.Default;

        public bool DisableLowEnergyBeep { get; set; }

        public bool CasualSuperMetroidPatches { get; set; }

        public CasPatches CasPatches { get; set; } = new();

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
