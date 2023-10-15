using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BDOLanguageUpdater.Tests;

public abstract class BaseHostTests: IAsyncLifetime
{
    protected IHost? Host { get; private set; }

    protected IServiceProvider? ServiceProvider { get; private set; }

    public virtual Task InitializeAsync()
    {
        var hostBuilder = new HostBuilder();
        this.ConfigureHost(hostBuilder);

        this.Host = hostBuilder.Build();
        this.ServiceProvider = this.Host.Services;

        return Task.CompletedTask;
    }
    
    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }
    
    protected virtual void ConfigureBoostrapServices(HostBuilderContext context, IServiceCollection bootstrapServices)
    {
    }
    
    protected virtual void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(this.ConfigureBoostrapServices);
        
        hostBuilder.ConfigureServices(this.ConfigureServices);

        hostBuilder.UseEnvironment(Environments.Development);
    }

    public virtual async Task DisposeAsync()
    {
        if (this.Host != null)
        {
            await this.Host.StopAsync();
        }
    }
}