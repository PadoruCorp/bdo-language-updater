using System;
using System.Text;
using System.Threading.Tasks;

namespace Padoru.Core.Files
{
    public class StringSerializer : ISerializer
    {
        public Task<byte[]> Serialize(object value)
        {
            var bytes = Encoding.UTF8.GetBytes(value.ToString());
            
            return Task.FromResult(bytes);
        }

        public Task<object> Deserialize(Type type, byte[] bytes, string uri)
        {
            object value = Encoding.UTF8.GetString(bytes);
            
            return Task.FromResult(value);
        }
    }
}