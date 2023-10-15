using System;
using System.Threading.Tasks;

namespace Padoru.Core.Files
{
    public interface ISerializer
    {
        Task<byte[]> Serialize(object value);

        Task<object> Deserialize(Type type, byte[] bytes, string uri);
    }
}