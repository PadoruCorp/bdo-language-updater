namespace Padoru.Core.Files
{
	public class FileNameGeneratorPath : IFileNameGenerator
	{
		public string GetName(string uri)
		{
			return FileUtils.PathFromUri(uri);
		}
	}
}
