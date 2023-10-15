using System;
using System.IO;

namespace Padoru.Core.Files
{
    public static class FileUtils
    {
        public static string PathFromUri(string uri)
        {
            var uriSeparator = "://";
            var index = uri.IndexOf(uriSeparator, StringComparison.Ordinal);
            if (index == -1)
            {
                return uri;
            }

            return uri.Substring(index + uriSeparator.Length);
        }
        
        public static string PathFileNameUri(string uri, bool includeExtension = true)
        {
            var path = PathFromUri(uri);

            if (includeExtension)
            {
                return Path.GetFileName(path);
            }

            return Path.GetFileNameWithoutExtension(path);
        }

        public static string ValidatedFileName(string filePath)
        {
            filePath = filePath.Replace('\\', '/');
            var parts = filePath.Split('/');
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var invalidChars = Path.GetInvalidFileNameChars();
                for (var index = 0; index < invalidChars.Length; index++)
                {
                    var c = invalidChars[index];

                    if (c == ':')
                    {
                        continue;
                    }

                    part = part.Replace(c, '_');
                }

                parts[i] = part;
            }

            return string.Join("/", parts);
        }
    }
}