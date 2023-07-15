using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using BDOLanguageUpdater.Service;
using BDOLanguageUpdater.WPF.ViewModels;

namespace BDOLanguageUpdater.WPF.Views;

public partial class MainWindow : Window
{
    private readonly LanguageFileWatcher watcher;
    private readonly MainWindowViewModel viewModel;
    private readonly IWritableOptions<UserPreferencesOptions> userPreferencesOptions;
    private readonly LanguageUpdaterService languageUpdaterService;

    public MainWindow(MainWindowViewModel viewModel, LanguageFileWatcher watcher,
        IWritableOptions<UserPreferencesOptions> userPreferencesOptions,
        LanguageUpdaterService languageUpdaterService)
    {
        InitializeComponent();

        DataContext = viewModel;

        this.watcher = watcher;
        this.viewModel = viewModel;
        this.userPreferencesOptions = userPreferencesOptions;
        this.languageUpdaterService = languageUpdaterService;
        viewModel.GeneralTabViewModel.BDOPath = userPreferencesOptions.Value.BDOClientPath;
    }

    private async void Browse(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = GetTopLevel(this);

        if (topLevel is null) throw new InvalidOperationException();

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select Folder",
            AllowMultiple = false
        });

        if (files.Count < 1) return;
        
        // Open reading stream from the first file.
        var storageFolder = files[0];

        var path = storageFolder.Path.LocalPath;

        watcher.SetPath(path);
        userPreferencesOptions.Update(options => { options.BDOClientPath = path; });
        viewModel.GeneralTabViewModel.BDOPath = path;
    }

    private async void UpdateLanguage(object sender, RoutedEventArgs args)
    {
        UpdateLanguageButton.IsEnabled = false;

        await languageUpdaterService.UpdateLanguage();

        UpdateLanguageButton.IsEnabled = true;
    }
}