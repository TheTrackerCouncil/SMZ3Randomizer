using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaGif;
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

        AddImages();

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

    protected void AddImages()
    {
        if (_model == null)
        {
            return;
        }

        var images = _model.GetMainImages();
        var overlayImages = _model.GetOverlayImages();

        if (overlayImages.Count == 0)
        {
            foreach (var modelModel in images)
            {
                if (modelModel.ImagePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    var image = new GifImage()
                    {
                        AutoStart = true,
                        Stretch = Stretch.UniformToFill,
                        SourceUri = new Uri(modelModel.ImagePath),
                        Width = modelModel.Width,
                        Height = modelModel.Height,
                        MinWidth = modelModel.Width,
                        MinHeight = modelModel.Height,
                        MaxWidth = modelModel.Width,
                        MaxHeight = modelModel.Height,
                        Margin = new Thickness(modelModel.OffsetX, modelModel.OffsetY, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Opacity = modelModel.IsActive ? 1 : 0.2
                    };

                    RenderOptions.SetBitmapInterpolationMode(image, BitmapInterpolationMode.None);

                    Height = modelModel.Height + 2;
                    Width = modelModel.Height + 2;

                    if (_model.AddShadows)
                    {
                        image.Effect = new DropShadowEffect
                        {
                            Color = Colors.Black,
                            BlurRadius = 3,
                            Opacity = 0.7
                        };
                    }

                    MainDock.Children.Add(image);
                }
                else
                {

                    var image = new Image
                    {
                        Source = new Bitmap(modelModel.ImagePath),
                        MaxWidth = modelModel.Width,
                        MaxHeight = modelModel.Height,
                        Margin = new Thickness(modelModel.OffsetX, modelModel.OffsetY, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Opacity = modelModel.IsActive? 1 : 0.2
                    };

                    if (_model.AddShadows)
                    {
                        image.Effect = new DropShadowEffect
                        {
                            Color = Colors.Black,
                            BlurRadius = 3,
                            Opacity = 0.7
                        };
                    }

                    MainDock.Children.Add(image);
                }
            }
        }
        else
        {
            var drawingGroup = new DrawingGroup();

            var anyActive = false;

            foreach (var overlay in images.Concat(overlayImages))
            {
                var overlayImage = new Bitmap(overlay.ImagePath);
                drawingGroup.Children.Add(new ImageDrawing()
                {
                    ImageSource = overlayImage,
                    Rect = new Rect(overlay.OffsetX, overlay.OffsetY, overlay.Width, overlay.Height)
                });
                anyActive |= overlay.IsActive;
            }

            var image = new Image
            {
                Source = new DrawingImage(drawingGroup),
                MaxWidth = 32,
                MaxHeight = 32,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Opacity = anyActive ? 1 : 0.2
            };

            if (_model.AddShadows)
            {
                image.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 3,
                    Opacity = 0.7
                };
            }

            MainDock.Children.Add(image);
        }

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

