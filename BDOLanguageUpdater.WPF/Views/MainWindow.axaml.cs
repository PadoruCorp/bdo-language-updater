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
    private readonly MainWindowViewModel viewModel;
    private readonly IWritableOptions<UserPreferencesOptions> userPreferencesOptions;
    private readonly LanguageUpdaterService languageUpdaterService;
    private readonly AutoRepairScheduler autoRepairScheduler;
    private readonly GameLauncher gameLauncher;
    private readonly LanguageFileBackupStore backupStore;
    private bool suppressAutoRepairUpdates;

    public bool ExitingFromTray { get; set; } = false;

    public MainWindow(
        MainWindowViewModel viewModel,
        LanguageFileDiscovery languageFileDiscovery,
        IWritableOptions<UserPreferencesOptions> userPreferencesOptions,
        LanguageUpdaterService languageUpdaterService,
        AutoRepairScheduler autoRepairScheduler,
        GameLauncher gameLauncher,
        LanguageFileBackupStore backupStore)
    {
        InitializeComponent();

        DataContext = viewModel;

        this.languageFileDiscovery = languageFileDiscovery;
        this.viewModel = viewModel;
        this.userPreferencesOptions = userPreferencesOptions;
        this.languageUpdaterService = languageUpdaterService;
        this.autoRepairScheduler = autoRepairScheduler;
        this.gameLauncher = gameLauncher;
        this.backupStore = backupStore;

        viewModel.GeneralTabViewModel.BDOPath = userPreferencesOptions.Value.BDOClientPath;
        viewModel.AdvancedTabViewModel.HideToTrayOnClose = userPreferencesOptions.Value.HideToTrayOnClose;
        viewModel.AdvancedTabViewModel.IsAutoRepairSupported = autoRepairScheduler.IsSupported;
        viewModel.AdvancedTabViewModel.AutoRepairEnabled =
            autoRepairScheduler.IsSupported && userPreferencesOptions.Value.AutoRepairEnabled;
        viewModel.AdvancedTabViewModel.SelectMaintenanceDay(ParseAutoRepairDay(userPreferencesOptions.Value.AutoRepairDay));
        viewModel.AdvancedTabViewModel.AutoRepairStatus = autoRepairScheduler.IsSupported
            ? GetAutoRepairStatus()
            : "Automatic update scheduling is only available on Windows.";

        RefreshAvailableLanguages(persistSelectedLanguage: false);

        viewModel.GeneralTabViewModel.ObservableForProperty(vm => vm.SelectedLanguage).Subscribe(LanguageSelectionChanged);
        viewModel.AdvancedTabViewModel.ObservableForProperty(vm => vm.HideToTrayOnClose).Subscribe(HideToTrayOnCloseChanged);
        viewModel.AdvancedTabViewModel.ObservableForProperty(vm => vm.AutoRepairEnabled).Subscribe(AutoRepairEnabledChanged);
        viewModel.AdvancedTabViewModel.ObservableForProperty(vm => vm.SelectedMaintenanceDay).Subscribe(MaintenanceDayChanged);

        _ = EnsureAutoRepairTaskMatchesPreferences();
    }

    private void LanguageSelectionChanged(IObservedChange<GeneralTabViewModel, GameLanguageFile?> obj)
    {
        if (obj.Value is null)
        {
            return;
        }

        userPreferencesOptions.Update(options => { options.LanguageCodeToReplace = obj.Value.Code; });
        UpdateSelectedLanguageBackupAvailability();
        viewModel.GeneralTabViewModel.StatusMessage =
            $"{obj.Value.DisplayName} will be replaced with the latest official English localization.";
    }

    private void HideToTrayOnCloseChanged(IObservedChange<AdvancedTabViewModel, bool> obj)
    {
        userPreferencesOptions.Update(options => { options.HideToTrayOnClose = obj.Value; });
    }

    private async void AutoRepairEnabledChanged(IObservedChange<AdvancedTabViewModel, bool> obj)
    {
        if (suppressAutoRepairUpdates)
        {
            return;
        }

        var advanced = viewModel.AdvancedTabViewModel;
        if (!autoRepairScheduler.IsSupported)
        {
            advanced.AutoRepairStatus = "Automatic update scheduling is only available on Windows.";
            return;
        }

        if (obj.Value)
        {
            var day = advanced.SelectedMaintenanceDay?.Day ?? DayOfWeek.Thursday;
            var result = await autoRepairScheduler.CreateOrUpdate(day);
            if (result.Succeeded)
            {
                userPreferencesOptions.Update(options =>
                {
                    options.AutoRepairEnabled = true;
                    options.AutoRepairDay = day.ToString();
                });
                advanced.AutoRepairStatus = result.Message;
                return;
            }

            userPreferencesOptions.Update(options => { options.AutoRepairEnabled = false; });
            advanced.AutoRepairStatus = $"Could not enable automatic update: {result.Message}";

            suppressAutoRepairUpdates = true;
            advanced.AutoRepairEnabled = false;
            suppressAutoRepairUpdates = false;
            return;
        }

        var deleteResult = await autoRepairScheduler.Delete();
        userPreferencesOptions.Update(options => { options.AutoRepairEnabled = false; });
        advanced.AutoRepairStatus = deleteResult.Message;
    }

    private async void MaintenanceDayChanged(IObservedChange<AdvancedTabViewModel, MaintenanceDayOption?> obj)
    {
        if (suppressAutoRepairUpdates || obj.Value is null)
        {
            return;
        }

        userPreferencesOptions.Update(options => { options.AutoRepairDay = obj.Value.Day.ToString(); });

        var advanced = viewModel.AdvancedTabViewModel;
        if (!advanced.AutoRepairEnabled || !autoRepairScheduler.IsSupported)
        {
            return;
        }

        var result = await autoRepairScheduler.CreateOrUpdate(obj.Value.Day);
        advanced.AutoRepairStatus = result.Succeeded
            ? result.Message
            : $"Could not update automatic update schedule: {result.Message}";
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
        RefreshAvailableLanguages(persistSelectedLanguage: true);
    }

    private void ScanLanguages(object sender, RoutedEventArgs args)
    {
        RefreshAvailableLanguages(persistSelectedLanguage: true);
    }

    private async void UpdateLanguage(object sender, RoutedEventArgs args)
    {
        await UpdateSelectedLanguage(GameLaunchMode.None);
    }

    private async void RestoreBackup(object sender, RoutedEventArgs args)
    {
        var general = viewModel.GeneralTabViewModel;
        if (general.SelectedLanguage is null)
        {
            general.StatusMessage = "Choose one detected installed language before restoring a backup.";
            return;
        }

        var selectedLanguageCode = general.SelectedLanguage.Code;
        var selectedLanguageName = general.SelectedLanguage.DisplayName;

        general.IsUpdating = true;
        general.StatusMessage = $"Restoring the backed up {selectedLanguageName} file.";

        try
        {
            var result = await Task.Run(() => languageUpdaterService.RestoreLanguageBackup(selectedLanguageCode));
            general.StatusMessage = result.Message;
        }
        finally
        {
            UpdateSelectedLanguageBackupAvailability();
            general.IsUpdating = false;
        }
    }

    private async void UpdateAndLaunchSteam(object sender, RoutedEventArgs args)
    {
        await UpdateSelectedLanguage(GameLaunchMode.Steam);
    }

    private async void UpdateAndLaunchStandaloneLauncher(object sender, RoutedEventArgs args)
    {
        await UpdateSelectedLanguage(GameLaunchMode.StandaloneLauncher);
    }

    private async Task UpdateSelectedLanguage(GameLaunchMode launchMode)
    {
        var general = viewModel.GeneralTabViewModel;
        if (general.SelectedLanguage is null)
        {
            general.StatusMessage = "Choose one detected installed language before updating.";
            return;
        }

        var selectedLanguageCode = general.SelectedLanguage.Code;
        var selectedLanguageName = general.SelectedLanguage.DisplayName;
        var launchAfterUpdate = launchMode != GameLaunchMode.None;

        general.IsUpdating = true;
        general.StatusMessage = launchMode switch
        {
            GameLaunchMode.Steam => $"Replacing {selectedLanguageName} with English before launching through Steam.",
            GameLaunchMode.StandaloneLauncher => $"Replacing {selectedLanguageName} with English before launching through the BDO launcher.",
            _ => $"Downloading the latest official English localization and replacing {selectedLanguageName}.",
        };

        try
        {
            var result = await Task.Run(() => languageUpdaterService.UpdateLanguage(selectedLanguageCode));
            general.StatusMessage = result.Message;
            UpdateSelectedLanguageBackupAvailability();

            if (!result.Succeeded || !launchAfterUpdate)
            {
                return;
            }

            var launchResult = launchMode switch
            {
                GameLaunchMode.Steam => gameLauncher.LaunchSteam(),
                GameLaunchMode.StandaloneLauncher => gameLauncher.LaunchStandalone(userPreferencesOptions.Value.BDOClientPath),
                _ => GameLaunchResult.Failure("No launch option was selected."),
            };
            general.StatusMessage = launchResult.Succeeded
                ? $"{result.Message} {launchResult.Message}"
                : launchResult.Message;
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
        UpdateSelectedLanguageBackupAvailability();

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

        general.SetSelectedLanguageBackupAvailability(false);
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

    private async Task EnsureAutoRepairTaskMatchesPreferences()
    {
        var advanced = viewModel.AdvancedTabViewModel;
        if (!autoRepairScheduler.IsSupported || !advanced.AutoRepairEnabled)
        {
            return;
        }

        var day = advanced.SelectedMaintenanceDay?.Day ?? DayOfWeek.Thursday;
        var result = await autoRepairScheduler.CreateOrUpdate(day);
        advanced.AutoRepairStatus = result.Succeeded
            ? result.Message
            : $"Could not refresh automatic update schedule: {result.Message}";
    }

    private string GetAutoRepairStatus()
    {
        if (!userPreferencesOptions.Value.AutoRepairEnabled)
        {
            return "Automatic update is disabled.";
        }

        var day = ParseAutoRepairDay(userPreferencesOptions.Value.AutoRepairDay);
        return $"Enabled. Runs every {day} from 08:00 and retries every 2 hours that day.";
    }

    private static DayOfWeek ParseAutoRepairDay(string value)
    {
        return Enum.TryParse<DayOfWeek>(value, ignoreCase: true, out var day)
            ? day
            : DayOfWeek.Thursday;
    }

    private void UpdateSelectedLanguageBackupAvailability()
    {
        var selectedLanguage = viewModel.GeneralTabViewModel.SelectedLanguage;
        viewModel.GeneralTabViewModel.SetSelectedLanguageBackupAvailability(
            selectedLanguage is not null && backupStore.HasBackup(selectedLanguage));
    }

    private enum GameLaunchMode
    {
        None,
        Steam,
        StandaloneLauncher,
    }
}
