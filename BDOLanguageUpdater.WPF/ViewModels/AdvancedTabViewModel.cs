using ReactiveUI;

namespace BDOLanguageUpdater.WPF.ViewModels;

public class AdvancedTabViewModel : ReactiveObject
{
    private bool _hideToTrayOnClose;
    
    public bool HideToTrayOnClose
    {
        get => _hideToTrayOnClose;
        set
        {
            _hideToTrayOnClose = value;
            this.RaisePropertyChanged();
        }
    }
}