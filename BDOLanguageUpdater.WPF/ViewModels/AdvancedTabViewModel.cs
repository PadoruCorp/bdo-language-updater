using System.Collections.ObjectModel;
using System.Globalization;
using ReactiveUI;

namespace BDOLanguageUpdater.WPF.ViewModels;

public class AdvancedTabViewModel : ReactiveObject
{
    private bool _autoRepairEnabled;
    private string _autoRepairStatus = "Automatic update is disabled.";
    private bool _hideToTrayOnClose;
    private bool _isAutoRepairSupported = true;
    private MaintenanceDayOption? _selectedMaintenanceDay;

    public AdvancedTabViewModel()
    {
        foreach (var day in GetMaintenanceDays())
        {
            MaintenanceDays.Add(day);
        }

        SelectedMaintenanceDay = MaintenanceDays.FirstOrDefault(day => day.Day == DayOfWeek.Thursday)
                                 ?? MaintenanceDays.FirstOrDefault();
    }

    public bool HideToTrayOnClose
    {
        get => _hideToTrayOnClose;
        set
        {
            _hideToTrayOnClose = value;
            this.RaisePropertyChanged();
        }
    }

    public bool AutoRepairEnabled
    {
        get => _autoRepairEnabled;
        set
        {
            _autoRepairEnabled = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(CanEditAutoRepairSchedule));
        }
    }

    public ObservableCollection<MaintenanceDayOption> MaintenanceDays { get; } = new();

    public MaintenanceDayOption? SelectedMaintenanceDay
    {
        get => _selectedMaintenanceDay;
        set
        {
            _selectedMaintenanceDay = value;
            this.RaisePropertyChanged();
        }
    }

    public bool IsAutoRepairSupported
    {
        get => _isAutoRepairSupported;
        set
        {
            _isAutoRepairSupported = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(CanEditAutoRepairSchedule));
        }
    }

    public bool CanEditAutoRepairSchedule => IsAutoRepairSupported && AutoRepairEnabled;

    public string AutoRepairStatus
    {
        get => _autoRepairStatus;
        set
        {
            _autoRepairStatus = value;
            this.RaisePropertyChanged();
        }
    }

    public void SelectMaintenanceDay(DayOfWeek day)
    {
        SelectedMaintenanceDay = MaintenanceDays.FirstOrDefault(option => option.Day == day)
                                 ?? MaintenanceDays.FirstOrDefault();
    }

    private static IEnumerable<MaintenanceDayOption> GetMaintenanceDays()
    {
        var culture = CultureInfo.CurrentCulture;
        var days = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday,
        };

        foreach (var day in days)
        {
            yield return new MaintenanceDayOption(day, culture.DateTimeFormat.GetDayName(day));
        }
    }
}

public sealed record MaintenanceDayOption(DayOfWeek Day, string DisplayName)
{
    public override string ToString()
    {
        return DisplayName;
    }
}
