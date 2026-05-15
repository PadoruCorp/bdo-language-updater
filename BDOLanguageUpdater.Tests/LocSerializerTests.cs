using BDOLanguageUpdater.Service.Serializer;

namespace BDOLanguageUpdater.Tests;

public sealed class LocSerializerTests
{
    [Fact]
    public async Task SerializeDeserialize_WithEscapedText_RoundTrips()
    {
        const string tsv = "1\t2\t3\t4\t5\tHello<quot>World<quot>\n" +
                           "2\t3\t4\t5\t6\t'+Formula<lf>Line";
        var serializer = new LocSerializer();

        var bytes = await serializer.Serialize(tsv);
        var deserialized = await serializer.Deserialize(typeof(string), bytes, "memory://test.loc");

        Assert.Equal(tsv, deserialized);
    }

    [Fact]
    public async Task SerializeDeserialize_WithTextStartingWithApostrophe_RoundTrips()
    {
        const string tsv = "18\t3202\t1\t0\t4\t'Course the Lord's all-powerful";
        var serializer = new LocSerializer();

        var bytes = await serializer.Serialize(tsv);
        var deserialized = await serializer.Deserialize(typeof(string), bytes, "memory://test.loc");

        Assert.Equal(tsv, deserialized);
    }
}
