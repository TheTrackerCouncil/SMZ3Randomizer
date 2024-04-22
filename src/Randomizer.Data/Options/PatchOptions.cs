using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using MSURandomizerLibrary;
using YamlDotNet.Serialization;

namespace Randomizer.Data.Options
{
    /// <summary>
    /// Represents user-configurable options for patching and customizing the
    /// randomized ROM.
    /// </summary>
    public class PatchOptions : INotifyPropertyChanged
    {
        private string _msu1Path = "";
        private string _msuName = "";
        private MsuRandomizationStyle? _msuRandomizationStyle;
        private List<string> _msuPaths = new List<string>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public Sprite SelectedLinkSprite { get; set; }
            = Sprite.DefaultLink;

        public Sprite SelectedSamusSprite { get; set; }
            = Sprite.DefaultSamus;

        public Sprite SelectedShipSprite { get; set; }
            = Sprite.DefaultShip;

        public Sprite? PreviousLinkSprite { get; set; }

        public Sprite? PreviousSamusSprite { get; set; }

        public Sprite? PreviousShipSprite { get; set; }

        [YamlIgnore]
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

        [YamlIgnore]
        public string MsuName
        {
            get => _msuName;
            set
            {
                if (value != _msuName)
                {
                    _msuName = value;
                    OnPropertyChanged(nameof(MsuName));
                }
            }
        }

        public List<string> MsuPaths
        {
            get => _msuPaths;
            set
            {
                if (value != _msuPaths)
                {
                    _msuPaths = value;
                    OnPropertyChanged(nameof(MsuPaths));
                }
            }
        }

        public MsuRandomizationStyle? MsuRandomizationStyle
        {
            get => _msuRandomizationStyle;
            set
            {
                if (value != _msuRandomizationStyle)
                {
                    _msuRandomizationStyle = value;
                    OnPropertyChanged(nameof(MsuRandomizationStyle));
                    MsuShuffleStyleEnabled = value == MSURandomizerLibrary.MsuRandomizationStyle.Shuffled ||
                                             value == MSURandomizerLibrary.MsuRandomizationStyle.Continuous;
                }
            }
        }

        private bool _msuShuffleStyleEnabled;

        [YamlIgnore]
        public bool MsuShuffleStyleEnabled
        {
            get => _msuShuffleStyleEnabled;
            set
            {
                _msuShuffleStyleEnabled = value;
                OnPropertyChanged();
            }
        }

        public MsuShuffleStyle MsuShuffleStyle { get; set; }

        public bool EnableExtendedSoundtrack { get; set; }

        public MusicShuffleMode ShuffleDungeonMusic { get; set; }
            = MusicShuffleMode.Default;

        [YamlIgnore]
        public bool CanEnableExtendedSoundtrack => File.Exists(Msu1Path);

        public HeartColor HeartColor { get; set; }
            = HeartColor.Red;

        public LowHealthBeepSpeed LowHealthBeepSpeed { get; set; }
            = LowHealthBeepSpeed.Half;

        public MenuSpeed MenuSpeed { get; set; }
            = MenuSpeed.Default;

        public ZeldaDrops? ZeldaDrops { get; set; }

        public bool DisableLowEnergyBeep { get; set; }

        public bool CasualSuperMetroidPatches { get; set; }

        public CasPatches CasPatches { get; set; } = new();

        public MetroidControlOptions MetroidControls { get; set; } = new();

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
