using System.Diagnostics;
using BDOLanguageUpdater.Service.Serializer;
using Xunit.Abstractions;

namespace BDOLanguageUpdater.Tests;

public sealed class LocalizationPerformanceTests
{
    private readonly ITestOutputHelper output;

    public LocalizationPerformanceTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    [Trait("Category", "Performance")]
    public async Task EnglishIntoSpanishLocalizationPipeline_WithRealFiles_Completes()
    {
        var serializer = new LocSerializer();
        var englishPath = TestFilePaths.GetTestDataPath("languagedata_en.loc");
        var spanishPath = TestFilePaths.GetTestDataPath("languagedata_es.loc");

        var englishBytes = await MeasureAsync("Read English .loc", () => File.ReadAllBytesAsync(englishPath));
        var spanishBytes = await MeasureAsync("Read Spanish .loc", () => File.ReadAllBytesAsync(spanishPath));

        var englishTsv = await MeasureAsync(
            "Deserialize English .loc",
            async () => (string)await serializer.Deserialize(typeof(string), englishBytes, englishPath));

        var spanishTsv = await MeasureAsync(
            "Deserialize Spanish .loc",
            async () => (string)await serializer.Deserialize(typeof(string), spanishBytes, spanishPath));

        var mergedTsv = Measure("Merge English over Spanish", () => DictionaryUtils.Merge(englishTsv, spanishTsv));
        var mergedBytes = await MeasureAsync("Serialize merged .loc", () => serializer.Serialize(mergedTsv));

        Assert.NotEmpty(englishTsv);
        Assert.NotEmpty(spanishTsv);
        Assert.NotEmpty(mergedTsv);
        Assert.NotEmpty(mergedBytes);
    }

    private T Measure<T>(string name, Func<T> action)
    {
        var before = Snapshot();
        var stopwatch = Stopwatch.StartNew();
        var result = action();
        stopwatch.Stop();

        WriteMeasurement(name, before, stopwatch.Elapsed);
        return result;
    }

    private async Task<T> MeasureAsync<T>(string name, Func<Task<T>> action)
    {
        var before = Snapshot();
        var stopwatch = Stopwatch.StartNew();
        var result = await action();
        stopwatch.Stop();

        WriteMeasurement(name, before, stopwatch.Elapsed);
        return result;
    }

    private static (long TotalAllocatedBytes, long TotalMemoryBytes) Snapshot()
    {
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        return (GC.GetTotalAllocatedBytes(precise: true), GC.GetTotalMemory(forceFullCollection: true));
    }

    private void WriteMeasurement(
        string name,
        (long TotalAllocatedBytes, long TotalMemoryBytes) before,
        TimeSpan elapsed)
    {
        var allocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - before.TotalAllocatedBytes;
        var memoryDeltaBytes = GC.GetTotalMemory(forceFullCollection: false) - before.TotalMemoryBytes;

        output.WriteLine(
            $"{name}: {elapsed.TotalMilliseconds:N0} ms, allocated {ToMegabytes(allocatedBytes):N1} MB, heap delta {ToMegabytes(memoryDeltaBytes):N1} MB");
    }

    private static double ToMegabytes(long bytes)
    {
        return bytes / 1024d / 1024d;
    }

}
