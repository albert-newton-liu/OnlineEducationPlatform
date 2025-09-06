using Microsoft.AspNetCore.SignalR;

namespace OnlineEducation.Core;

public class NotificationHub : Hub
{
    public async Task SendNotification(string userId, string message)
    {
        // Hubs automatically have a "Clients" property
        // The "User" property allows you to send a message to a specific user.
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

}

