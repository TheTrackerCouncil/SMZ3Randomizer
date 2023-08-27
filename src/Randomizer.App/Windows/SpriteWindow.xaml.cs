using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Randomizer.App.ViewModels;
using Randomizer.Data.Options;
using Randomizer.Shared;

namespace Randomizer.App.Windows;

[NotAService]
public partial class SpriteWindow : Window
{
    private readonly SpriteService _spriteService;

    public SpriteWindow(SpriteService spriteService)
    {
        _spriteService = spriteService;
        InitializeComponent();

        DataContext = Model = new SpriteWindowViewModel();

    }

    public SpriteWindowViewModel Model { get; } = new();

    public RandomizerOptions Options { get; set; } = new();

    public SpriteType SpriteType { get; set; }

    private IEnumerable<Sprite>? _allSprites = new List<Sprite>();

    private Dictionary<string, SpriteOptions> _spriteOptions = new();

    public Sprite? SelectedSprite { get; private set; }
    public Dictionary<string, SpriteOptions> SelectedSpriteOptions { get; private set; } = new();

    private string _searchText = "";
    private SpriteFilter _spriteFilter;

    private void SpriteWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _spriteOptions = SpriteType switch
        {
            SpriteType.Link => Options.GeneralOptions.LinkSpriteOptions,
            SpriteType.Samus => Options.GeneralOptions.SamusSpriteOptions,
            SpriteType.Ship => Options.GeneralOptions.ShipSpriteOptions,
            _ => new Dictionary<string, SpriteOptions>()
        };

        _allSprites = _spriteService.Sprites.Where(x => x.SpriteType == SpriteType).OrderBy(x => !_spriteOptions.ContainsKey(x.FilePath));

        Model.Sprites.Add(new SpriteViewModel(_spriteService.GetRandomPreviewImage(SpriteType), "Random Sprite", SpriteType));

        Task.Factory.StartNew(() =>
        {
            foreach (var sprite in _allSprites)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var model = new SpriteViewModel(sprite, GetSpriteOptionFor(sprite));
                    model.CheckFilterOption(_searchText, _spriteFilter);
                    Model.Sprites.Add(model);
                }), DispatcherPriority.Background);
            }
        });
    }

    private SpriteOptions GetSpriteOptionFor(Sprite sprite)
    {
        if (_spriteOptions.TryGetValue(sprite.FilePath, out var option))
        {
            return option;
        }
        else
        {
            return SpriteOptions.Default;
        }
    }

    private void UIElement_OnTextInput(object sender, TextCompositionEventArgs e)
    {
        var text = SearchTextBox.Text;
    }

    private void SearchTextBox_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (SearchTextBox.Text == _searchText) return;
        _searchText = SearchTextBox.Text;
        Search();
    }

    private void Search()
    {
        foreach (var sprite in Model.Sprites)
        {
            sprite.CheckFilterOption(_searchText, _spriteFilter);
        }
    }

    private void HideButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SpriteViewModel sprite }) return;
        sprite.SpriteOption = SpriteOptions.Hide;
        sprite.CheckFilterOption(_searchText, _spriteFilter);
    }

    private void SpriteFilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _spriteFilter = (SpriteFilter)SpriteFilterComboBox.SelectedItem;
        Search();
    }

    private void ToggleFavoriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SpriteViewModel sprite }) return;
        sprite.SpriteOption = sprite.SpriteOption == SpriteOptions.Favorite
            ? SpriteOptions.Default
            : SpriteOptions.Favorite;
        sprite.CheckFilterOption(_searchText, _spriteFilter);
    }

    private void SelectButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SpriteViewModel sprite }) return;

        if (sprite.Sprite == null)
        {
            if (!Model.Sprites.Any(x => x.Display && x.Sprite != null))
            {
                MessageBox.Show(
                    "No available sprites to pick from. Change your search and filter options before selecting the random sprite option.",
                    "SMZ3 Cas' Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var random = new Random().Sanitize();
            sprite = Model.Sprites.Where(x => x.Display && x.Sprite != null).Random(random)!;
        }

        SelectedSprite = sprite.Sprite;
        SelectedSpriteOptions = Model.Sprites.Where(x => x.SpriteOption != SpriteOptions.Default)
            .ToDictionary(x => x.Sprite?.FilePath ?? "", x => x.SpriteOption);
        DialogResult = true;
        Close();
    }
}

