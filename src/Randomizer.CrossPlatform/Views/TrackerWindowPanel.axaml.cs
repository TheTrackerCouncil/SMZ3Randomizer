using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.Shared;

namespace Randomizer.CrossPlatform.Views;

public partial class TrackerWindowPanel : UserControl
{
    private TrackerWindowPanelViewModel? _model;

    public TrackerWindowPanel()
    {
        InitializeComponent();
    }

    public TrackerWindowPanel(TrackerWindowPanelViewModel model)
    {
        _model = model;
        InitializeComponent();
        Update();
        model.PropertyChanged += (_, _) =>
        {
            Update();
        };
    }

    private void Update()
    {
        if (!CheckAccess())
        {
            Dispatcher.UIThread.Invoke(Update);
            return;
        }

        MainDock.Children.Clear();

        if (_model == null)
        {
            return;
        }

        foreach (var image in _model.GetImages().NonNull())
        {
            MainDock.Children.Add(GetGridItemControl(image, true));
        }

        var menu = new ContextMenu();

        foreach (var menuItem in _model.GetMenuItems().NonNull())
        {
            menu.Items.Add(menuItem);
        }

        if (menu.Items.Any())
        {
            ContextMenu = menu;
        }
        else
        {
            ContextMenu = null;
        }
    }

    protected Image GetGridItemControl(TrackerWindowPanelImage model, bool displayDropShadows)
    {
        var image = new Image
        {
            Source = new Bitmap(model.ImagePath),
            MaxWidth = model.Width,
            MaxHeight = model.Height,
            Margin = new Thickness(model.OffsetX, model.OffsetY, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Opacity = model.IsActive ? 1 : 0.2
        };

        if (displayDropShadows)
        {
            image.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 3,
                Opacity = 0.7
            };
        }

        return image;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        if (!point.Properties.IsLeftButtonPressed)
        {
            return;
        }

        _model?.Click();
    }
}

