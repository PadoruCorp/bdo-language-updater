using System.Threading;
using System.Threading.Tasks;
using BDLanguageUpdater.Service;
using Microsoft.Extensions.Hosting;
#if ON_W10
using Microsoft.Toolkit.Uwp.Notifications;
#endif

namespace BDOLanguageUpdater.WPF;

/// <summary>
/// This notifications manager needs to be a <see cref="BackgroundService"/>
/// in order to be automatically instantiated and registered to the
/// <see cref="LanguageFileUpdater"/> events.
/// </summary>
public class NotificationsManager : BackgroundService
{
    public NotificationsManager(LanguageFileUpdater languageFileUpdater)
    {
        languageFileUpdater.OnUpdateFinish += LanguageFileUpdaterOnOnUpdateFinish;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private void LanguageFileUpdaterOnOnUpdateFinish()
    {
#if ON_W10
        new ToastContentBuilder()
            .AddText("BDO Language Updater")
            .AddText("Client language updated!")
            .Show();
#endif
    }
}