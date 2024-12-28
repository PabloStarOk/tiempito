namespace Tiempitod.NET.Configuration.User;

public class UserConfigurationProvider : DaemonService, IUserConfigurationProvider
{
    private readonly IUserConfigurationReader _userConfigurationReader;
    private readonly IUserConfigurationWriter _userConfigurationWriter;
    
    public UserConfiguration UserConfiguration { get; private set; }

    /// <summary>
    /// Instantiates a new <see cref="UserConfigurationProvider"/>
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <param name="userConfigurationReader">Reader for user's configuration.</param>
    /// <param name="userConfigurationWriter">Writer for user's configuration.</param>
    public UserConfigurationProvider(
        ILogger<UserConfigurationProvider> logger,
        IUserConfigurationReader userConfigurationReader,
        IUserConfigurationWriter userConfigurationWriter) : base(logger)
    {
        _userConfigurationReader = userConfigurationReader;
        _userConfigurationWriter = userConfigurationWriter;
    }

    protected override void OnStartService()
    {
        UserConfiguration = _userConfigurationReader.Read();
    }

    public OperationResult SaveUserConfig(UserConfiguration userConfig)
    {
        if (string.IsNullOrWhiteSpace(userConfig.DefaultSession))
            return new OperationResult
            (
                Success: false,
                Message: "The user configuration contains invalid data."
            );

        bool wasWritten = _userConfigurationWriter.Write(userConfig);

        return new OperationResult
        (
            wasWritten,
            wasWritten ? "The user configuration was saved." : "User configuration was not saved, try again."
        );
    }
}
