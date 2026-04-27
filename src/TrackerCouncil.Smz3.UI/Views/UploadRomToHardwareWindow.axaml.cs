using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaControls.Extensions;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class UploadRomToHardwareWindow : Window
{
    private UploadRomToHardwareWindowViewModel _model;
    private UploadRomToHardwareWindowService? _service;

    public UploadRomToHardwareWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = _model = new UploadRomToHardwareWindowViewModel();
        }
        else
        {
            _service = IControlServiceFactory.GetControlService<UploadRomToHardwareWindowService>();
            DataContext = _model = _service!.GetViewModel();
        }
    }

    public bool DidUploadSuccessfully { get; set; }

    public Task<bool?> ShowDialog(Window parent, string rom, string target)
    {
        _service?.StartUpload(rom, target);
        return ShowDialog<bool?>(parent);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        DidUploadSuccessfully = !_model.IsLoading;
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

