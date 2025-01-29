using Tiempitod.NET.Common;
using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Commands.Configuration;

/// <summary>
/// Represents the subcommand to modify parameters of the user's configuration.
/// </summary>
/// <param name="userConfigProvider">Provider of user's configuration.</param>
/// <param name="arguments">Parameters and their values to modify.</param>
public class SetConfigCommand(
    IUserConfigProvider userConfigProvider,
    IReadOnlyDictionary<string, string> arguments) : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!arguments.TryGetValue("default-session-id", out string? defaultSessionId)
            || string.IsNullOrWhiteSpace(defaultSessionId))
            return Task.FromResult(new OperationResult(Success: false, "Nothing to update."));

        UserConfig newUserConfig = userConfigProvider.UserConfig with
        {
            DefaultSessionId = defaultSessionId,
        };
        
        return Task.FromResult(userConfigProvider.SaveUserConfig(newUserConfig));
    }
}
