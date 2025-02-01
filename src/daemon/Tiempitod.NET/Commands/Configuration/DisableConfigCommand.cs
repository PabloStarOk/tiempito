using Tiempitod.NET.Common;
using Tiempitod.NET.Configuration.User.Interfaces;

namespace Tiempitod.NET.Commands.Configuration;

/// <summary>
/// Represents the command to disable a user's feature configuration.
/// </summary>
/// <param name="userConfigService">Service of user's configuration.</param>
/// <param name="arguments">Parameters and their values to modify.</param>
public readonly struct DisableConfigCommand(
    IUserConfigService userConfigService,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    // TODO: Duplicated code along with EnableConfigCommand
    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!arguments.TryGetValue("feature", out string? feature)
            || string.IsNullOrWhiteSpace(feature))
            return new OperationResult(Success: false, "Feature was not provided.");

        return await userConfigService.DisableFeatureAsync(feature);
    }
}
