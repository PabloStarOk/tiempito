using Microsoft.Extensions.FileProviders;

using Tiempitod.NET.Common;

namespace Tiempitod.NET.Configuration.AppFilesystem;

/// <summary>
/// Creates the default user's configuration file.
/// </summary>
public class UserConfigFileCreator : Service
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

    protected override Task<bool> OnStartServiceAsync()
    {
        CreateFile(AppConfigConstants.UserConfigFileName);
        return Task.FromResult(true);
    }

    protected override Task<bool> OnStopServiceAsync()
    {
        return Task.FromResult(true);
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
        _logger.LogInformation("User config filed was created at {Path}", fileInfo.PhysicalPath);
    }
}
