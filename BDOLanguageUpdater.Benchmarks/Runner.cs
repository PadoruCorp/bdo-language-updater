using BDOLanguageUpdater.Benchmarks;
using BDOLanguageUpdater.Service.Serializer;
using BenchmarkDotNet.Attributes;

[Config(typeof(AntiVirusFriendlyConfig))]
[MemoryDiagnoser]
public class Runner
{
    private readonly byte[] languageFile = File.ReadAllBytes("languagedata_en.loc");
    
    [Benchmark]
    public async Task Decompress_Original()
    {
        var locSerializer = new LocSerializer();
        var englishTSV = await locSerializer.Deserialize(typeof(string), languageFile, string.Empty);
    }
    
    [Benchmark]
    public async Task Decompress_New()
    {
        await LocSerializerOptimized.Decompress(languageFile);
    }
    
    // [Benchmark]
    public void Merge()
    {
        DictionaryUtils.Merge(TestData.TsvFrom, TestData.TsvTo);
    }
}


public static class TestData
{
    public const string TsvTo = "1\t2\t3\t4\t5\tFrom\n2\t0\t0\t0\t0\tFrom\n3\t0\t0\t0\t0\tFrom\n4\t0\t0\t0\t0\tFrom";

    public const string TsvFrom = "1\t2\t3\t4\t5\tTo\n2\t0\t0\t0\t0\tTo\n3\t0\t0\t0\t0\tTo";
}