using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

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
        if (sender is not Button { Tag: SpriteViewModel { CanFavorite: true } sprite }) return;
        e.Handled = true;
        sprite.SetSpriteOption(sprite.SpriteOption == SpriteOptions.Favorite
            ? SpriteOptions.Default
            : SpriteOptions.Favorite);
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
        if (sender is not Button { Tag: SpriteViewModel { CanHide: true } sprite }) return;
        e.Handled = true;
        sprite.SetSpriteOption(sprite.SpriteOption == SpriteOptions.Hide
            ? SpriteOptions.Default
            : SpriteOptions.Hide);
        sprite.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
        sprite.Display = sprite.SpriteOption == SpriteOptions.Default;
    }

    private async void DeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { Tag: SpriteViewModel { CanDelete: true } sprite } || _spriteService == null) return;
            e.Handled = true;
            var response = await MessageWindow.ShowYesNoDialog("Are you sure you want to delete this sprite?", parentWindow: this);
            if (!response)
            {
                return;
            }

            _spriteService.DeleteSprite(sprite.Sprite);
            Model.Sprites.Remove(sprite);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }

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

        _allSprites = _spriteService.Sprites
            .Where(x => x.SpriteType == _spriteType)
            .OrderByDescending(x => x.IsRandomSprite)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Author)
            .ToList();

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

    private async void AddSpriteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_spriteService == null)
            {
                return;
            }

            var filter = _spriteType == SpriteType.Ship ? "IPS files (*.ips)|*.ips" : "RDC files (*.rdc)|*.rdc";
            var result = await CrossPlatformTools.OpenFileDialogAsync(this, FileInputControlType.OpenFile, filter, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            var spritePath = result?.TryGetLocalPath();

            if (string.IsNullOrEmpty(spritePath) || !File.Exists(spritePath))
            {
                return;
            }

            var previewImagePath = Path.ChangeExtension(spritePath, ".png");
            if (!File.Exists(previewImagePath))
            {
                if (await MessageWindow.ShowYesNoDialog("Is there a png preview image associated with this sprite?",
                    parentWindow: this))
                {
                    result = await CrossPlatformTools.OpenFileDialogAsync(this, FileInputControlType.OpenFile, "png files (*.png)|*.png", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                    previewImagePath = result?.TryGetLocalPath();

                    if (!File.Exists(previewImagePath))
                    {
                        previewImagePath = null;
                    }
                }
                else
                {
                    previewImagePath = null;
                }
            }

            var newSprite = _spriteService.AddCustomSprite(spritePath, previewImagePath);
            if (newSprite == null)
            {
                await MessageWindow.ShowErrorDialog("Unable to add custom sprite", parentWindow: this);
                return;
            }

            var oldModel = Model.Sprites.FirstOrDefault(x => x.Sprite == newSprite);
            if (oldModel != null)
            {
                Model.Sprites.Remove(oldModel);
            }

            var model = new SpriteViewModel(newSprite);
            model.CheckFilterOption(Model.SearchText, Model.SpriteFilter);
            var inserted = false;
            for (var i = 0; i < Model.Sprites.Count; i++)
            {
                if (Model.Sprites[i].Sprite.IsRandomSprite)
                {
                    continue;
                }

                if (string.Compare(Model.Sprites[i].Name, newSprite.Name, StringComparison.Ordinal) > 0)
                {
                    Model.Sprites.Insert(i, model);
                    inserted = true;
                    break;
                }
                else if (string.Compare(Model.Sprites[i].Name, newSprite.Name, StringComparison.Ordinal) == 0 &&
                         string.Compare(Model.Sprites[i].Author, newSprite.Author, StringComparison.Ordinal) > 0)
                {
                    Model.Sprites.Insert(i, model);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                Model.Sprites.Add(model);
            }
        }
        catch
        {
            await MessageWindow.ShowErrorDialog("Error attempting to select the sprite");
        }
    }
}

