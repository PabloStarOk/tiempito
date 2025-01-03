using System.CommandLine;
using Tiempito.CLI.NET.Client;
using Tiempito.IPC.NET.Messages;

namespace Tiempito.CLI.NET.Session;

/// <summary>
/// Creates a new session configuration.
/// </summary>
public class CreateCommand : Command
{
    private readonly IClient _client;
    
    public CreateCommand(IClient client, Option<string> sessionIdOption) 
        : base("create", "Creates a new session configuration.")
    {
        _client = client;
        
        var targetCyclesOption = new Option<string>("--target-cycles", "Target cycles to complete.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true
        };
        targetCyclesOption.AddAlias("-t");
        
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
    /// Sends the request to create a new configuration session.
    /// </summary>
    /// <param name="sessionId">ID of the new configuration session.</param>
    /// <param name="targetCycles">Target cycles to complete.</param>
    /// <param name="focusDuration">Duration of the focus time.</param>
    /// <param name="breakDuration">Duration of the break time.</param>
    private async Task CommandHandler(string sessionId, string targetCycles, string focusDuration, string breakDuration)
    {
        var arguments = new Dictionary<string, string>
        {
            { "session-id", sessionId },
            { "target-cycles", targetCycles },
            { "focus-duration", focusDuration },
            { "break-duration", breakDuration },
        };
        await _client.SendRequestAsync(new Request(CommandType: "session", SubcommandType: "create", arguments));
        Response response = await _client.ReceiveResponseAsync();
        Console.WriteLine(response.Message);
    }
}
