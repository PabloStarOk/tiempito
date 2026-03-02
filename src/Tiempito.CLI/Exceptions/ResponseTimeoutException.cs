namespace Tiempito.CLI.Exceptions;

/// <summary>
/// Exception thrown when a response is not received within the expected timeout period.
/// </summary>
public sealed class ResponseTimeoutException : TimeoutException
{
}