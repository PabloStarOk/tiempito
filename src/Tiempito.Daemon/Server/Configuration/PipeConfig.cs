using System.IO.Pipes;
using System.Text;

namespace Tiempito.Daemon.Server.Configuration;

/// <summary>
/// Represents the configuration for the named pipes of the command server.
/// </summary>
public class PipeConfig
{
    /// <summary>
    /// The configuration section name for pipe settings.
    /// </summary>
    public const string Pipe = "Pipe";

    /// <summary>
    /// Gets the name of the named pipe.
    /// </summary>
    public string PipeName { get; init; } = "tiempito-pipe";

    /// <summary>
    /// Gets the direction of the pipe communication.
    /// </summary>
    public PipeDirection PipeDirection { get; init; } = PipeDirection.InOut;

    /// <summary>
    /// Gets the maximum number of pipe instances allowed.
    /// </summary>
    public int PipeMaxInstances { get; init; } = 1;

    /// <summary>
    /// Gets the encoding used for pipe communication.
    /// </summary>
    public string PipeEncoding { get; init; } = "utf-8";

    /// <summary>
    /// Gets a value indicating whether indicates whether to display the impersonation user.
    /// </summary>
    public bool DisplayImpersonationUser { get; init; } = true;

    /// <summary>
    /// Gets the encoding type according based in the current configuration.
    /// </summary>
    /// <returns>An Encoding type.</returns>
    public Encoding GetEncoding()
    {
        try
        {
            return Encoding.GetEncoding(PipeEncoding);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }
}
