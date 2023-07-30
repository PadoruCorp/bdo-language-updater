using ReactiveUI;

namespace BDOLanguageUpdater.WPF.ViewModels;

public class AdvancedTabViewModel : ReactiveObject
{
    private bool _hideToTrayOnClose;
    private bool _openOnStartup;
    
    public bool HideToTrayOnClose
    {
        get => _hideToTrayOnClose;
        set
        {
            _hideToTrayOnClose = value;
            this.RaisePropertyChanged();
        }
    }

    public bool OpenOnStartup
    {
        get => _openOnStartup;
        set
        {
            _openOnStartup = value;
            this.RaisePropertyChanged();
        }
    }
}