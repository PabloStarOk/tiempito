using Tiempito.CLI.Services.Abstractions;

namespace Tiempito.CLI.Services.Implementations;

/// <summary>
/// Writes messages to the provided <see cref="TextWriter"/> instances.
/// Uses the standard output writer for non-error messages and the standard error writer for error messages.
/// </summary>
internal sealed class MessageWriter : IMessageWriter
{
    private readonly TextWriter _stdOut;
    private readonly TextWriter _stdError;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageWriter"/> class.
    /// </summary>
    /// <param name="stdOut">The writer used for non-error messages (standard output).</param>
    /// <param name="stdError">The writer used for error messages (standard error).</param>
    public MessageWriter(TextWriter stdOut, TextWriter stdError)
    {
        _stdOut = stdOut;
        _stdError = stdError;
    }

    /// <inheritdoc/>
    public async ValueTask WriteLineAsync(bool error, string message, CancellationToken cancellationToken = default)
    {
        if (error)
        {
            await _stdError.WriteLineAsync(message.AsMemory(), cancellationToken);
        }
        else
        {
            await _stdOut.WriteLineAsync(message.AsMemory(), cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(bool error, string message, CancellationToken cancellationToken = default)
    {
        if (error)
        {
            await _stdError.WriteAsync(message.AsMemory(), cancellationToken);
        }
        else
        {
            await _stdOut.WriteAsync(message.AsMemory(), cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async ValueTask ClearLineAsync(CancellationToken cancellationToken = default)
    {
        await _stdOut.WriteAsync("\r\x1B[2K".AsMemory(), cancellationToken);
    }
}