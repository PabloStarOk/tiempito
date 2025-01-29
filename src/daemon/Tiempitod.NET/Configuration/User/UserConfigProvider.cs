using Tiempitod.NET.Common;

namespace Tiempitod.NET.Configuration.User;

public class UserConfigProvider : Service, IUserConfigProvider
{
    private readonly IUserConfigReader _userConfigReader;
    private readonly IUserConfigWriter _userConfigWriter;

    public event EventHandler? OnUserConfigChanged;
    public UserConfig UserConfig { get; private set; }

    /// <summary>
    /// Instantiates a new <see cref="UserConfigProvider"/>
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <param name="userConfigReader">Reader for user's configuration.</param>
    /// <param name="userConfigWriter">Writer for user's configuration.</param>
    public UserConfigProvider(
        ILogger<UserConfigProvider> logger,
        IUserConfigReader userConfigReader,
        IUserConfigWriter userConfigWriter) : base(logger)
    {
        _userConfigReader = userConfigReader;
        _userConfigWriter = userConfigWriter;
    }

    protected override Task<bool> OnStartServiceAsync()
    {
        UserConfig = _userConfigReader.Read();
        return Task.FromResult(true);
    }

    protected override Task<bool> OnStopServiceAsync()
    {
        return Task.FromResult(true);
    }

    public OperationResult SaveUserConfig(UserConfig userConfig)
    {
        if (string.IsNullOrWhiteSpace(userConfig.DefaultSessionId))
            return new OperationResult
            (
                Success: false,
                Message: "The user configuration contains invalid data."
            );

        bool wasWritten = _userConfigWriter.Write(userConfig);
        
        string responseMsg;
        
        if (wasWritten)
        {
            // Save new user config
            UserConfig = userConfig;
            
            // Notify default session changed
            OnUserConfigChanged?.Invoke(this, EventArgs.Empty);
            
            responseMsg = "The user configuration was saved.";
        }
        else
            responseMsg = "User configuration was not saved, try again.";

        return new OperationResult(wasWritten, responseMsg);
    }
}
