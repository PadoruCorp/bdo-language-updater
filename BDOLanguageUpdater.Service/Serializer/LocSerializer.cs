using Padoru.Core.Files;

namespace BDOLanguageUpdater.Service.Serializer;

public class LocSerializer : ISerializer
{
    public Task<byte[]> Serialize(object value)
    {
        var content = value.ToString() ?? throw new InvalidCastException();
        return Task.FromResult(LocBinaryTsvConverter.FromTsv(content));
    }

    public Task<object> Deserialize(Type type, byte[] bytes, string uri)
    {
        return Task.FromResult<object>(LocBinaryTsvConverter.ToTsv(bytes));
    }

    public Task<byte[]> InflateAsync(byte[] buffer)
    {
        return LocCompression.InflateAsync(buffer);
    }

    public Task<byte[]> Deflate(byte[] buffer)
    {
        return Task.FromResult(LocCompression.Deflate(buffer));
    }
}
