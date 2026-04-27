using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;

namespace Tiempito.Daemon.Application.Notifications;

/// <summary>
/// Defines a service for sending notifications related to session state changes.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification asynchronously based on the provided session state and notification type.
    /// </summary>
    /// <param name="sessionState">The current state of the session.</param>
    /// <param name="type">The type of notification to send.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask NotifyAsync(SessionState sessionState, NotificationType type);
}
