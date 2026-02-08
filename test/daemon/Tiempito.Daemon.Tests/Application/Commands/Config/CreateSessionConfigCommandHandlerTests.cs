using Moq;

using Tiempito.Daemon.Application.Commands.Config;
using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands.Config;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Tests.Application.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="CreateSessionConfigCommandHandler"/> class.
/// </summary>
[Trait("Commands", "Unit")]
public sealed class CreateSessionConfigCommandHandlerTests
{
    private const string FocusDurationString = "1m";
    private const string BreakDurationString = "2m";
    private const string DelayDurationString = "5s";

    private readonly Mock<ISessionConfigService> _sessionConfigServiceMock;
    private readonly Mock<ITimeSpanConverter> _timeSpanConverterMock;
    private readonly CreateSessionConfigCommandHandler _handler;
    private readonly CreateSessionConfigCommand _stubCommand = CreateSessionConfigCommand.CreateNew(
        sessionConfigId: "config1",
        targetCycles: 0,
        focusDuration: "1m",
        breakDuration: "2m",
        delayBetweenTimes: "5s");

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSessionConfigCommandHandlerTests"/> class.
    /// </summary>
    public CreateSessionConfigCommandHandlerTests()
    {
        _sessionConfigServiceMock = new Mock<ISessionConfigService>();
        _timeSpanConverterMock = new Mock<ITimeSpanConverter>();
        _handler = new CreateSessionConfigCommandHandler(
            _sessionConfigServiceMock.Object,
            _timeSpanConverterMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommandHandler.CanHandle"/> returns true when given a <see cref="CreateSessionConfigCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnTrue_when_CommandIsCreateSessionConfig()
    {
        // Act
        bool actual = _handler.CanHandle(_stubCommand);

        // Assert
        Assert.True(actual);
    }

    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommandHandler.CanHandle"/> returns false when given a command that is not a <see cref="CreateSessionConfigCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnFalse_when_CommandIsNotCreateSessionConfig()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.False(actual);
    }

    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommandHandler.HandleAsync"/> returns the expected <see cref="OperationResult"/>
    /// based on the success parameter when adding a session config.
    /// </summary>
    /// <param name="success">Indicates whether the operation should succeed or fail.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_should_ReturnExpectedResult(bool success)
    {
        // Arrange
        var sessionConfig = new SessionConfig(
            _stubCommand.SessionConfigId,
            TargetCycles: 0,
            DelayBetweenTimes: TimeSpan.FromMinutes(1),
            FocusDuration: TimeSpan.FromMinutes(2),
            BreakDuration: TimeSpan.FromSeconds(5));
        var expectedResult = new OperationResult(success, Message: "Test message.");
        _timeSpanConverterMock
            .Setup(m => m.TryConvert(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny))
            .Returns((string input, out TimeSpan result) =>
            {
                result = input switch
                {
                    FocusDurationString => sessionConfig.FocusDuration,
                    BreakDurationString => sessionConfig.BreakDuration,
                    DelayDurationString => sessionConfig.DelayBetweenTimes,
                    _ => TimeSpan.Zero,
                };
                return true;
            });
        _sessionConfigServiceMock.Setup(m => m.AddConfigAsync(sessionConfig)).ReturnsAsync(expectedResult);

        // Act
        OperationResult actualResult = await _handler.HandleAsync(_stubCommand);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _timeSpanConverterMock.Verify(m => m.TryConvert(FocusDurationString, out It.Ref<TimeSpan>.IsAny), Times.Once);
        _timeSpanConverterMock.Verify(m => m.TryConvert(BreakDurationString, out It.Ref<TimeSpan>.IsAny), Times.Once);
        _timeSpanConverterMock.Verify(m => m.TryConvert(DelayDurationString, out It.Ref<TimeSpan>.IsAny), Times.Once);
        _sessionConfigServiceMock.Verify(m => m.AddConfigAsync(sessionConfig), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommandHandler.HandleAsync"/> returns an error <see cref="OperationResult"/>
    /// when any of the duration strings (focus, break) cannot be converted by <see cref="ITimeSpanConverter"/>.
    /// </summary>
    /// <param name="duration">The duration string that should fail conversion.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(FocusDurationString)]
    [InlineData(BreakDurationString)]
    public async Task HandleAsync_should_ReturnErrorResult_when_GivenDurationStringCannotBeConverted(string duration)
    {
        // Arrange
        _timeSpanConverterMock.Setup(m => m.TryConvert(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny)).Returns(true);
        _timeSpanConverterMock.Setup(m => m.TryConvert(duration, out It.Ref<TimeSpan>.IsAny)).Returns(false);

        // Act
        OperationResult actualResult = await _handler.HandleAsync(_stubCommand);

        // Assert
        Assert.False(actualResult.Success);
        _timeSpanConverterMock.Verify(m => m.TryConvert(duration, out It.Ref<TimeSpan>.IsAny), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommandHandler.HandleAsync"/> throws an <see cref="ArgumentException"/>
    /// when the provided command is not a <see cref="CreateSessionConfigCommand"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_should_ThrowArgumentException_when_CommandIsNotCreateSessionConfig()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }
}