using System;
using System.Collections.Generic;
using System.Text;

namespace BDOLanguageUpdater.Service.Serializer;

public static class DictionaryUtils
{
    public static string Merge(string fromFileTsv, string toFileTsv)
    {
        var fromDictionary = FileContentToDictionary(fromFileTsv);
        var toDictionary = FileContentToDictionary(toFileTsv);

        foreach (var key in fromDictionary.Keys)
        {
            if (toDictionary.ContainsKey(key))
            {
                toDictionary[key] = fromDictionary[key];
            }
        }

        return DictionaryToFileContent(toDictionary);
    }

    private static Dictionary<string, string> FileContentToDictionary(string fileContent)
    {
        var idToTextMapping = new Dictionary<string, string>();
        foreach (var line in fileContent.Split('\n'))
        {
            var parts = line.Split('\t');
            if (parts.Length < 2) continue;
            var id = $"{parts[0]}\t{parts[1]}\t{parts[2]}\t{parts[3]}\t{parts[4]}\t";
            var text = parts[5];
            idToTextMapping[id] = text;
        }

        return idToTextMapping;
    }

    private static string DictionaryToFileContent(Dictionary<string, string> dictionary)
    {
        var builder = new StringBuilder();
        foreach (var valuePair in dictionary)
        {
            builder.Append(valuePair.Key);
            builder.Append(valuePair.Value);
            builder.Append(Environment.NewLine);
        }

        return builder.ToString().Trim();
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