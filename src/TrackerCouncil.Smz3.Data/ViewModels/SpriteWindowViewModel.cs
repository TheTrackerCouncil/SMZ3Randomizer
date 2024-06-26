using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.Data.ViewModels;

public class SpriteWindowViewModel : INotifyPropertyChanged
{
    private ObservableCollection<SpriteViewModel> _sprites = new();
    private SpriteFilter _spriteFilter;
    private string _searchText = "";

    public ObservableCollection<SpriteViewModel> Sprites
    {
        get => _sprites;
        set => SetField(ref _sprites, value);
    }

    public SpriteFilter SpriteFilter
    {
        get => _spriteFilter;
        set => SetField(ref _spriteFilter, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
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
}
