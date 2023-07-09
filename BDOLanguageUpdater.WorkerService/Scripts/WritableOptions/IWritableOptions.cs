using Microsoft.Extensions.Options;

namespace BDLanguageUpdater.WorkerService;

public interface IWritableOptions<out T> : IOptionsSnapshot<T> where T : class, new()
{
    void Update(Action<T> applyChanges);
}
