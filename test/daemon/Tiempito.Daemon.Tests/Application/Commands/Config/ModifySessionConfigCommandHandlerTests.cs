using Moq;

using Tiempito.Daemon.Application.Commands.Config;
using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands.Config;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Tests.Application.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="ModifySessionConfigCommandHandler"/> class.
/// </summary>
[Trait("Commands", "Unit")]
public sealed class ModifySessionConfigCommandHandlerTests
{
    private const string FocusDurationString = "1m";
    private const string BreakDurationString = "2m";
    private const string DelayDurationString = "5s";

    private readonly Mock<ISessionConfigService> _sessionConfigServiceMock;
    private readonly Mock<ITimeSpanConverter> _timeSpanConverterMock;
    private readonly ModifySessionConfigCommandHandler _handler;
    private readonly ModifySessionConfigCommand _stubCommand = ModifySessionConfigCommand.CreateNew(
        sessionConfigId: "config1",
        targetCycles: 0,
        focusDuration: "1m",
        breakDuration: "2m",
        delayBetweenTimes: "5s");

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifySessionConfigCommandHandlerTests"/> class.
    /// </summary>
    public ModifySessionConfigCommandHandlerTests()
    {
        _sessionConfigServiceMock = new Mock<ISessionConfigService>();
        _timeSpanConverterMock = new Mock<ITimeSpanConverter>();
        _handler = new ModifySessionConfigCommandHandler(
            _sessionConfigServiceMock.Object,
            _timeSpanConverterMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="ModifySessionConfigCommandHandler.CanHandle"/> returns true when given a <see cref="ModifySessionConfigCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnTrue_when_CommandIsModifySessionConfig()
    {
        // Act
        bool actual = _handler.CanHandle(_stubCommand);

        // Assert
        Assert.True(actual);
    }

    /// <summary>
    /// Tests that <see cref="ModifySessionConfigCommandHandler.CanHandle"/> returns false when given a command that is not <see cref="ModifySessionConfigCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnFalse_when_CommandIsNotModifySessionConfig()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.False(actual);
    }

    /// <summary>
    /// Tests that <see cref="ModifySessionConfigCommandHandler.HandleAsync"/> returns the expected result
    /// based on the success parameter, verifying correct parsing and service invocation.
    /// </summary>
    /// <param name="success">Indicates whether the operation should succeed or fail.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_should_ReturnExpectedResult(bool success)
    {
        // Arrange
        var focusDuration = TimeSpan.FromMinutes(1);
        var breakDuration = TimeSpan.FromMinutes(2);
        var delayBetweenTimes = TimeSpan.FromSeconds(5);
        var expectedResult = new OperationResult(success, Message: "Test message.");
        _timeSpanConverterMock
            .Setup(m => m.TryConvert(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny))
            .Returns((string input, out TimeSpan result) =>
            {
                result = input switch
                {
                    FocusDurationString => focusDuration,
                    BreakDurationString => breakDuration,
                    DelayDurationString => delayBetweenTimes,
                    _ => TimeSpan.Zero,
                };
                return true;
            });
        _sessionConfigServiceMock.Setup(
                m => m.ModifyConfigAsync(
                    _stubCommand.SessionConfigId, 0, delayBetweenTimes, focusDuration, breakDuration))
            .ReturnsAsync(expectedResult);

        // Act
        OperationResult actualResult = await _handler.HandleAsync(_stubCommand);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _timeSpanConverterMock.Verify(m => m.TryConvert(FocusDurationString, out It.Ref<TimeSpan>.IsAny), Times.Once);
        _timeSpanConverterMock.Verify(m => m.TryConvert(BreakDurationString, out It.Ref<TimeSpan>.IsAny), Times.Once);
        _timeSpanConverterMock.Verify(m => m.TryConvert(DelayDurationString, out It.Ref<TimeSpan>.IsAny), Times.Once);
        _sessionConfigServiceMock.Verify(
            m => m.ModifyConfigAsync(
                _stubCommand.SessionConfigId, 0, delayBetweenTimes, focusDuration, breakDuration), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="ModifySessionConfigCommandHandler.HandleAsync"/> falls back to null values
    /// for durations when parsing fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_should_FallbackToNull_when_DurationsCannotBeParsed()
    {
        // Arrange
        const string invalidDuration = "invalid";
        var cmd = ModifySessionConfigCommand.CreateNew(
            sessionConfigId: "config1",
            targetCycles: 0,
            focusDuration: invalidDuration,
            breakDuration: invalidDuration,
            delayBetweenTimes: invalidDuration);
        TimeSpan? focusDuration = null;
        TimeSpan? breakDuration = null;
        TimeSpan? delayBetweenTimes = null;
        var expectedResult = new OperationResult(true, Message: "Test message.");
        _timeSpanConverterMock
            .Setup(m => m.TryConvert(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny))
            .Returns(false);
        _sessionConfigServiceMock.Setup(
                m => m.ModifyConfigAsync(
                    _stubCommand.SessionConfigId, 0, delayBetweenTimes, focusDuration, breakDuration))
            .ReturnsAsync(expectedResult);

        // Act
        OperationResult actualResult = await _handler.HandleAsync(cmd);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _timeSpanConverterMock.Verify(m => m.TryConvert(invalidDuration, out It.Ref<TimeSpan>.IsAny), Times.Exactly(3));
        _sessionConfigServiceMock.Verify(
            m => m.ModifyConfigAsync(
                _stubCommand.SessionConfigId, 0, delayBetweenTimes, focusDuration, breakDuration), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="ModifySessionConfigCommandHandler.HandleAsync"/> throws an <see cref="ArgumentException"/>
    /// when the provided command is not a <see cref="ModifySessionConfigCommand"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_should_ThrowArgumentException_when_CommandIsNotModifySessionConfig()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }
}