namespace Tiempito.IPC.NET.Messages;

/// <summary>
/// Represents the status code of a response.
/// </summary>
/// <summary>
/// Represents the status code of a response.
/// </summary>
public enum ResponseStatusCode
{
    /// <summary>
    /// The request was successful.
    /// </summary>
    Ok = 200,

    /// <summary>
    /// The request was invalid.
    /// </summary>
    BadRequest = 400,

    /// <summary>
    /// An internal server error occurred.
    /// </summary>
    Error = 500
}
