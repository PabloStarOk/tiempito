namespace Tiempito.CLI;

/// <summary>
/// Manages the application's lifetime and handles graceful shutdown on cancellation events.
/// </summary>
internal sealed class ApplicationLifetime : IDisposable
{
    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with the application's lifetime.
    /// </summary>
    public CancellationToken Token => _appTokenSource.Token;

    private readonly CancellationTokenSource _appTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationLifetime"/> class.
    /// </summary>
    /// <param name="appTokenSource">The <see cref="CancellationTokenSource"/> used to control application lifetime.</param>
    public ApplicationLifetime(CancellationTokenSource appTokenSource)
    {
        _appTokenSource = appTokenSource;
        Console.CancelKeyPress += StopApplication;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Console.CancelKeyPress -= StopApplication;
        _appTokenSource.Dispose();
    }

    private void StopApplication(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        _appTokenSource.Cancel();
    }
}
