namespace Tiempitod.NET.Server.Messages;

/// <summary>
/// Represents a response from the daemon for the client.
/// </summary>
/// <param name="StatusCode">Status of the response.</param>
/// <param name="Success">If the requested operation was completed successfully.</param>
/// <param name="Message">A human-readable message for the client telling about the operation success or failure.</param>
public record DaemonResponse(DaemonStatusCode StatusCode, bool Success, string Message)
{
    /// <summary>
    /// Returns a <see cref="DaemonResponse"/> about an operation that was completed successfully.
    /// </summary>
    /// <param name="message">A human-readable message for the client telling about the operation success.</param>
    /// <returns>A <see cref="DaemonResponse"/> with the information.</returns>
    public static DaemonResponse Ok(string message)
    {
        return new DaemonResponse(
            StatusCode: DaemonStatusCode.Ok,
            Success: true,
            Message: message);
    }
    
    /// <summary>
    /// Returns a <see cref="DaemonResponse"/> about an operation that failed due to a client error.
    /// </summary>
    /// <param name="message">A human-readable message for the client telling about error.</param>
    /// <returns>A <see cref="DaemonResponse"/> with the information.</returns>
    public static DaemonResponse BadRequest(string message)
    {
        return new DaemonResponse(
            StatusCode: DaemonStatusCode.BadRequest,
            Success: false,
            Message: message);
    }
    
    /// <summary>
    /// Returns a <see cref="DaemonResponse"/> about an operation that failed due to a internal error of the daemon.
    /// </summary>
    /// <param name="message">A human-readable message for the client telling about error.</param>
    /// <returns>A <see cref="DaemonResponse"/> with the information.</returns>
    public static DaemonResponse InternalError(string message)
    {
        return new DaemonResponse(
            StatusCode: DaemonStatusCode.Error,
            Success: false,
            Message: message);
    }
}
