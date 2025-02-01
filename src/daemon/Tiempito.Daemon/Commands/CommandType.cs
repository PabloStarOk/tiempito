namespace Tiempito.Daemon.Commands;

/// <summary>
/// Command types the daemon handles.
/// </summary>
public enum CommandType
{
    /// <summary>
    /// Command to manage sessions.
    /// </summary>
    Session,
    
    /// <summary>
    /// Command to manage user and session configurations.
    /// </summary>
    Config,
}