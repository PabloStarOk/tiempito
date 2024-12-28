namespace Tiempitod.NET.Configuration.User;

public class UserConfigurationProvider : DaemonService, IUserConfigurationProvider
{
    private readonly IUserConfigurationReader _userConfigurationReader;
    
    public UserConfiguration UserConfiguration { get; private set; }

    public UserConfigurationProvider(
        ILogger<UserConfigurationProvider> logger,
        IUserConfigurationReader userConfigurationReader) : base(logger)
    {
        _userConfigurationReader = userConfigurationReader;
    }

    protected override void OnStartService()
    {
        UserConfiguration = _userConfigurationReader.Read();
    }
}
