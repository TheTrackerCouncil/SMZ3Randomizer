using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.ViewModels;

namespace Randomizer.CrossPlatform.Views;

public partial class SpriteWindow : ScalableWindow
{

    private SpriteType _spriteType;
    private readonly RandomizerOptions _options;
    private IEnumerable<Sprite>? _allSprites = new List<Sprite>();
    private readonly SpriteService? _spriteService;

    private Dictionary<string, SpriteOptions> _spriteOptions = new();

    public Sprite? SelectedSprite { get; private set; }
    public Dictionary<string, SpriteOptions> SelectedSpriteOptions { get; private set; } = new();

    public bool DialogResult;


    public SpriteWindow()
    {
        _options = new RandomizerOptions();
        InitializeComponent();
        DataContext = Model;
    }

    public SpriteWindow(SpriteService spriteService, OptionsFactory optionsFactory)
    {
        _spriteService = spriteService;
        _options = optionsFactory.Create();
        InitializeComponent();
        DataContext = Model;
    }

    public SpriteWindowViewModel Model { get; } = new();

    public void SetSpriteType(SpriteType type)
    {
        _spriteType = type;

        if (type == SpriteType.Link)
        {
            SelectedSprite = _options.PatchOptions.SelectedLinkSprite;
            Model.SearchText = _options.GeneralOptions.LinkSpriteSearchText;
            Model.SpriteFilter = _options.GeneralOptions.LinkSpriteFilter;
        }
        else if (type == SpriteType.Samus)
        {
            SelectedSprite = _options.PatchOptions.SelectedSamusSprite;
            Model.SearchText = _options.GeneralOptions.SamusSpriteSearchText;
            Model.SpriteFilter = _options.GeneralOptions.SamusSpriteFilter;
        }
        else
        {
            SelectedSprite = _options.PatchOptions.SelectedShipSprite;
            Model.SearchText = _options.GeneralOptions.ShipSpriteSearchText;
            Model.SpriteFilter = _options.GeneralOptions.ShipSpriteFilter;
        }
    }

    private void SearchTextBox_OnKeyUp(object? sender, KeyEventArgs e)
    {
        Search();
    }

    private void ToggleFavoriteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SpriteViewModel sprite }) return;
        sprite.SpriteOption = sprite.SpriteOption == SpriteOptions.Favorite
            ? SpriteOptions.Default
            : SpriteOptions.Favorite;
        sprite.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
    }

    private void SelectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SpriteViewModel sprite }) return;

        if (sprite.Sprite.IsRandomSprite && !Model.Sprites.Any(x => x is { Display: true, Sprite.IsRandomSprite: false }))
        {
            var window = new MessageWindow(new MessageWindowRequest()
            {
                Message =
                    "No available sprites to pick from. Change your search and filter options before selecting the random sprite option.",
                Title = "SMZ3 Cas' Randomizer",
                Buttons = MessageWindowButtons.OK,
                Icon = MessageWindowIcon.Error
            });
            window.ShowDialog(this);
            return;
        }

        SelectedSprite = sprite.Sprite;
        SelectedSpriteOptions = Model.Sprites.Where(x => x.SpriteOption != SpriteOptions.Default)
            .ToDictionary(x => x.Sprite?.FilePath ?? "", x => x.SpriteOption);
        DialogResult = true;
        Close();
    }

    private void HideButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SpriteViewModel sprite }) return;
        sprite.SpriteOption = SpriteOptions.Hide;
        sprite.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
        sprite.Display = false;
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_spriteService == null)
        {
            return;
        }

        _spriteOptions = _spriteType switch
        {
            SpriteType.Link => _options.GeneralOptions.LinkSpriteOptions,
            SpriteType.Samus => _options.GeneralOptions.SamusSpriteOptions,
            SpriteType.Ship => _options.GeneralOptions.ShipSpriteOptions,
            _ => new Dictionary<string, SpriteOptions>()
        };

        _allSprites = _spriteService.Sprites.Where(x => x.SpriteType == _spriteType).OrderByDescending(x => x.IsRandomSprite).ThenByDescending(x => _spriteOptions.ContainsKey(x.FilePath));

        Task.Factory.StartNew(() =>
        {
            foreach (var sprite in _allSprites)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    var model = new SpriteViewModel(sprite);
                    model.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
                    Model.Sprites.Add(model);
                }, DispatcherPriority.Background);
            }
        });
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        SelectedSpriteOptions = _allSprites!.Where(x => x.SpriteOption != SpriteOptions.Default)
            .ToDictionary(x => x.FilePath ?? "", x => x.SpriteOption);
    }

    private void Search()
    {
        foreach (var sprite in Model.Sprites)
        {
            sprite.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
        }
    }

    private void EnumComboBox_OnValueChanged(object sender, EnumValueChangedEventArgs args)
    {
        Model.SpriteFilter =
            (SpriteFilter?)this.Find<EnumComboBox>(nameof(FilterComboBox))!.Value ?? SpriteFilter.All;
        Search();
    }
}

