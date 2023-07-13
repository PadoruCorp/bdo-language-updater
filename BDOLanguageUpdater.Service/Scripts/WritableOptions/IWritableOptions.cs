using System;
using Microsoft.Extensions.Options;

namespace BDOLanguageUpdater.Service;

public interface IWritableOptions<out T> : IOptionsSnapshot<T> where T : class, new()
{
    void Update(Action<T> applyChanges);
}
