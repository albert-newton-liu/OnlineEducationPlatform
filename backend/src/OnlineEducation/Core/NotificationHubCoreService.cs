using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OnlineEducation.Core;

[Authorize]
public class NotificationHub : Hub
{
    protected async Task SendNotification(string userId, string message)
    {
        // Hubs automatically have a "Clients" property
        // The "User" property allows you to send a message to a specific user.
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

}

