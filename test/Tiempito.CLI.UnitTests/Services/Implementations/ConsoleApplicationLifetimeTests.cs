using Tiempito.CLI.Services.Implementations;

namespace Tiempito.CLI.UnitTests.Services.Implementations;

/// <summary>
/// Unit tests for the <see cref="ConsoleApplicationLifetime"/> class.
/// </summary>
public sealed class ConsoleApplicationLifetimeTests : IDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly ConsoleApplicationLifetime _appLifetime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleApplicationLifetimeTests"/> class.
    /// </summary>
    public ConsoleApplicationLifetimeTests()
    {
        _cts = new CancellationTokenSource();
        _appLifetime = new ConsoleApplicationLifetime(_cts);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cts.Dispose();
    }

    /// <summary>
    /// Tests that calling <see cref="ConsoleApplicationLifetime.StopApplication"/> cancels the provided <see cref="CancellationTokenSource"/>.
    /// </summary>
    [Fact]
    public void StopApplication_should_CancelGivenTokenSource()
    {
        // Act
        _appLifetime.StopApplication();

        // Assert
        Assert.True(_cts.IsCancellationRequested);
    }
}