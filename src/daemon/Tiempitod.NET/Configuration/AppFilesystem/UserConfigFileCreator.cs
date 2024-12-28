using Microsoft.Extensions.FileProviders;

namespace Tiempitod.NET.Configuration.AppFilesystem;

/// <summary>
/// Creates the default user's configuration file.
/// </summary>
public class UserConfigFileCreator : DaemonService
{
    private readonly IFileProvider _userDirectoryFileProvider;
    
    /// <summary>
    /// Instantiates a new <see cref="UserConfigFileCreator"/>.
    /// </summary>
    /// <param name="logger">Logger to register special events.</param>
    /// <param name="userDirectoryFileProvider">A provider of files in the user's configuration directory.</param>
    public UserConfigFileCreator(ILogger<UserConfigFileCreator> logger,
        [FromKeyedServices(AppConfigConstants.UserConfigFileProviderKey)] IFileProvider userDirectoryFileProvider) : base(logger)
    {
        _userDirectoryFileProvider = userDirectoryFileProvider;
    }

    protected override void OnStartService()
    {
        CreateFile(AppConfigConstants.UserConfigFileName);
    }
    
    /// <summary>
    /// Creates a file in the user's config directory with the given name.
    /// </summary>
    /// <param name="fileName">Name of the file to create.</param>
    private void CreateFile(string fileName)
    {
        IFileInfo fileInfo = _userDirectoryFileProvider.GetFileInfo(fileName);
        
        if (fileInfo.Exists || string.IsNullOrWhiteSpace(fileInfo.PhysicalPath))
            return;
        
        File.Create(fileInfo.PhysicalPath).Dispose();
        Logger.LogInformation("User config filed was created at {Path}", fileInfo.PhysicalPath);
    }
}
