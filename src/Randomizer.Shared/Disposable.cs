using System;

namespace Randomizer.Shared;

/// <summary>
/// Provides a mechanism for running a callback when the object is disposed.
/// </summary>
public class Disposable : IDisposable
{
    private readonly Action _dispose;

    /// <summary>
    /// Initializes a new instance of the <see cref="Disposable"/> class.
    /// </summary>
    /// <param name="dispose">
    /// The action to invoke when <see cref="Dispose"/> is called.
    /// </param>
    public Disposable(Action dispose)
    {
        _dispose = dispose;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _dispose();
        GC.SuppressFinalize(this);
    }
}
