using System.Net.Http;
using BDOLanguageUpdater.Service;
using BDOLanguageUpdater.Service.Serializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Padoru.Core.Files;

public static class ServiceCollectionExtensions
{
    public static void RegisterFileManager(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IFileSystem, MemoryFileSystem>();
        serviceCollection.AddTransient<ISerializer, StringSerializer>();
        serviceCollection.AddSingleton<IFileManager>(provider =>
        {
            var fileManager = new FileManager(provider.GetRequiredService<ISerializer>(),
                provider.GetRequiredService<IFileSystem>());

            var locSerializer = new LocSerializer();
            var passthroughSerializer = new PassthroughSerializer();

            var httpsFileSystem = new HttpsFileSystem(provider.GetRequiredService<IHttpClientFactory>(),
                provider.GetRequiredService<ILogger<HttpsFileSystem>>());
            var localFileSystem = new LocalFileSystem(string.Empty);
            
            fileManager.RegisterProtocol(Constants.LOCAL_LOCALIZATION_PROTOCOL_HEADER, locSerializer,
                localFileSystem);
            
            fileManager.RegisterProtocol(Constants.HTTPS_LOCALIZATION_PROTOCOL_HEADER, locSerializer,
                httpsFileSystem);
            
            fileManager.RegisterProtocol(Constants.LOCAL_NOT_SERIALIZED_PROTOCOL_HEADER, passthroughSerializer,
                localFileSystem);

            fileManager.RegisterProtocol(Constants.HTTPS_STRING_PROTOCOL_HEADER, new StringSerializer(),
                httpsFileSystem);

            fileManager.RegisterProtocol(Constants.HTTPS_NOT_SERIALIZED_PROTOCOL_HEADER, passthroughSerializer,
                httpsFileSystem);

            fileManager.RegisterProtocol(Constants.LOCAL_NOT_SERIALIZED_TEST_PROTOCOL_HEADER, passthroughSerializer,
                new MemoryFileSystem());

            return fileManager;
        });
    }
}