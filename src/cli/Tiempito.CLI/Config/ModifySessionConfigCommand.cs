using System.CommandLine;

using Tiempito.CLI.Client;

namespace Tiempito.CLI.Config;

/// <summary>
/// Represents the command to modify an existing session configuration.
/// </summary>
public class ModifySessionConfigCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    
    /// <summary>
    /// Instantiates a <see cref="ModifySessionConfigCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    public ModifySessionConfigCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> sessionIdOption) 
        : base("modify-session-config", "Modifies an existing session configuration.")
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        this.AddAlias("modify-session");
        
        var targetCyclesOption = new Option<string>("--target-cycles", "Target cycles to complete.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = false
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
            IsRequired = false
        };
        focusDurationOption.AddAlias("-f");
        
        var breakDurationOption = new Option<string>("--break-duration", "The duration of a break time.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = false
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
    /// Sends the request to modify a configuration session.
    /// </summary>
    /// <param name="sessionConfigId">ID of the configuration session to modify.</param>
    /// <param name="targetCycles">New target cycles to complete.</param>
    /// <param name="delayBetweenTimes">Delay before starting a time after another has been completed.</param>
    /// <param name="focusDuration">New duration of the focus time.</param>
    /// <param name="breakDuration">New duration of the break time.</param>
    private async Task CommandHandler(
        string sessionConfigId, string targetCycles,
        string delayBetweenTimes, string focusDuration,
        string breakDuration)
    {
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
