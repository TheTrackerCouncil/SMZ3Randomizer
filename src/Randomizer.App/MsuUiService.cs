using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using MSURandomizerUI;
using Randomizer.Data.Options;

namespace Randomizer.App;

public class MsuUiService
{
    private readonly IMsuLookupService _msuLookupService;
    private readonly IMsuUiFactory _msuUiFactory;
    private readonly RandomizerOptions _options;

    public MsuUiService(OptionsFactory optionsFactory, IMsuLookupService msuLookupService, IMsuUiFactory msuUiFactory)
    {
        _options = optionsFactory.Create();
        _msuLookupService = msuLookupService;
        _msuUiFactory = msuUiFactory;
    }

    public bool OpenMsuWindow(Window parentWindow, SelectionMode selectionMode, MsuRandomizationStyle? randomizationStyle)
    {
        if (!VerifyMsuDirectory(parentWindow)) return false;
        if (!_msuUiFactory.OpenMsuWindow(selectionMode, true, out var options)) return false;
        if (options.SelectedMsus?.Any() != true) return false;
        _options.PatchOptions.MsuPaths = options.SelectedMsus.ToList();
        _options.PatchOptions.MsuRandomizationStyle = randomizationStyle;
        return true;
    }

    public bool LookupMsus()
    {
        if (!string.IsNullOrEmpty(_options.GeneralOptions.MsuPath) && Directory.Exists(_options.GeneralOptions.MsuPath) || _msuLookupService.Status != MsuLoadStatus.Loading)
        {
            Task.Run(() =>
            {
                _msuLookupService.LookupMsus(_options.GeneralOptions.MsuPath);
            });

            return true;
        }

        return false;
    }

    public bool VerifyMsuDirectory(Window parentWindow)
    {
        if (!string.IsNullOrEmpty(_options.GeneralOptions.MsuPath) && Directory.Exists(_options.GeneralOptions.MsuPath))
        {
            return true;
        }

        MessageBox.Show(parentWindow, "Please select the parent folder than contains all of your MSUs. To preserve drive space, it is recommended that the Rom Output and MSU folders be on the same drive.", "MSU Path Needed",
            MessageBoxButton.OK, MessageBoxImage.Exclamation);

        using var dialog = new CommonOpenFileDialog();
        dialog.EnsurePathExists = true;
        dialog.Title = "Select MSU Path";
        dialog.IsFolderPicker = true;

        if (dialog.ShowDialog(parentWindow) == CommonFileDialogResult.Ok)
        {
            _options.GeneralOptions.MsuPath = dialog.FileName;
        }

        if (string.IsNullOrEmpty(_options.GeneralOptions.MsuPath) ||
            !Directory.Exists(_options.GeneralOptions.MsuPath))
        {
            return false;
        }

        Task.Run(() =>
        {
            _msuLookupService.LookupMsus(_options.GeneralOptions.MsuPath);
        });

        MessageBox.Show(parentWindow, "Updated MSU folder. If you want to change the MSU path in the future, you can do so in the Tools -> Options window", "MSU Path Updated",
            MessageBoxButton.OK, MessageBoxImage.Information);

        LookupMsus();

        return true;
    }

}
