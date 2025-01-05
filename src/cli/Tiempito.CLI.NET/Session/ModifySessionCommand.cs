using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Session;

/// <summary>
/// Represents the command to modify an existing session configuration.
/// </summary>
public class ModifySessionCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    
    /// <summary>
    /// Instantiates a <see cref="ModifySessionCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    public ModifySessionCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> sessionIdOption) 
        : base("modify", "Modifies an existing session configuration.")
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        
        var targetCyclesOption = new Option<string>("--target-cycles", "Target cycles to complete.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = false
        };
        targetCyclesOption.AddAlias("-t");
        
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
        AddOption(focusDurationOption);
        AddOption(breakDurationOption);
        this.SetHandler(CommandHandler, 
            sessionIdOption,
            targetCyclesOption, 
            focusDurationOption,
            breakDurationOption);
    }
    
    /// <summary>
    /// Sends the request to modify a configuration session.
    /// </summary>
    /// <param name="sessionId">ID of the configuration session to modify.</param>
    /// <param name="targetCycles">New target cycles to complete.</param>
    /// <param name="focusDuration">New duration of the focus time.</param>
    /// <param name="breakDuration">New duration of the break time.</param>
    private async Task CommandHandler(string sessionId, string targetCycles, string focusDuration, string breakDuration)
    {
        var arguments = new Dictionary<string, string>
        {
            { "session-id", sessionId },
            { "target-cycles", targetCycles },
            { "focus-duration", focusDuration },
            { "break-duration", breakDuration },
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
}
