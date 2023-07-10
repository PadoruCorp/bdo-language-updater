using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BDLanguageUpdater.Service;

public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
{
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
        
        var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(physicalPath));
        if (jObject == null) throw new InvalidOperationException();
        
        var sectionObject = jObject.TryGetValue(_section, out var section) ?
            JsonConvert.DeserializeObject<T>(section.ToString()) : Value;
        if (sectionObject == null) throw new InvalidOperationException();
        
        applyChanges(sectionObject);

        jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
        File.WriteAllText(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
    }
}
