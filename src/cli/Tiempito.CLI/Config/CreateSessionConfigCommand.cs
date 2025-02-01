using System.CommandLine;

using Tiempito.CLI.Client;

namespace Tiempito.CLI.Config;

/// <summary>
/// Represents the command to create a new session configuration.
/// </summary>
public class CreateSessionConfigCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    
    /// <summary>
    /// Instantiates a <see cref="CreateSessionConfigCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">ID option of the new session configuration.</param>
    public CreateSessionConfigCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> sessionIdOption) 
        : base("create-session-config", "Creates a new session configuration.")
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        this.AddAlias("create-session");
        
        var targetCyclesOption = new Option<string>("--target-cycles", "Target cycles to complete.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true
        };
        targetCyclesOption.AddAlias("-t");
        
        var delayOption = new Option<string>("--delay-between-times", "Delay before starting a time after another has been completed.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = false
        };
        delayOption.AddAlias("-d");
        
        var focusDurationOption = new Option<string>("--focus-duration", "The duration of a focus time.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true
        };
        focusDurationOption.AddAlias("-f");
        
        var breakDurationOption = new Option<string>("--break-duration", "The duration of a break time.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true
        };
        breakDurationOption.AddAlias("-b");
        
        sessionIdOption.IsRequired = true;
        AddOption(sessionIdOption);
        AddOption(targetCyclesOption);
        AddOption(delayOption);
        AddOption(focusDurationOption);
        AddOption(breakDurationOption);
        this.SetHandler(CommandHandler, 
            sessionIdOption,
            targetCyclesOption, 
            delayOption,
            focusDurationOption,
            breakDurationOption);
    }
    
    /// <summary>
    /// Sends the request to create a new configuration session.
    /// </summary>
    /// <param name="sessionConfigId">ID of the new configuration session.</param>
    /// <param name="targetCycles">Target cycles to complete.</param>
    /// <param name="delayBetweenTimes">Delay before starting a time after another has been completed.</param>
    /// <param name="focusDuration">Duration of the focus time.</param>
    /// <param name="breakDuration">Duration of the break time.</param>
    private async Task CommandHandler(
        string sessionConfigId, string targetCycles,
        string delayBetweenTimes, string focusDuration, 
        string breakDuration)
    {
        delayBetweenTimes = !string.IsNullOrWhiteSpace(delayBetweenTimes) ? delayBetweenTimes : "0s";
        
        var arguments = new Dictionary<string, string>
        {
            { "session-config-id", sessionConfigId },
            { "target-cycles", targetCycles },
            { "delay-times", delayBetweenTimes },
            { "focus-duration", focusDuration },
            { "break-duration", breakDuration },
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
}
