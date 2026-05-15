using System.Collections.ObjectModel;
using BDOLanguageUpdater.Service;
using ReactiveUI;

namespace BDOLanguageUpdater.WPF.ViewModels;

public class GeneralTabViewModel : ReactiveObject
{
    private string _bdoPath = string.Empty;
    private GameLanguageFile? _selectedLanguage;
    private string _statusMessage = "Select your Black Desert Online folder, scan installed languages, then replace one with English.";
    private bool _isUpdating;
    private bool _hasSelectedLanguageBackup;

    public string BDOPath
    {
        get => _bdoPath;
        set
        {
            _bdoPath = value;
            this.RaisePropertyChanged();
        }
    }

    public ObservableCollection<GameLanguageFile> AvailableLanguages { get; } = new();

    public GameLanguageFile? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            _selectedLanguage = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(CanUpdate));
            this.RaisePropertyChanged(nameof(CanRestoreBackup));
            this.RaisePropertyChanged(nameof(SelectedLanguageDescription));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            this.RaisePropertyChanged();
        }
    }

    public bool IsUpdating
    {
        get => _isUpdating;
        set
        {
            _isUpdating = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(CanUpdate));
            this.RaisePropertyChanged(nameof(CanRestoreBackup));
        }
    }

    public bool HasAvailableLanguages => AvailableLanguages.Count > 0;

    public bool CanUpdate => HasAvailableLanguages && SelectedLanguage is not null && !IsUpdating;

    public bool HasSelectedLanguageBackup
    {
        get => _hasSelectedLanguageBackup;
        private set
        {
            _hasSelectedLanguageBackup = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(CanRestoreBackup));
        }
    }

    public bool CanRestoreBackup => SelectedLanguage is not null && HasSelectedLanguageBackup && !IsUpdating;

    public string TargetLanguageDescription => "English (en)";

    public string SelectedLanguageDescription => SelectedLanguage?.DisplayName ?? "No installed language selected";

    public void SetAvailableLanguages(IReadOnlyList<GameLanguageFile> languages, string preferredLanguageCode)
    {
        AvailableLanguages.Clear();
        foreach (var language in languages)
        {
            AvailableLanguages.Add(language);
        }

        SelectedLanguage = AvailableLanguages.FirstOrDefault(language =>
            language.Code.Equals(preferredLanguageCode, StringComparison.OrdinalIgnoreCase));

        SelectedLanguage ??= AvailableLanguages.FirstOrDefault();

        this.RaisePropertyChanged(nameof(HasAvailableLanguages));
        this.RaisePropertyChanged(nameof(CanUpdate));
    }

    public void SetSelectedLanguageBackupAvailability(bool hasBackup)
    {
        HasSelectedLanguageBackup = hasBackup;
    }
}
