using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using BDOLanguageUpdater.Service;
using BDOLanguageUpdater.WPF.ViewModels;
using ReactiveUI;

namespace BDOLanguageUpdater.WPF.Views;

public partial class MainWindow : Window
{
    private readonly LanguageFileDiscovery languageFileDiscovery;
    private readonly LanguageFileWatcher watcher;
    private readonly MainWindowViewModel viewModel;
    private readonly IWritableOptions<UserPreferencesOptions> userPreferencesOptions;
    private readonly LanguageUpdaterService languageUpdaterService;

    public bool ExitingFromTray { get; set; } = false;

    public MainWindow(
        MainWindowViewModel viewModel,
        LanguageFileDiscovery languageFileDiscovery,
        LanguageFileWatcher watcher,
        IWritableOptions<UserPreferencesOptions> userPreferencesOptions,
        LanguageUpdaterService languageUpdaterService)
    {
        InitializeComponent();

        DataContext = viewModel;

        this.languageFileDiscovery = languageFileDiscovery;
        this.watcher = watcher;
        this.viewModel = viewModel;
        this.userPreferencesOptions = userPreferencesOptions;
        this.languageUpdaterService = languageUpdaterService;

        viewModel.GeneralTabViewModel.BDOPath = userPreferencesOptions.Value.BDOClientPath;
        viewModel.AdvancedTabViewModel.HideToTrayOnClose = userPreferencesOptions.Value.HideToTrayOnClose;

        RefreshAvailableLanguages(persistSelectedLanguage: false);

        viewModel.GeneralTabViewModel.ObservableForProperty(vm => vm.SelectedLanguage).Subscribe(LanguageSelectionChanged);
        viewModel.AdvancedTabViewModel.ObservableForProperty(vm => vm.HideToTrayOnClose).Subscribe(HideToTrayOnCloseChanged);
    }

    private void LanguageSelectionChanged(IObservedChange<GeneralTabViewModel, GameLanguageFile?> obj)
    {
        if (obj.Value is null)
        {
            return;
        }

        userPreferencesOptions.Update(options => { options.LanguageCodeToReplace = obj.Value.Code; });
        viewModel.GeneralTabViewModel.StatusMessage =
            $"{obj.Value.DisplayName} will be replaced with the latest official English localization.";
    }

    private void HideToTrayOnCloseChanged(IObservedChange<AdvancedTabViewModel, bool> obj)
    {
        userPreferencesOptions.Update(options => { options.HideToTrayOnClose = obj.Value; });
    }

    private async void Browse(object sender, RoutedEventArgs args)
    {
        var topLevel = GetTopLevel(this);

        if (topLevel is null) throw new InvalidOperationException();

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Black Desert Online Folder",
            AllowMultiple = false
        });

        if (folders.Count < 1) return;

        var path = folders[0].Path.LocalPath;

        userPreferencesOptions.Update(options => { options.BDOClientPath = path; });
        viewModel.GeneralTabViewModel.BDOPath = path;
        watcher.SetPath(languageFileDiscovery.GetAdsPath(path));
        RefreshAvailableLanguages(persistSelectedLanguage: true);
    }

    private void ScanLanguages(object sender, RoutedEventArgs args)
    {
        RefreshAvailableLanguages(persistSelectedLanguage: true);
    }

    private async void UpdateLanguage(object sender, RoutedEventArgs args)
    {
        var general = viewModel.GeneralTabViewModel;
        if (general.SelectedLanguage is null)
        {
            general.StatusMessage = "Choose one detected installed language before updating.";
            return;
        }

        var selectedLanguageCode = general.SelectedLanguage.Code;
        var selectedLanguageName = general.SelectedLanguage.DisplayName;

        general.IsUpdating = true;
        general.StatusMessage =
            $"Downloading the latest official English localization and replacing {selectedLanguageName}.";

        try
        {
            var result = await Task.Run(() => languageUpdaterService.UpdateLanguage(selectedLanguageCode));
            general.StatusMessage = result.Message;
        }
        finally
        {
            general.IsUpdating = false;
        }
    }

    private void RefreshAvailableLanguages(bool persistSelectedLanguage)
    {
        var general = viewModel.GeneralTabViewModel;
        var preferredLanguageCode = general.SelectedLanguage?.Code ?? userPreferencesOptions.Value.LanguageCodeToReplace;
        var languages = languageFileDiscovery.GetAvailableLanguages(general.BDOPath);

        general.SetAvailableLanguages(languages, preferredLanguageCode);

        if (general.SelectedLanguage is not null)
        {
            if (persistSelectedLanguage)
            {
                userPreferencesOptions.Update(options => { options.LanguageCodeToReplace = general.SelectedLanguage.Code; });
            }

            var languageList = string.Join(", ", languages.Select(language => language.DisplayName));
            general.StatusMessage =
                $"Detected {languageList}. The selected language will be replaced with English.";
            return;
        }

        var adsPath = languageFileDiscovery.GetAdsPath(general.BDOPath);
        general.StatusMessage = Directory.Exists(adsPath)
            ? $"No replaceable language files were found in '{adsPath}'. Expected languagedata_*.loc files other than English."
            : $"Could not find the ads folder at '{adsPath}'. Select the Black Desert Online folder and scan again.";
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
