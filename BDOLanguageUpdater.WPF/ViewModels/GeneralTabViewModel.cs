using ReactiveUI;

namespace BDOLanguageUpdater.WPF.ViewModels;

public class GeneralTabViewModel : ReactiveObject
{
    public string BDOPath { get; set; } = null!;
}