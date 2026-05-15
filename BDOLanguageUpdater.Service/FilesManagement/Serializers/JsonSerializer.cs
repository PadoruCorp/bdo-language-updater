using System;
using System.Text.Json;
using System.Threading.Tasks;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace Padoru.Core.Files
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions options;

        public JsonSerializer()
        {
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
            };
        }

        public JsonSerializer(JsonSerializerOptions options)
        {
            this.options = options;
        }

        public Task<byte[]> Serialize(object value)
        {
            var bytes = SystemJsonSerializer.SerializeToUtf8Bytes(value, value.GetType(), options);

            return Task.FromResult(bytes);
        }

        public Task<object> Deserialize(Type type, byte[] bytes, string uri)
        {
            var value = SystemJsonSerializer.Deserialize(bytes, type, options)
                        ?? throw new InvalidOperationException($"Could not deserialize '{uri}' as {type.Name}.");

            return Task.FromResult(value);
        }
    }
}
