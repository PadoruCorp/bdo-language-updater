namespace BDOLanguageUpdater.WPF.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public GeneralTabViewModel GeneralTabViewModel { get; } = new();
    public AdvancedTabViewModel AdvancedTabViewModel { get; } = new();
}