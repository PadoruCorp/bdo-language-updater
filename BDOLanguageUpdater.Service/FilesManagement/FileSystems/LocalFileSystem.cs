using System;
using System.IO;
using System.Threading.Tasks;

namespace Padoru.Core.Files
{
    public class LocalFileSystem : IFileSystem
    {
        private readonly string basePath;

        public LocalFileSystem(string basePath)
        {
            this.basePath = basePath;
        }

        public async Task<bool> Exists(string uri)
        {
            var path = GetFullPath(uri);

            return await Task.FromResult(File.Exists(path));
        }

        public async Task<File<byte[]>> Read(string uri)
        {
            var path = GetFullPath(uri);

            var bytes = await File.ReadAllBytesAsync(path);

            Console.WriteLine($"Read file from path '{path}'");

            return new File<byte[]>(uri, bytes);
        }

        public async Task Write(File<byte[]> file)
        {
            var path = GetFullPath(file.Uri);

            var directory = Path.GetDirectoryName(path) ?? ".";
            
            Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(path, file.Data);

            Console.WriteLine($"Wrote file to path '{path}'");
        }

        public async Task Delete(string uri)
        {
            var path = GetFullPath(uri);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find file. Uri {uri}");
            }
            
            File.Delete(path);
            
            await Task.CompletedTask;
        }

        private string GetFullPath(string uri)
        {
            return Path.Combine(basePath, FileUtils.ValidatedFileName(FileUtils.PathFromUri(uri)));
        }
    }
}