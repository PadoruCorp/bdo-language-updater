using System.Threading.Tasks;

namespace Padoru.Core.Files
{
	public interface IProtocol
	{
		Task<bool> Exists(string uri);

		Task<object> Read<T>(string uri);

		Task<File<T>> Write<T>(string uri, T value);

		Task Delete(string uri);
	}
}
