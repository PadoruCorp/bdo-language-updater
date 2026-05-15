using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BDOLanguageUpdater.Service;

public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHostEnvironment _environment;
    private readonly IOptionsMonitor<T> _options;
    private readonly string _section;
    private readonly string _file;

    public WritableOptions(
        IHostEnvironment environment,
        IOptionsMonitor<T> options,
        string section,
        string file)
    {
        _environment = environment;
        _options = options;
        _section = section;
        _file = file;
    }

    public T Value => _options.CurrentValue;
    public T Get(string? name) => _options.Get(name);

    public void Update(Action<T> applyChanges)
    {
        var fileProvider = _environment.ContentRootFileProvider;
        var fileInfo = fileProvider.GetFileInfo(_file);
        var physicalPath = fileInfo.PhysicalPath;
        if (physicalPath == null) throw new InvalidOperationException();

        var jsonObject = JsonNode.Parse(File.ReadAllText(physicalPath)) as JsonObject;
        if (jsonObject == null) throw new InvalidOperationException();

        var sectionObject = jsonObject.TryGetPropertyValue(_section, out var section) && section is not null
            ? section.Deserialize<T>(SerializerOptions)
            : Value;
        if (sectionObject == null) throw new InvalidOperationException();

        applyChanges(sectionObject);

        jsonObject[_section] = JsonSerializer.SerializeToNode(sectionObject, SerializerOptions);
        File.WriteAllText(physicalPath, jsonObject.ToJsonString(SerializerOptions));
    }
}
