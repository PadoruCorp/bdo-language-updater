namespace BDOLanguageUpdater.Service.Serializer;

public static class DictionaryUtils
{
    public static string Merge(string fromFileTsv, string toFileTsv)
    {
        return LocalizationTsvMerger.Merge(fromFileTsv, toFileTsv);
    }
}

public struct LanguageFileLine
{
    public uint FirstId { get; set; }
    public uint SecondId { get; set; }
    public ushort ThirdId { get; set; }
    public byte FourthId { get; set; }
    public byte FifthId { get; set; }
    public string Text { get; set; }
}
