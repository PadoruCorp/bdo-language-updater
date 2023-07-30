using ReactiveUI;

namespace BDOLanguageUpdater.WPF.ViewModels;

public class GeneralTabViewModel : ReactiveObject
{
    private string _bdoPath = string.Empty;
    
    public string BDOPath
    {
        get => _bdoPath;
        set
        {
            _bdoPath = value;
            this.RaisePropertyChanged();
        }
    }
}