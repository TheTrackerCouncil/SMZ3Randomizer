using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

using Randomizer.SMZ3;
using Randomizer.SMZ3.Msu;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// Represents user-configurable options for patching and customizing the
    /// randomized ROM.
    /// </summary>
    public class PatchOptions : INotifyPropertyChanged
    {
        private string _musicPackPath;
        private MusicPack _musicPack;

        public event PropertyChangedEventHandler PropertyChanged;

        public Sprite LinkSprite { get; set; }
            = Sprite.DefaultLink;

        public Sprite SamusSprite { get; set; }
            = Sprite.DefaultSamus;

        public ShipSprite ShipPatch { get; set; }
            = ShipSprite.DefaultShip;

        [JsonIgnore]
        public MusicPack MusicPack
        {
            get => _musicPack;
            set
            {
                if (value != _musicPack)
                {
                    _musicPack = value;
                    _musicPackPath = value.FileName;
                    OnPropertyChanged(nameof(MusicPack));
                    OnPropertyChanged(nameof(MusicPackPath));
                    OnPropertyChanged(nameof(CanEnableExtendedSoundtrack));
                }
            }
        }
        public string MusicPackPath
        {
            get => _musicPackPath;
            set
            {
                if (value != _musicPackPath)
                {
                    _musicPackPath = value;
                    OnPropertyChanged(nameof(MusicPackPath));
                    OnPropertyChanged(nameof(CanEnableExtendedSoundtrack));
                }
            }
        }

        public bool EnableExtendedSoundtrack { get; set; }

        public MusicShuffleMode ShuffleDungeonMusic { get; set; }
            = MusicShuffleMode.Default;

        public bool CanEnableExtendedSoundtrack => File.Exists(MusicPack?.FileName);

        public HeartColor HeartColor { get; set; }
            = HeartColor.Red;

        public LowHealthBeepSpeed LowHealthBeepSpeed { get; set; }
            = LowHealthBeepSpeed.Half;

        public MenuSpeed MenuSpeed { get; set; }
            = MenuSpeed.Default;

        public bool DisableLowEnergyBeep { get; set; }

        public bool CasualSuperMetroidPatches { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
