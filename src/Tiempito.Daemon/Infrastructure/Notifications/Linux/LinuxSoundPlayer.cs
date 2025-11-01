#if LINUX
using System.Diagnostics;

namespace Tiempito.Daemon.Infrastructure.Notifications.Linux;

/// <summary>
/// A sound player for linux operating systems.
/// </summary>
internal sealed class LinuxSoundPlayer : ILinuxSoundPlayer, IDisposable
{
    private const string RequiredEnvVariable = "XDG_RUNTIME_DIR";
    private const string PreferredAudioSystem = "pw-play";
    
    private readonly ILogger<LinuxSoundPlayer> _logger;
    private readonly bool _isRequiredEnvVariableDefined;
    private string _audioSystem = string.Empty;
    private bool _audioSystemLoaded;
    
    private Process? _currentProcess;
    
    /// <summary>
    /// Instantiates a <see cref="LinuxSoundPlayer"/>.
    /// </summary>
    /// <param name="logger">Logger to register errors related to linux audio.</param>
    public LinuxSoundPlayer(ILogger<LinuxSoundPlayer> logger)
    {
        _logger = logger;
        
        _isRequiredEnvVariableDefined = Environment.GetEnvironmentVariables().Contains(RequiredEnvVariable);
        if (!_isRequiredEnvVariableDefined)
            _logger.LogError("Required \"{RequiredEnvVariable}\" environment variable for Linux is not defined, notifications won't have sound.", RequiredEnvVariable);
    }
    
    public async ValueTask PlayAsync(string filepath)
    {
        if (!_isRequiredEnvVariableDefined)
            return;
        
        try
        {
            if (!_audioSystemLoaded)
            {
                _audioSystem = await GetAudioSystemAsync();
                _audioSystemLoaded = true;
            }

            if (string.IsNullOrWhiteSpace(_audioSystem))
                return;

            _currentProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _audioSystem,
                    Arguments = filepath,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false,
                },
                EnableRaisingEvents = true
            };

            _currentProcess.ErrorDataReceived += OnErrorReceived;
            _currentProcess.Exited += async (_, _) => await StopAsync();
            _currentProcess.Start();
            _logger.LogDebug("Notification sound played.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't play notification sound.");
            await StopAsync();
        }
    }

    public ValueTask StopAsync()
    {
        if (_currentProcess == null)
            return ValueTask.CompletedTask;
        
        try
        {
            _currentProcess.Close();
            _currentProcess.Dispose();
            _currentProcess = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't stop notification sound.");
        }
        
        return ValueTask.CompletedTask;
    }
    
    public void Dispose()
    {
        _currentProcess?.Close();
        _currentProcess?.Dispose();
        _currentProcess = null;
    }
    
    /// <summary>
    /// Gets the current available audio system binary to play the sound.
    /// Only PipeWire, PulseAudio and ALSA are supported.
    /// </summary>
    /// <returns>A string representing the path of the audio system binary, preference for PipeWire.</returns>
    private async Task<string> GetAudioSystemAsync()
    {
        string stdOut;
        
        // Execute command to find the available
        using (var tempProcess = new Process())
        {
            tempProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"which pw-play && which paplay && which aplay\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false,
            };
            tempProcess.Start();
            tempProcess.ErrorDataReceived += OnErrorReceived;
            await tempProcess.WaitForExitAsync();
            stdOut = await tempProcess.StandardOutput.ReadToEndAsync();
        }

        string[] audioSystemsBinaries = stdOut.Split(Environment.NewLine)
            .Where(str => !string.IsNullOrWhiteSpace(str)).ToArray();
        
        if (audioSystemsBinaries.Length < 1)
            return string.Empty;
        
        string? pipeWireBin = Array.Find(audioSystemsBinaries, 
            str => str.Contains(PreferredAudioSystem, StringComparison.InvariantCultureIgnoreCase));
        return string.IsNullOrWhiteSpace(pipeWireBin) 
            ? audioSystemsBinaries[0] 
            : pipeWireBin;
    }
    
    /// <summary>
    /// Logs error events received in the redirected stderr of the processes.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="eventArgs">Error data arguments.</param>
    private void OnErrorReceived(object sender, DataReceivedEventArgs eventArgs)
    {
        if (!string.IsNullOrWhiteSpace(eventArgs.Data))
            _logger.LogError("Error while trying to play a notification sound. Sender: {Sender} | ExitCode: {ExitCode} | Error: {Err}", sender, _currentProcess?.ExitCode, eventArgs.Data);
    }
}
#endif
