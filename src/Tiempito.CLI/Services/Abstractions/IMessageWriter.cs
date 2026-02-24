namespace Tiempito.CLI.Services.Abstractions;

/// <summary>
/// Defines a writer for CLI messages.
/// </summary>
public interface IMessageWriter
{
    /// <summary>
    /// Asynchronously writes a message with a newline.
    /// </summary>
    /// <param name="error">If true, the message represents an error.</param>
    /// <param name="message">The message text to write.</param>
    /// <param name="cancellationToken">A token to cancel the write operation.</param>
    /// <returns>A ValueTask that completes when the write operation finishes.</returns>
    public ValueTask WriteLineAsync(bool error, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously writes a message without a newline.
    /// </summary>
    /// <param name="error">If true, the message represents an error.</param>
    /// <param name="message">The message text to write.</param>
    /// <param name="cancellationToken">A token to cancel the write operation.</param>
    /// <returns>A ValueTask that completes when the write operation finishes.</returns>
    public ValueTask WriteAsync(bool error, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously clears the current line in the CLI.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the clear operation.</param>
    /// <returns>A ValueTask that completes when the clear operation finishes.</returns>
    public ValueTask ClearLineAsync(CancellationToken cancellationToken = default);
}