using Tiempito.CLI.Services.Abstractions;

namespace Tiempito.CLI.Services.Implementations;

/// <summary>
/// Manages the console application's lifetime and handles graceful shutdown on cancellation events.
/// </summary>
internal sealed class ConsoleApplicationLifetime : IApplicationLifetime, IDisposable
{
    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with the application's lifetime.
    /// </summary>
    public CancellationToken StoppingToken => _appTokenSource.Token;

    private readonly CancellationTokenSource _appTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleApplicationLifetime"/> class.
    /// </summary>
    /// <param name="appTokenSource">The <see cref="CancellationTokenSource"/> used to control application lifetime.</param>
    public ConsoleApplicationLifetime(CancellationTokenSource appTokenSource)
    {
        _appTokenSource = appTokenSource;
        Console.CancelKeyPress += StopApplication;
    }

    /// <inheritdoc/>
    public void StopApplication()
    {
        _appTokenSource.Cancel();
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
        StopApplication();
    }
}
