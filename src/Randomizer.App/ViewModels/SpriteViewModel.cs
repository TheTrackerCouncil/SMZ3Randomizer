using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Randomizer.Data.Options;

namespace Randomizer.App.ViewModels;

public class SpriteViewModel : INotifyPropertyChanged
{
    private bool _display;

    private static Dictionary<SpriteType, (int, int)> s_ImageDimensions = new()
    {
        { SpriteType.Link, (64, 96) }, { SpriteType.Samus, (64, 106) }, { SpriteType.Ship, (248, 92) },
    };

    private static Dictionary<SpriteType, int> s_Widths = new()
    {
        { SpriteType.Link, 250 }, { SpriteType.Samus, 250 }, { SpriteType.Ship, 450 },
    };

    public SpriteViewModel(Sprite sprite)
    {
        Sprite = sprite;
        Name = sprite.Name;
        Author = sprite.Author;
        PreviewPath = sprite.PreviewPath;
        SpriteOption = sprite.SpriteOption;
        Display = sprite.SpriteOption != SpriteOptions.Hide;
        PanelWidth = s_Widths[sprite.SpriteType];
        ImageWidth = s_ImageDimensions[sprite.SpriteType].Item1;
        ImageHeight = s_ImageDimensions[sprite.SpriteType].Item2;
        CanFavoriteAndHide = true;
    }

    public Sprite Sprite { get; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string PreviewPath { get; set; }
    public bool CanFavoriteAndHide { get; set; }
    public int PanelWidth { get; }
    public int ImageWidth { get; }
    public int ImageHeight { get; }

    public SpriteOptions SpriteOption
    {
        get => Sprite.SpriteOption;
        set
        {
            Sprite.SpriteOption = value;
            OnPropertyChanged();
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
        if (Sprite.IsRandomSprite)
        {
            Display = true;
            return;
        }

        Display = Sprite.MatchesFilter(searchTerm, spriteFilter);
    }
}
