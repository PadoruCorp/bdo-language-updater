using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Padoru.Core.Files
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings settings;

        public JsonSerializer()
        {
            settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };
        }
        
        public JsonSerializer(JsonSerializerSettings settings)
        {
            this.settings = settings;
        }

        public Task<byte[]> Serialize(object value)
        {
            var text = JsonConvert.SerializeObject(value, settings);
            
            var bytes = Encoding.UTF8.GetBytes(text);
            
            return Task.FromResult(bytes);
        }

        public Task<object> Deserialize(Type type, byte[] bytes, string uri)
        {
            var text = Encoding.UTF8.GetString(bytes);
            
            var value = JsonConvert.DeserializeObject(text, type, settings);
            
            return Task.FromResult(value);
        }
    }
}