namespace Tiempitod.NET.Common;

/// <summary>
/// Represents an the result of an operation of a daemon service.
/// </summary>
/// <param name="Success">If the operation was completed successfully.</param>
/// <param name="Message">A human-readable message for the client telling about the operation success or failure.</param>
public record OperationResult(bool Success, string Message);
