using Microsoft.Extensions.DependencyInjection;

using Moq;

using Tiempito.Daemon.Application.Commands;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Enums;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Tests.Application.Commands;

/// <summary>
/// Unit tests for the <see cref="CommandDispatcher"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Command")]
public class CommandDispatcherTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<ICommandHandler> _commandHandlerMock;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly CommandDispatcher _dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandDispatcherTests"/> class.
    /// </summary>
    public CommandDispatcherTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        Mock<IServiceScopeFactory> scopeFactoryMock = _mockRepository.Create<IServiceScopeFactory>();
        _scopeMock = _mockRepository.Create<IServiceScope>();
        _commandHandlerMock = _mockRepository.Create<ICommandHandler>();
        _dispatcher = new CommandDispatcher(scopeFactoryMock.Object);

        ICommandHandler[] handlers = [_commandHandlerMock.Object];
        scopeFactoryMock.Setup(m => m.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(m => m.ServiceProvider.GetService(typeof(IEnumerable<ICommandHandler>)))
            .Returns(handlers);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="CommandDispatcher.DispatchAsync"/> returns the expected <see cref="Response"/>
    /// when the <see cref="ICommandHandler"/> executes successfully.
    /// </summary>
    /// <param name="success">Indicates if the command handler should succeed.</param>
    /// <param name="expectedStatus">The expected <see cref="ResponseStatusCode"/> returned in the response.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true, ResponseStatusCode.Ok)]
    [InlineData(false, ResponseStatusCode.BadRequest)]
    public async Task DispatchAsync_should_ReturnExpectedResponse_when_CommandHandlerExecutesSuccessfully(
        bool success,
        ResponseStatusCode expectedStatus)
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();
        var cmdResult = new OperationResult(success, "A test message.");
        _commandHandlerMock.Setup(m => m.CanHandle(command)).Returns(true);
        _commandHandlerMock.Setup(m => m.HandleAsync(command, _cancellationToken)).ReturnsAsync(cmdResult);

        // Act
        Response response = await _dispatcher.DispatchAsync(command, _cancellationToken);

        // Assert
        Assert.NotEqual(command.Id, response.Id);
        Assert.Equal(command.CorrelationId, response.CorrelationId);
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(cmdResult.Success, response.Success);
        Assert.Equal(cmdResult.Message, response.Message);
    }

    /// <summary>
    /// Tests that <see cref="CommandDispatcher.DispatchAsync"/> disposes the service scope after execution.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DispatchAsync_should_DisposeServiceScopeAfterExecution()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();
        var cmdResult = new OperationResult(true, "A test message.");
        _commandHandlerMock.Setup(m => m.CanHandle(It.IsAny<Command>())).Returns(true);
        _commandHandlerMock.Setup(m => m.HandleAsync(command, _cancellationToken)).ReturnsAsync(cmdResult);

        // Act
        await _dispatcher.DispatchAsync(command, _cancellationToken);

        // Assert
        _scopeMock.Verify(m => m.Dispose(), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="CommandDispatcher.DispatchAsync"/> throws an <see cref="InvalidOperationException"/>
    /// when no handler is available for the given command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DispatchAsync_should_ThrowInvalidOperationException_when_NoHandlerForCommand()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();
        _commandHandlerMock.Setup(m => m.CanHandle(command)).Returns(false);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _dispatcher.DispatchAsync(command, _cancellationToken).AsTask());
    }
}