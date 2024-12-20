using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.Data.ViewModels;

public class SpriteViewModel : INotifyPropertyChanged
{
    private bool _display;
    private static readonly IBrush s_defaultIconBrush = Brushes.Silver;
    private static readonly IBrush s_starredIconBrush = Brushes.Goldenrod;
    private static readonly IBrush s_hiddenIconBrush = Brushes.IndianRed;

    private static readonly Dictionary<SpriteType, (int, int)> s_imageDimensions = new()
    {
        { SpriteType.Link, (64, 96) }, { SpriteType.Samus, (64, 106) }, { SpriteType.Ship, (248, 92) },
    };

    private static readonly Dictionary<SpriteType, int> s_widths = new()
    {
        { SpriteType.Link, 235 }, { SpriteType.Samus, 235 }, { SpriteType.Ship, 450 },
    };

    public SpriteViewModel(Sprite sprite)
    {
        Sprite = sprite;
        Name = sprite.Name;
        Author = sprite.Author;
        PreviewPath = sprite.PreviewPath;
        Display = sprite.SpriteOption != SpriteOptions.Hide;
        PanelWidth = s_widths[sprite.SpriteType];
        ImageWidth = s_imageDimensions[sprite.SpriteType].Item1;
        ImageHeight = s_imageDimensions[sprite.SpriteType].Item2;
        CanFavorite = !sprite.IsRandomSprite;
        CanHide = sprite is { IsUserSprite: false };
        CanDelete = sprite is { IsRandomSprite: false, IsUserSprite: true };
        IconOpacity = CanFavorite ? 1f : 0.3f;

        SetSpriteOption(sprite.SpriteOption);
    }

    public Sprite Sprite { get; }
    public string Name { get; set; }
    public string Author { get; set; }
    public bool CanFavorite { get; set; }
    public int PanelWidth { get; }
    public int ImageWidth { get; }
    public int ImageHeight { get; }
    public float IconOpacity { get; }

    private string _previewPath;
    public string PreviewPath
    {
        get => _previewPath;
        set => SetField(ref _previewPath, value);
    }

    private bool _canHide;
    public bool CanHide
    {
        get => _canHide;
        set => SetField(ref _canHide, value);
    }

    private bool _canDelete;
    public bool CanDelete
    {
        get => _canDelete;
        set => SetField(ref _canDelete, value);
    }

    private IBrush? _starBrush = s_defaultIconBrush;
    public IBrush? StarBrush
    {
        get => _starBrush;
        set => SetField(ref _starBrush, value);
    }

    private IBrush? _hideBrush = s_defaultIconBrush;
    public IBrush? HideBrush
    {
        get => _hideBrush;
        set => SetField(ref _hideBrush, value);
    }

    private IBrush? _deleteBrush = s_defaultIconBrush;
    public IBrush? DeleteBrush
    {
        get => _deleteBrush;
        set => SetField(ref _deleteBrush, value);
    }

    public SpriteOptions SpriteOption
    {
        get => Sprite.SpriteOption;
        set
        {
            Sprite.SpriteOption = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(IsHidden));
        }
    }

    public string AuthorText => string.IsNullOrEmpty(Author) ? "" : $"by {Author}";

    public bool Display
    {
        get => _display;
        set => SetField(ref _display, value);
    }

    public bool IsFavorite => SpriteOption == SpriteOptions.Favorite;

    public bool IsHidden => SpriteOption == SpriteOptions.Hide;

    public void SetSpriteOption(SpriteOptions option)
    {
        SpriteOption = option;

        if (Sprite.IsRandomSprite)
        {
            StarBrush = s_defaultIconBrush;
            HideBrush = s_defaultIconBrush;
            return;
        }
        else if (option == SpriteOptions.Default)
        {
            StarBrush = s_defaultIconBrush;
            HideBrush = s_defaultIconBrush;
            DeleteBrush = s_defaultIconBrush;
        }
        else if (option == SpriteOptions.Favorite)
        {
            StarBrush = s_starredIconBrush;
            HideBrush = s_defaultIconBrush;
            DeleteBrush = s_defaultIconBrush;
        }
        else if (option == SpriteOptions.Hide)
        {
            StarBrush = s_defaultIconBrush;
            HideBrush = s_hiddenIconBrush;
            DeleteBrush = s_defaultIconBrush;
        }
    }

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
