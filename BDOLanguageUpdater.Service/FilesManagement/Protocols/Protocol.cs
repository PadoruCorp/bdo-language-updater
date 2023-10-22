using System;
using System.Threading.Tasks;

namespace Padoru.Core.Files
{
    public class Protocol : IProtocol
    {
        private readonly ISerializer serializer;
        private readonly IFileSystem fileSystem;
        
        public Protocol(ISerializer serializer, IFileSystem fileSystem)
        {
            this.serializer = serializer;
            this.fileSystem = fileSystem;
        }
        
        public async Task<bool> Exists(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("The provided uri is null or empty");
            }

            return await fileSystem.Exists(uri);
        }

        public async Task<object> Read<T>(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("The provided uri is null or empty");
            }
                
            var file = await fileSystem.Read(uri);
                    
            var bytes = file.Data;

            var result = await serializer.Deserialize(typeof(T), bytes, uri);

            return result;
        }

        public async Task<File<T>> Write<T>(string uri, T value)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("The provided uri is null or empty");
            }
                
            var bytes = await serializer.Serialize(value);

            var newFile = new File<byte[]>(uri, bytes);

            await fileSystem.Write(newFile);

            return new File<T>(uri, value);
        }

        public async Task Delete(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("The provided uri is null or empty");
            }
                
            if (!await fileSystem.Exists(uri))
            {
                throw new Exception($"Cannot delete file because it does not exists: {uri}");
            }
            
            await fileSystem.Delete(uri);
        }
    }
}
