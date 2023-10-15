namespace Padoru.Core.Files
{
    public class File<T>
    {
        public readonly string Uri;
        public readonly T Data;

        public File(string uri, T data)
        {
            Uri = uri;
            Data = data;
        }
    }
}