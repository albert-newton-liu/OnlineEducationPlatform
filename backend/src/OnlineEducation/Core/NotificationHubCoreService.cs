using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OnlineEducation.Core;

/// <summary>
/// SignalR hub for sending real-time notifications to users in the Online Education Platform.
/// Requires authorization for all connections.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    /// <summary>
    /// Sends a notification message to a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to receive the notification.</param>
    /// <param name="message">The notification message to send.</param>
    protected async Task SendNotification(string userId, string message)
    {
        // Hubs automatically have a "Clients" property
        // The "User" property allows you to send a message to a specific user.
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }
}

