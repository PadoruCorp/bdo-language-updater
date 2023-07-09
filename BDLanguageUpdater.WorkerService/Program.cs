using BDLanguageUpdater.WorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>();

        services.ConfigureWritable<UserPreferencesOptions>(ctx.Configuration.GetSection(UserPreferencesOptions.UserPreferences));
        services.ConfigureWritable<UrlMetadataOptions>(ctx.Configuration.GetSection(UrlMetadataOptions.UrlMetadata));
    })
    .Build();

host.Run();
