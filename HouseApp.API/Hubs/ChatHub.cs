using Microsoft.AspNetCore.SignalR;
using HouseApp.API.Data;
using HouseApp.API.Models;
using HouseApp.API.DTOs;
using System.Security.Claims;

namespace HouseApp.API.Hubs;

public class ChatHub : Hub
{
    private readonly AppDbContext _context;

    public ChatHub(AppDbContext context)
    {
        _context = context;
    }

    public async Task JoinHouse(string houseId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"House_{houseId}");
    }

    public async Task LeaveHouse(string houseId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"House_{houseId}");
    }

    public async Task SendMessage(int houseId, string messageText)
    {
        var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userName = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        var message = new Message
        {
            HouseId = houseId,
            SenderId = userId,
            SenderName = userName,
            MessageText = messageText,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Broadcast to all clients in the house chat
        await Clients.Group($"House_{houseId}").SendAsync("ReceiveMessage", new MessageDto
        {
            Id = message.Id,
            HouseId = message.HouseId,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            MessageText = message.MessageText,
            Timestamp = message.Timestamp,
            IsRead = message.IsRead
        });
    }

    public async Task MarkMessageAsRead(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}
