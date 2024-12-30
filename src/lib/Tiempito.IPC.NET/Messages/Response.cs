namespace Tiempito.IPC.NET.Messages;

/// <summary>
/// Represents a response from the daemon.
/// </summary>
/// <param name="StatusCode">Status of the response.</param>
/// <param name="Success">If the requested operation was completed successfully.</param>
/// <param name="Message">A human-readable message for the client telling about the operation success or failure.</param>
public record Response(ResponseStatusCode StatusCode, bool Success, string Message)
{
    /// <summary>
    /// Returns a <see cref="Response"/> about an operation that was completed successfully.
    /// </summary>
    /// <param name="message">A human-readable message for the client telling about the operation success.</param>
    /// <returns>A <see cref="Response"/> with the information.</returns>
    public static Response Ok(string message)
    {
        return new Response(
            StatusCode: ResponseStatusCode.Ok,
            Success: true,
            Message: message);
    }
    
    /// <summary>
    /// Returns a <see cref="Response"/> about an operation that failed due to a client error.
    /// </summary>
    /// <param name="message">A human-readable message for the client telling about error.</param>
    /// <returns>A <see cref="Response"/> with the information.</returns>
    public static Response BadRequest(string message)
    {
        return new Response(
            StatusCode: ResponseStatusCode.BadRequest,
            Success: false,
            Message: message);
    }
    
    /// <summary>
    /// Returns a <see cref="Response"/> about an operation that failed due to a internal error of the daemon.
    /// </summary>
    /// <param name="message">A human-readable message for the client telling about error.</param>
    /// <returns>A <see cref="Response"/> with the information.</returns>
    public static Response InternalError(string message)
    {
        return new Response(
            StatusCode: ResponseStatusCode.Error,
            Success: false,
            Message: message);
    }
}

