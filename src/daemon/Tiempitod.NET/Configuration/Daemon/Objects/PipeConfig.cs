using System.IO.Pipes;
using System.Text;

namespace Tiempitod.NET.Configuration.Daemon.Objects;

/// <summary>
/// Represents the configuration for the named pipes of the command server.
/// </summary>
public class PipeConfig
{
    public const string Pipe = "Pipe";

    public string PipeName { get; init; } = "tiempito-pipe";
    public PipeDirection PipeDirection { get; init; } = PipeDirection.InOut;
    public int PipeMaxInstances { get; init; } = 1;
    public string PipeEncoding { get; init; } = "utf8";
    public int MaxRestartAttempts { get; init; } = 3;
    public bool DisplayImpersonationUser { get; init; } = true;

    /// <summary>
    /// Gets the encoding type according based in the current configuration.
    /// </summary>
    /// <returns>An Encoding type.</returns>
    public Encoding GetEncoding()
    {
        string formattedEncoding = PipeEncoding.ToLower().Replace("-", "");

        return formattedEncoding switch
        {
            "utf8" => new UTF8Encoding(),
            "ascii" => new ASCIIEncoding(),
            "utf32" => new UTF32Encoding(),
            "unicode" => new UnicodeEncoding(),
            _ => new UTF8Encoding()
        };
    }
}
