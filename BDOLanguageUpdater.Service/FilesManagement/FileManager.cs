using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Padoru.Core.Files
{
    public class FileManager : IFileManager
    {
        private const string PROTOCOL_HEADER_REGEX = @"^\w+://$";

        private readonly Dictionary<string, IProtocol> protocols = new();
        private readonly Regex protocolRegex;
        private readonly Protocol defaultProtocol;

        public FileManager(ISerializer defaultSerializer, IFileSystem defaultFileSystem)
        {
            protocolRegex = new Regex(PROTOCOL_HEADER_REGEX);

            defaultProtocol = new Protocol(defaultSerializer, defaultFileSystem);
        }

        public void RegisterProtocol(string protocolHeader, ISerializer serializer, IFileSystem fileSystem)
        {
            if (serializer == null)
            {
                throw new Exception($"Cannot register protocol with a null {nameof(ISerializer)}");
            }

            if (fileSystem == null)
            {
                throw new Exception($"Cannot register protocol with a null {nameof(IFileSystem)}");
            }

            var protocol = new Protocol(serializer, fileSystem);

            RegisterProtocol(protocolHeader, protocol);
        }

        public void RegisterProtocol(string protocolHeader, IProtocol protocol)
        {
            if (!protocolRegex.IsMatch(protocolHeader ?? string.Empty))
            {
                throw new Exception($"Invalid protocol '{protocolHeader}'. Only word characters allowed.");
            }

            if (protocols.ContainsKey(protocolHeader))
            {
                throw new Exception($"Cannot register protocol {protocolHeader}, it is already registered");
            }

            protocols.Add(protocolHeader, protocol);
        }

        public bool UnregisterProtocol(string protocolHeader)
        {
            if (protocols.ContainsKey(protocolHeader))
            {
                protocols.Remove(protocolHeader);
                return true;
            }

            return false;
        }

        public async Task<bool> Exists(string uri)
        {
            return await GetProtocol(uri).Exists(uri);
        }

        public async Task<File<T>> Read<T>(string uri)
        {
            var value = await GetProtocol(uri).Read<T>(uri);

            return new File<T>(uri, (T)value);
        }

        public async Task<File<T>> Write<T>(string uri, T value)
        {
            return await GetProtocol(uri).Write<T>(uri, value);
        }

        public async Task Delete(string uri)
        {
            await GetProtocol(uri).Delete(uri);
        }

        private IProtocol GetProtocol(string uri)
        {
            foreach (var protocol in protocols)
            {
                if (uri.StartsWith(protocol.Key))
                {
                    return protocol.Value;
                }
            }

            return defaultProtocol;
        }
    }
}