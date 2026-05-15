using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using BDOLanguageUpdater.Service.Serializer;

namespace BDOLanguageUpdater.Tests;

public sealed class LocalizationFileStructureTests
{
    [Fact]
    public async Task Serialize_WithKnownTsv_WritesExpectedBinaryRecordStructure()
    {
        const string tsv = "1\t2\t3\t4\t5\tHello\n" +
                           "10\t20\t30\t40\t50\t'+Formula<lf><quot>Quoted<quot>";
        var serializer = new LocSerializer();

        var bytes = await serializer.Serialize(tsv);
        var parsedFile = LocFileParser.Parse(bytes);

        Assert.Equal(2, parsedFile.Records.Count);
        Assert.Equal(0x78, bytes[4]);
        Assert.Equal(0x01, bytes[5]);
        Assert.All(parsedFile.Records, record => Assert.Equal(0u, record.Footer));

        Assert.Equal(new LocRecordKey(1, 2, 3, 4, 5), parsedFile.Records[0].Key);
        Assert.Equal("Hello", parsedFile.Records[0].Text);
        Assert.Equal(5u, parsedFile.Records[0].TextLength);

        Assert.Equal(new LocRecordKey(10, 20, 30, 40, 50), parsedFile.Records[1].Key);
        Assert.Equal("+Formula\n\"Quoted\"", parsedFile.Records[1].Text);
        Assert.Equal(17u, parsedFile.Records[1].TextLength);
    }

    [Fact]
    public async Task EnglishIntoSpanishMerge_PreservesSpanishRecordStructureAndCanRoundTrip()
    {
        var serializer = new LocSerializer();
        var englishPath = TestFilePaths.GetTestDataPath("languagedata_en.loc");
        var spanishPath = TestFilePaths.GetTestDataPath("languagedata_es.loc");

        var englishBytes = await File.ReadAllBytesAsync(englishPath);
        var spanishBytes = await File.ReadAllBytesAsync(spanishPath);
        var originalSpanish = LocFileParser.Parse(spanishBytes);

        var englishTsv = (string)await serializer.Deserialize(typeof(string), englishBytes, englishPath);
        var spanishTsv = (string)await serializer.Deserialize(typeof(string), spanishBytes, spanishPath);
        var mergedTsv = DictionaryUtils.Merge(englishTsv, spanishTsv);
        var mergedBytes = await serializer.Serialize(mergedTsv);
        var mergedFile = LocFileParser.Parse(mergedBytes);
        var roundTrippedTsv = (string)await serializer.Deserialize(typeof(string), mergedBytes, "memory://merged.loc");

        Assert.Equal(mergedTsv, roundTrippedTsv);
        Assert.Equal(mergedFile.DeclaredInflatedLength, mergedFile.InflatedLength);
        Assert.Equal(originalSpanish.Records.Count, mergedFile.Records.Count);

        for (var i = 0; i < originalSpanish.Records.Count; i++)
        {
            Assert.Equal(originalSpanish.Records[i].Key, mergedFile.Records[i].Key);
            Assert.Equal(originalSpanish.Records[i].Footer, mergedFile.Records[i].Footer);
        }
    }

    [Fact]
    public async Task RealLocalizationFixtures_AreStructurallyValid()
    {
        var english = LocFileParser.Parse(await File.ReadAllBytesAsync(TestFilePaths.GetTestDataPath("languagedata_en.loc")));
        var spanish = LocFileParser.Parse(await File.ReadAllBytesAsync(TestFilePaths.GetTestDataPath("languagedata_es.loc")));

        Assert.NotEmpty(english.Records);
        Assert.NotEmpty(spanish.Records);
        Assert.Equal(english.DeclaredInflatedLength, english.InflatedLength);
        Assert.Equal(spanish.DeclaredInflatedLength, spanish.InflatedLength);
        Assert.All(english.Records, record => Assert.Equal(0u, record.Footer));
        Assert.All(spanish.Records, record => Assert.Equal(0u, record.Footer));
    }

    private static class LocFileParser
    {
        private const int FileHeaderLength = 6;
        private const int RecordHeaderLength = 16;
        private const int RecordFooterLength = 4;

        public static ParsedLocFile Parse(byte[] bytes)
        {
            Assert.True(bytes.Length >= FileHeaderLength, "The .loc file must start with a 6-byte compression header.");
            Assert.Equal(0x78, bytes[4]);
            Assert.Equal(0x01, bytes[5]);

            var declaredInflatedLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(0, sizeof(uint))));
            var inflatedBytes = Inflate(bytes);

            Assert.Equal(declaredInflatedLength, inflatedBytes.Length);

            return new ParsedLocFile(
                declaredInflatedLength,
                inflatedBytes.Length,
                ParseRecords(inflatedBytes));
        }

        private static byte[] Inflate(byte[] bytes)
        {
            using var compressedStream = new MemoryStream(bytes, FileHeaderLength, bytes.Length - FileHeaderLength, writable: false);
            using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
            using var inflatedStream = new MemoryStream();
            deflateStream.CopyTo(inflatedStream);
            return inflatedStream.ToArray();
        }

        private static IReadOnlyList<LocRecord> ParseRecords(byte[] inflatedBytes)
        {
            var records = new List<LocRecord>();
            var offset = 0;

            while (offset < inflatedBytes.Length)
            {
                Assert.True(
                    inflatedBytes.Length - offset >= RecordHeaderLength + RecordFooterLength,
                    $"Record at offset {offset} does not contain the required header and footer bytes.");

                var textLength = BinaryPrimitives.ReadUInt32LittleEndian(inflatedBytes.AsSpan(offset, sizeof(uint)));
                offset += sizeof(uint);
                var strType = BinaryPrimitives.ReadUInt32LittleEndian(inflatedBytes.AsSpan(offset, sizeof(uint)));
                offset += sizeof(uint);
                var strId1 = BinaryPrimitives.ReadUInt32LittleEndian(inflatedBytes.AsSpan(offset, sizeof(uint)));
                offset += sizeof(uint);
                var strId2 = BinaryPrimitives.ReadUInt16LittleEndian(inflatedBytes.AsSpan(offset, sizeof(ushort)));
                offset += sizeof(ushort);
                var strId3 = inflatedBytes[offset++];
                var strId4 = inflatedBytes[offset++];

                var textByteLength = checked((int)textLength * sizeof(char));
                Assert.True(
                    inflatedBytes.Length - offset >= textByteLength + RecordFooterLength,
                    $"Record at offset {offset} declares text that extends beyond the inflated file.");

                var text = Encoding.Unicode.GetString(inflatedBytes, offset, textByteLength);
                offset += textByteLength;

                var footer = BinaryPrimitives.ReadUInt32LittleEndian(inflatedBytes.AsSpan(offset, sizeof(uint)));
                offset += sizeof(uint);

                records.Add(new LocRecord(
                    new LocRecordKey(strType, strId1, strId2, strId3, strId4),
                    textLength,
                    text,
                    footer));
            }

            Assert.Equal(inflatedBytes.Length, offset);
            return records;
        }
    }

    private sealed record ParsedLocFile(
        int DeclaredInflatedLength,
        int InflatedLength,
        IReadOnlyList<LocRecord> Records);

    private sealed record LocRecord(
        LocRecordKey Key,
        uint TextLength,
        string Text,
        uint Footer);

    private readonly record struct LocRecordKey(
        uint StrType,
        uint StrId1,
        ushort StrId2,
        byte StrId3,
        byte StrId4);
}
