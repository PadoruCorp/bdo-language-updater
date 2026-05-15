using System.Text;
using System.Text.Json.Nodes;
using BDOLanguageUpdater.Service;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BDOLanguageUpdater.Tests;

public sealed class SystemTextJsonSerializerTests
{
    [Fact]
    public async Task JsonSerializer_RoundTripsObject()
    {
        var serializer = new Padoru.Core.Files.JsonSerializer();
        var value = new JsonSerializerSample("Black Desert", 10, true);

        var bytes = await serializer.Serialize(value);
        var result = await serializer.Deserialize(typeof(JsonSerializerSample), bytes, "memory://sample.json");

        var sample = Assert.IsType<JsonSerializerSample>(result);
        Assert.Equal(value.Name, sample.Name);
        Assert.Equal(value.Version, sample.Version);
        Assert.Equal(value.Enabled, sample.Enabled);
    }

    [Fact]
    public void WritableOptions_Update_UpdatesSectionAndPreservesOtherSections()
    {
        var tempDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        try
        {
            var settingsPath = Path.Combine(tempDirectory.FullName, "appsettings.json");
            File.WriteAllText(
                settingsPath,
                """
                {
                  "UserPreferences": {
                    "BDOClientPath": "C:\\BDO",
                    "LanguageCodeToReplace": "es",
                    "HideToTrayOnClose": true,
                    "OpenOnStartup": false
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  }
                }
                """,
                Encoding.UTF8);

            var writableOptions = new WritableOptions<UserPreferencesOptions>(
                new TestHostEnvironment(tempDirectory.FullName),
                new TestOptionsMonitor<UserPreferencesOptions>(new UserPreferencesOptions()),
                UserPreferencesOptions.UserPreferences,
                "appsettings.json");

            writableOptions.Update(options =>
            {
                options.LanguageCodeToReplace = "pt";
                options.OpenOnStartup = true;
            });

            var json = JsonNode.Parse(File.ReadAllText(settingsPath))!.AsObject();
            var userPreferences = json["UserPreferences"]!.AsObject();
            var logging = json["Logging"]!.AsObject();

            Assert.Equal("C:\\BDO", userPreferences["BDOClientPath"]!.GetValue<string>());
            Assert.Equal("pt", userPreferences["LanguageCodeToReplace"]!.GetValue<string>());
            Assert.True(userPreferences["HideToTrayOnClose"]!.GetValue<bool>());
            Assert.True(userPreferences["OpenOnStartup"]!.GetValue<bool>());
            Assert.Equal("Information", logging["LogLevel"]!["Default"]!.GetValue<string>());
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    private sealed record JsonSerializerSample(string Name, int Version, bool Enabled);

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public TestHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
        }

        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = nameof(SystemTextJsonSerializerTests);
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        public TestOptionsMonitor(T value)
        {
            CurrentValue = value;
        }

        public T CurrentValue { get; }

        public T Get(string? name)
        {
            return CurrentValue;
        }

        public IDisposable? OnChange(Action<T, string?> listener)
        {
            return null;
        }
    }
}
