using Microsoft.Extensions.DependencyInjection;

namespace BDLanguageUpdater.Service;

public static class IServiceCollectionExtensions
{
    public static void RegisterUpdaterServices(this IServiceCollection services)
    {
        services.AddHttpClient(Constants.HTTP_CLIENT_NAME);
    }
}