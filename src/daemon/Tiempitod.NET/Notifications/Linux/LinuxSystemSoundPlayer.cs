#if LINUX
using System.Diagnostics;

namespace Tiempitod.NET.Notifications.Linux;

/// <summary>
/// A sound player for linux operating systems.
/// </summary>
public class LinuxSystemSoundPlayer : ISystemSoundPlayer
{
    private readonly ILogger<LinuxSystemSoundPlayer> _logger;
    private const string PreferredAudioSystem = "pw-play";
    
    private Process? _currentProcess;
    
    /// <summary>
    /// Instantiates a <see cref="LinuxSystemSoundPlayer"/>.
    /// </summary>
    /// <param name="logger">Logger to register errors related to linux audio.</param>
    public LinuxSystemSoundPlayer(ILogger<LinuxSystemSoundPlayer> logger)
    {
        _logger = logger;
        GetAudioSystemAsync().Wait();
    }
    
    public async Task PlayAsync(string filepath)
    {
        string systemAudioPlayer = await GetAudioSystemAsync();
        
        if (string.IsNullOrWhiteSpace(systemAudioPlayer))
            return;
        
        _currentProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = systemAudioPlayer,
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
    }

    public Task StopAsync()
    {
        if (_currentProcess == null)
            return Task.CompletedTask;
        
        _currentProcess.Close();
        _currentProcess.Dispose();
        _currentProcess = null;
        return Task.CompletedTask;
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
