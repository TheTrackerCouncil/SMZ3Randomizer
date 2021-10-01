using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

using Randomizer.SMZ3;

namespace Randomizer.App.ViewModels
{
    public class RandomizerOptions : INotifyPropertyChanged
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public RandomizerOptions()
        {
            General = new GeneralOptions();
            Seed = new SeedOptions();
        }

        [JsonConstructor]
        public RandomizerOptions(GeneralOptions general, SeedOptions seed)
        {
            General = general;
            Seed = seed;
        }

        public RandomizerOptions(Window owner)
            : this()
        {
            General.SetOwner(owner);
        }

        public GeneralOptions General { get; }

        public SeedOptions Seed { get; }

        public static RandomizerOptions Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<RandomizerOptions>(json, s_jsonOptions);
        }

        public RandomizerOptions WithOwner(Window owner)
        {
            General.SetOwner(owner);
            return this;
        }

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this, s_jsonOptions);
            File.WriteAllText(path, json);
        }

        public Config ToConfig() => new()
        {
            GameMode = GameMode.Normal,
            Z3Logic = Z3Logic.Normal,
            SMLogic = SMLogic.Normal,
            SwordLocation = Seed.SwordLocation,
            MorphLocation = Seed.MorphLocation,
            MorphBombsLocation = Seed.MorphBombsLocation,
            KeyShuffle = Seed.Keysanity ? KeyShuffle.Keysanity : KeyShuffle.None,
            Race = Seed.Race
        };

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }
    }
}
