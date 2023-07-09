using ReactiveUI;

namespace BDOLanguageUpdater.WPF.ViewModels;

public class AdvancedTabViewModel : ReactiveObject
{
    public string FileChangedCheckInterval { get; set; }
}