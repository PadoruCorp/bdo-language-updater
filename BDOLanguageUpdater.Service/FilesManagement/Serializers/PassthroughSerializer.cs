using System;
using System.Threading.Tasks;

namespace Padoru.Core.Files;

public class PassthroughSerializer : ISerializer
{
    public Task<byte[]> Serialize(object value)
    {
        return Task.FromResult((byte[])value);
    }

    public Task<object> Deserialize(Type type, byte[] bytes, string uri)
    {
        return Task.FromResult((object)bytes);
    }
}