using BDOLanguageUpdater.Service;
using Microsoft.Extensions.Hosting;

namespace BDOLanguageUpdater.WPF;

internal static class ApplicationHost
{
    public static IHost Create(string[]? args = null)
    {
        return Host.CreateDefaultBuilder(args ?? [])
            .UseContentRoot(AppContext.BaseDirectory)
            .UseSerilog()
            .ConfigureServices((context, services) => { context.RegisterServices(services); })
            .Build();
    }
}
