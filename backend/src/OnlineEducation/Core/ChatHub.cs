
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace OnlineEducation.Core;

[Authorize]
public class ChatHub : Hub
{
    /// <summary>
    /// Sends a private message from one authenticated user to another.
    /// The sender's ID is automatically retrieved from the JWT token.
    /// </summary>
    /// <param name="recipientId">The ID of the user to receive the message.</param>
    /// <param name="message">The content of the message.</param>
    public async Task SendPrivateMessage(string recipientId, string message)
    {
        // Get the sender's unique user ID from the authenticated connection context.
        var senderId = Context.UserIdentifier;
        var senderUsername = Context.User?.Identity?.Name;

        Console.WriteLine($"SendPrivateMessage recipientId: {recipientId}, message: {message}");

        if (string.IsNullOrEmpty(recipientId) || senderId == recipientId)
        {
            // Do not send if recipient is invalid or it's a self-message
            return;
        }

        // Send the message to the recipient by their User ID.
        await Clients.User(recipientId).SendAsync("ReceiveMessage", senderId, senderUsername, message);

        // Also send the message back to the sender so they see it on their screen.
        await Clients.Caller.SendAsync("ReceiveMessage", senderId, senderUsername, message);

    }
}