using Microsoft.AspNetCore.SignalR.Client;
using HouseApp.DTOs;
using HouseApp.Models;

namespace HouseApp.Services;

public class ChatService
{
    private HubConnection? _hubConnection;
    private readonly AuthService _authService;

    public event Action<ChatMessage>? MessageReceived;
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public ChatService(AuthService authService)
    {
        _authService = authService;
    }

    public async Task InitializeAsync(int houseId)
    {
        try
        {
            var token = await SecureStorage.GetAsync(Constants.TokenKey);
            if (string.IsNullOrEmpty(token))
                return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{Constants.ApiBaseUrl}/chathub?houseId={houseId}", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<MessageDto>("ReceiveMessage", async (messageDto) =>
            {
                var currentUserId = await _authService.GetCurrentUserIdAsync();
                var message = new ChatMessage
                {
                    Id = messageDto.Id ?? 0,
                    HouseId = messageDto.HouseId,
                    SenderId = messageDto.SenderId,
                    SenderName = messageDto.SenderName,
                    MessageText = messageDto.MessageText,
                    Timestamp = messageDto.Timestamp ?? DateTime.Now,
                    IsRead = messageDto.IsRead,
                    IsCurrentUser = messageDto.SenderId == currentUserId
                };
                MessageReceived?.Invoke(message);
            });

            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR connection error: {ex.Message}");
        }
    }

    public async Task SendMessageAsync(int houseId, string message)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.InvokeAsync("SendMessage", houseId, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send message error: {ex.Message}");
            }
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }
}
