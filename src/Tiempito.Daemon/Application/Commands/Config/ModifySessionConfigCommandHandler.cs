using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands;
using Tiempito.IPC.Models.Commands.Config;

namespace Tiempito.Daemon.Application.Commands.Config;

/// <summary>
/// Handles modification of session configuration.
/// </summary>
/// <param name="sessionConfigService">Service for managing session configurations.</param>
/// <param name="timeSpanConverter">Service for converting time span strings.</param>
internal sealed class ModifySessionConfigCommandHandler(
    ISessionConfigService sessionConfigService,
    ITimeSpanConverter timeSpanConverter)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) => command is ModifySessionConfigCommand;

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (command is not ModifySessionConfigCommand modifyCmd)
        {
            throw new ArgumentException($"Command must be a {nameof(ModifySessionConfigCommand)}.", nameof(command));
        }

        TimeSpan? delayBetweenTimes = null;
        TimeSpan? focusDuration = null;
        TimeSpan? breakDuration = null;

        if (modifyCmd.DelayBetweenTimes is not null
            && timeSpanConverter.TryConvert(modifyCmd.DelayBetweenTimes, out TimeSpan parsedDelayBetweenTimes))
        {
            delayBetweenTimes = parsedDelayBetweenTimes;
        }

        if (modifyCmd.FocusDuration is not null
            && timeSpanConverter.TryConvert(modifyCmd.FocusDuration, out TimeSpan parsedFocusDuration))
        {
            focusDuration = parsedFocusDuration;
        }

        if (modifyCmd.BreakDuration is not null
            && timeSpanConverter.TryConvert(modifyCmd.BreakDuration, out TimeSpan parsedBreakDuration))
        {
            breakDuration = parsedBreakDuration;
        }

        return await sessionConfigService.ModifyConfigAsync(
            modifyCmd.SessionConfigId,
            (int?)modifyCmd.TargetCycles,
            delayBetweenTimes,
            focusDuration,
            breakDuration);
    }
}
