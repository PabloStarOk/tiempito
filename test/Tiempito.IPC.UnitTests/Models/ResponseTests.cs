using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.IPC.UnitTests.Models;

/// <summary>
/// Unit tests for the <see cref="Response"/> record.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class ResponseTests
{
    /// <summary>
    /// Tests that <see cref="Response.Ok"/> returns a successful response with the specified parameters.
    /// </summary>
    [Fact]
    public void Ok_should_ReturnSuccessResponseWithSpecifiedParameters()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        const string message = "This is a test message";

        // Act
        var actual = Response.Ok(correlationId, message);

        // Assert
        Assert.True(actual.Success);
        Assert.Equal(correlationId, actual.CorrelationId);
        Assert.Equal(message, actual.Message);
        Assert.Equal(ResponseStatusCode.Ok, actual.StatusCode);
    }

    /// <summary>
    /// Tests that <see cref="Response.BadRequest"/> returns an error response with the specified parameters.
    /// </summary>
    [Fact]
    public void BadRequest_should_ReturnErrorResponseWithSpecifiedParameters()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        const string message = "This is a test message";

        // Act
        var actual = Response.BadRequest(correlationId, message);

        // Assert
        Assert.False(actual.Success);
        Assert.Equal(correlationId, actual.CorrelationId);
        Assert.Equal(message, actual.Message);
        Assert.Equal(ResponseStatusCode.BadRequest, actual.StatusCode);
    }

    /// <summary>
    /// Tests that <see cref="Response.DaemonNotRunning"/> returns an error response with the specified correlation ID.
    /// </summary>
    [Fact]
    public void DaemonNotRunning_should_ReturnErrorResponseWithSpecifiedCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid();

        // Act
        var actual = Response.DaemonNotRunning(correlationId);

        // Assert
        Assert.False(actual.Success);
        Assert.Equal(correlationId, actual.CorrelationId);
        Assert.Equal(ResponseStatusCode.Error, actual.StatusCode);
    }

    /// <summary>
    /// Tests that <see cref="Response.Timeout"/> returns an error response with the specified parameters.
    /// </summary>
    [Fact]
    public void Timeout_should_ReturnErrorResponseWithSpecifiedParameters()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        const string message = "This is a test message";

        // Act
        var actual = Response.Timeout(correlationId, message);

        // Assert
        Assert.False(actual.Success);
        Assert.Equal(correlationId, actual.CorrelationId);
        Assert.Equal(message, actual.Message);
        Assert.Equal(ResponseStatusCode.Error, actual.StatusCode);
    }
}