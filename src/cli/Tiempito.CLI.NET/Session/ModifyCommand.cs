using System.CommandLine;
using Tiempito.CLI.NET.Client;
using Tiempito.IPC.NET.Messages;

namespace Tiempito.CLI.NET.Session;

public class ModifyCommand : Command
{
    private readonly IClient _client;
    
    public ModifyCommand(IClient client, Option<string> sessionIdOption) 
        : base("modify", "Modifies an existing session configuration.")
    {
        _client = client;
        
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
    
    // TODO: Replace Console with IConsole from DI.
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
        // TODO: Replace hardcoded command and subcommand.
        await _client.SendRequestAsync(new Request(CommandType: "session", SubcommandType: "modify", arguments));
        Response response = await _client.ReceiveResponseAsync();
        Console.WriteLine(response.Message);
    }
}
