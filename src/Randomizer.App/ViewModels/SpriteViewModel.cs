using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Randomizer.Data.Options;

namespace Randomizer.App.ViewModels;

public class SpriteViewModel : INotifyPropertyChanged
{
    private bool _display;
    private SpriteOptions _spriteOption;

    private static Dictionary<SpriteType, (int, int)> s_ImageDimensions = new()
    {
        { SpriteType.Link, (64, 96) }, { SpriteType.Samus, (64, 106) }, { SpriteType.Ship, (248, 92) },
    };

    private static Dictionary<SpriteType, int> s_Widths = new()
    {
        { SpriteType.Link, 250 }, { SpriteType.Samus, 250 }, { SpriteType.Ship, 450 },
    };

    public SpriteViewModel(Sprite sprite, SpriteOptions spriteOption)
    {
        Sprite = sprite;
        Name = sprite.Name;
        Author = sprite.Author;
        PreviewPath = sprite.PreviewPath;
        SpriteOption = spriteOption;
        Display = spriteOption != SpriteOptions.Hide;
        PanelWidth = s_Widths[sprite.SpriteType];
        ImageWidth = s_ImageDimensions[sprite.SpriteType].Item1;
        ImageHeight = s_ImageDimensions[sprite.SpriteType].Item2;
        CanFavoriteAndHide = true;
    }

    public SpriteViewModel(string path, string name, SpriteType spriteType)
    {
        Sprite = null;
        Name = name;
        Author = "";
        PreviewPath = path;
        Display = true;
        PanelWidth = s_Widths[spriteType];
        ImageWidth = s_ImageDimensions[spriteType].Item1;
        ImageHeight = s_ImageDimensions[spriteType].Item2;
    }

    public Sprite? Sprite { get; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string PreviewPath { get; set; }
    public bool CanFavoriteAndHide { get; set; }
    public int PanelWidth { get; }
    public int ImageWidth { get; }
    public int ImageHeight { get; }

    public SpriteOptions SpriteOption
    {
        get => _spriteOption;
        set
        {
            SetField(ref _spriteOption, value);
            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(IsNotFavorite));
        }
    }

    public string AuthorText => string.IsNullOrEmpty(Author) ? "" : $"by {Author}";

    public bool Display
    {
        get => _display;
        set => SetField(ref _display, value);
    }

    public bool IsFavorite => SpriteOption == SpriteOptions.Favorite;

    public bool IsNotFavorite => !IsFavorite;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void CheckFilterOption(string searchTerm, SpriteFilter spriteFilter)
    {
        if (Sprite == null)
        {
            Display = true;
            return;
        }

        Display = (string.IsNullOrEmpty(searchTerm) || Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
                  ((spriteFilter == SpriteFilter.Default && SpriteOption != SpriteOptions.Hide) || (spriteFilter == SpriteFilter.Favorited && SpriteOption == SpriteOptions.Favorite) || (spriteFilter == SpriteFilter.Hidden && SpriteOption == SpriteOptions.Hide) || spriteFilter == SpriteFilter.All);
    }
}
