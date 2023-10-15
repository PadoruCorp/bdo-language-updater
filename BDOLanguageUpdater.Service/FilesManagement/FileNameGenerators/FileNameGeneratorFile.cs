namespace Padoru.Core.Files
{
	public class FileNameGeneratorFile : IFileNameGenerator
	{
		private readonly bool includeExtension;
		
		public FileNameGeneratorFile(bool includeExtension)
		{
			this.includeExtension = includeExtension;
		}
		
		public string GetName(string uri)
		{
			return FileUtils.PathFileNameUri(uri, includeExtension);
		}
	}
}
