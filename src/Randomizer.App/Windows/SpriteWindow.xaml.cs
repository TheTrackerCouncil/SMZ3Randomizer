using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Randomizer.App.ViewModels;
using Randomizer.Data.Options;
using Randomizer.Sprites;

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

    private void SpriteWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _spriteOptions = SpriteType switch
        {
            SpriteType.Link => Options.GeneralOptions.LinkSpriteOptions,
            SpriteType.Samus => Options.GeneralOptions.SamusSpriteOptions,
            SpriteType.Ship => Options.GeneralOptions.ShipSpriteOptions,
            _ => new Dictionary<string, SpriteOptions>()
        };

        _allSprites = _spriteService.Sprites.Where(x => x.SpriteType == SpriteType).OrderByDescending(x => x.IsRandomSprite).ThenByDescending(x => _spriteOptions.ContainsKey(x.FilePath));

        Task.Factory.StartNew(() =>
        {
            foreach (var sprite in _allSprites)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var model = new SpriteViewModel(sprite);
                    model.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
                    Model.Sprites.Add(model);
                }), DispatcherPriority.Background);
            }
        });
    }

    private void SearchTextBox_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (SearchTextBox.Text == Model.SearchText) return;
        Model.SearchText = SearchTextBox.Text;
        Search();
    }

    private void Search()
    {
        foreach (var sprite in Model.Sprites)
        {
            sprite.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
        }
    }

    private void HideButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SpriteViewModel sprite }) return;
        sprite.SpriteOption = SpriteOptions.Hide;
        sprite.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
    }

    private void SpriteFilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Model.SpriteFilter = (SpriteFilter)SpriteFilterComboBox.SelectedItem;
        Search();
    }

    private void ToggleFavoriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SpriteViewModel sprite }) return;
        sprite.SpriteOption = sprite.SpriteOption == SpriteOptions.Favorite
            ? SpriteOptions.Default
            : SpriteOptions.Favorite;
        sprite.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
    }

    private void SelectButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SpriteViewModel sprite }) return;

        if (sprite.Sprite.IsRandomSprite && !Model.Sprites.Any(x => x.Display && !x.Sprite.IsRandomSprite))
        {
            MessageBox.Show(
                "No available sprites to pick from. Change your search and filter options before selecting the random sprite option.",
                "SMZ3 Cas' Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SelectedSprite = sprite.Sprite;
        SelectedSpriteOptions = Model.Sprites.Where(x => x.SpriteOption != SpriteOptions.Default)
            .ToDictionary(x => x.Sprite?.FilePath ?? "", x => x.SpriteOption);
        DialogResult = true;
        Close();
    }

    private void SpriteWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        SelectedSpriteOptions = _allSprites!.Where(x => x.SpriteOption != SpriteOptions.Default)
            .ToDictionary(x => x.FilePath ?? "", x => x.SpriteOption);
    }
}

