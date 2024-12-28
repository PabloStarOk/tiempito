namespace Tiempitod.NET.Configuration.User;

public class UserConfigProvider : DaemonService, IUserConfigProvider
{
    private readonly IUserConfigReader _userConfigReader;
    private readonly IUserConfigWriter _userConfigWriter;
    
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

    protected override void OnStartService()
    {
        UserConfig = _userConfigReader.Read();
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

        return new OperationResult
        (
            wasWritten,
            wasWritten ? "The user configuration was saved." : "User configuration was not saved, try again."
        );
    }
}
