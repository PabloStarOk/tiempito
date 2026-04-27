namespace Tiempito.Daemon.Domain.Shared;

/// <summary>
/// Represents the result of an operation.
/// </summary>
/// <param name="Success">If the operation was completed successfully.</param>
/// <param name="Message">A human-readable message for the client telling about the operation success or failure.</param>
public record OperationResult(bool Success, string Message);
