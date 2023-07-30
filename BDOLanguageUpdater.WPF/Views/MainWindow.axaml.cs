using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using BDOLanguageUpdater.Service;
using BDOLanguageUpdater.WPF.ViewModels;
using ReactiveUI;

namespace BDOLanguageUpdater.WPF.Views;

public partial class MainWindow : Window
{
    private readonly LanguageFileWatcher watcher;
    private readonly MainWindowViewModel viewModel;
    private readonly IWritableOptions<UserPreferencesOptions> userPreferencesOptions;
    private readonly LanguageUpdaterService languageUpdaterService;
    private readonly StartupHelper startupHelper;

    public bool ExitingFromTray { get; set; } = false;

    public MainWindow(MainWindowViewModel viewModel, 
        LanguageFileWatcher watcher,
        IWritableOptions<UserPreferencesOptions> userPreferencesOptions,
        LanguageUpdaterService languageUpdaterService,
        StartupHelper startupHelper)
    {
        InitializeComponent();

        DataContext = viewModel;

        this.watcher = watcher;
        this.viewModel = viewModel;
        this.userPreferencesOptions = userPreferencesOptions;
        this.languageUpdaterService = languageUpdaterService;
        this.startupHelper = startupHelper;
        viewModel.GeneralTabViewModel.BDOPath = userPreferencesOptions.Value.BDOClientPath;
        viewModel.AdvancedTabViewModel.HideToTrayOnClose = userPreferencesOptions.Value.HideToTrayOnClose;
        viewModel.AdvancedTabViewModel.OpenOnStartup = userPreferencesOptions.Value.OpenOnStartup;
        
        viewModel.AdvancedTabViewModel.ObservableForProperty(vm => vm.HideToTrayOnClose).Subscribe(HideToTrayOnCloseChanged);
        viewModel.AdvancedTabViewModel.ObservableForProperty(vm => vm.OpenOnStartup).Subscribe(OpenOnStartupChanged);
    }

    private void HideToTrayOnCloseChanged(IObservedChange<AdvancedTabViewModel, bool> obj)
    {
        this.userPreferencesOptions.Update(options => { options.HideToTrayOnClose = obj.Value; });
    }

    private void OpenOnStartupChanged(IObservedChange<AdvancedTabViewModel, bool> obj)
    {
        this.userPreferencesOptions.Update(options => { options.OpenOnStartup = obj.Value; });

        startupHelper.SetStartupOnBoot(obj.Value);
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

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        
        if (ExitingFromTray)
        {
            return;
        }
        
        if (this.IsVisible && !viewModel.AdvancedTabViewModel.HideToTrayOnClose)
        {
            return;
        }

        e.Cancel = true;
        this.Hide();
    }
}