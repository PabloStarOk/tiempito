namespace Tiempito.CLI.Services.Abstractions;

/// <summary>
/// Defines the application lifetime management interface.
/// </summary>
public interface IApplicationLifetime
{
    /// <summary>
    /// Gets the <see cref="CancellationToken"/> that is triggered when the application is stopping.
    /// </summary>
    public CancellationToken StoppingToken { get; }

    /// <summary>
    /// Requests the application to stop.
    /// </summary>
    public void StopApplication();
}