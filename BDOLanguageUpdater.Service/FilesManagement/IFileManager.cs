using System.Threading.Tasks;

namespace Padoru.Core.Files
{
    public interface IFileManager
    {
        void RegisterProtocol(string protocolHeader, ISerializer serializer, IFileSystem fileSystem);
        
        void RegisterProtocol(string protocolHeader, IProtocol protocol);

        bool UnregisterProtocol(string protocolHeader);

        Task<bool> Exists(string uri);

        Task<File<T>> Read<T>(string uri);

        Task<File<T>> Write<T>(string uri, T value);

        Task Delete(string uri);
    }
}
