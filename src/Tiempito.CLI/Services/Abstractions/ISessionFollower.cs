namespace Tiempito.CLI.Services.Abstractions;

/// <summary>
/// Defines a contract for following a session.
/// </summary>
public interface ISessionFollower
{
    /// <summary>
    /// Sets a value indicating whether the session must be followed.
    /// </summary>
    public bool MustFollow { set; }

    /// <summary>
    /// Follows the session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask FollowAsync(CancellationToken cancellationToken = default);
}