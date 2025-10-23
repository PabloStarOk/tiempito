using MessagePack;

using Tiempito.IPC.Enums;

namespace Tiempito.IPC.Models;

/// <summary>
/// Represents a response.
/// </summary>
[MessagePackObject]
public record Response : Message
{
    /// <summary>
    /// Gets the <see cref="ResponseStatusCode"/> representing the result of the operation.
    /// </summary>
    [Key(3)]
    public ResponseStatusCode StatusCode { get; }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    [Key(4)]
    public bool Success { get; }

    /// <summary>
    /// Gets a human-readable message describing the outcome.
    /// </summary>
    [Key(5)]
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Response"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this response.</param>
    /// <param name="correlationId">Correlation identifier to associate this response with an originating message.</param>
    /// <param name="timestamp">The UTC timestamp when the response was created.</param>
    /// <param name="statusCode">The <see cref="ResponseStatusCode"/> representing the result.</param>
    /// <param name="success">True when the operation succeeded; otherwise false.</param>
    /// <param name="message">A human-readable message describing the outcome.</param>
    public Response(
        Guid id,
        Guid correlationId,
        DateTimeOffset timestamp,
        ResponseStatusCode statusCode,
        bool success,
        string message)
        : base(id, correlationId, timestamp)
    {
        StatusCode = statusCode;
        Success = success;
        Message = message;
    }

    /// <summary>
    /// Creates a new successful <see cref="Response"/> with <see cref="ResponseStatusCode.Ok"/>.
    /// </summary>
    /// <param name="correlationId">Correlation identifier to associate this response with an originating message.</param>
    /// <param name="message">A human-readable message describing the outcome.</param>
    /// <returns>A newly constructed successful <see cref="Response"/>.</returns>
    public static Response Ok(Guid correlationId, string message)
    {
        return CreateNew(
            correlationId,
            statusCode: ResponseStatusCode.Ok,
            success: true,
            message: message);
    }

    /// <summary>
    /// Creates a new failed <see cref="Response"/> with <see cref="ResponseStatusCode.BadRequest"/>.
    /// </summary>
    /// <param name="correlationId">Correlation identifier to associate this response with an originating message.</param>
    /// <param name="message">A human-readable message describing the failure.</param>
    /// <returns>A newly constructed failed <see cref="Response"/>.</returns>
    public static Response BadRequest(Guid correlationId, string message)
    {
        return CreateNew(
            correlationId,
            statusCode: ResponseStatusCode.BadRequest,
            success: false,
            message: message);
    }

    /// <summary>
    /// Creates a new <see cref="Response"/> with a generated id and the current UTC timestamp.
    /// </summary>
    /// <param name="correlationId">Correlation identifier to associate this response with an originating message.</param>
    /// <param name="statusCode">The <see cref="ResponseStatusCode"/> representing the result.</param>
    /// <param name="success">True when the operation succeeded; otherwise false.</param>
    /// <param name="message">A human-readable message describing the outcome.</param>
    /// <returns>A newly constructed <see cref="Response"/>.</returns>
    private static Response CreateNew(
        Guid correlationId,
        ResponseStatusCode statusCode,
        bool success,
        string message)
    {
        return new Response(
            id: Guid.NewGuid(),
            correlationId,
            timestamp: DateTimeOffset.UtcNow,
            statusCode: statusCode,
            success: success,
            message: message);
    }
}
