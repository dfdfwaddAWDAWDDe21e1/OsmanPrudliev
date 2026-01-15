using Microsoft.AspNetCore.SignalR;
using HouseApp.API.Data;
using HouseApp.API.Models;
using HouseApp.API.DTOs;

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

    public async Task SendMessage(MessageDto messageDto)
    {
        var message = new Message
        {
            HouseId = messageDto.HouseId,
            SenderId = messageDto.SenderId,
            SenderName = messageDto.SenderName,
            MessageText = messageDto.MessageText,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        messageDto.Id = message.Id;
        messageDto.Timestamp = message.Timestamp;

        await Clients.Group($"House_{messageDto.HouseId}").SendAsync("ReceiveMessage", messageDto);
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
