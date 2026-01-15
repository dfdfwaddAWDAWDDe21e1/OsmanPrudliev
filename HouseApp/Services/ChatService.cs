using Microsoft.AspNetCore.SignalR.Client;
using HouseApp.DTOs;
using HouseApp.Models;

namespace HouseApp.Services;

public class ChatService
{
    private HubConnection? _hubConnection;
    private readonly AuthService _authService;
    private bool _isConnecting;

    public event Action<ChatMessage>? MessageReceived;
    public event Action<string>? ConnectionStatusChanged;
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public ChatService(AuthService authService)
    {
        _authService = authService;
    }

    public async Task InitializeAsync(int houseId)
    {
        if (_isConnecting || IsConnected)
            return;

        _isConnecting = true;

        try
        {
            var token = await SecureStorage.GetAsync(Constants.TokenKey);
            if (string.IsNullOrEmpty(token))
            {
                ConnectionStatusChanged?.Invoke("Error: No authentication token");
                return;
            }

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

            _hubConnection.Reconnecting += (error) =>
            {
                ConnectionStatusChanged?.Invoke("Reconnecting...");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                ConnectionStatusChanged?.Invoke("Connected");
                return Task.CompletedTask;
            };

            _hubConnection.Closed += (error) =>
            {
                ConnectionStatusChanged?.Invoke("Disconnected");
                return Task.CompletedTask;
            };

            await _hubConnection.StartAsync();
            ConnectionStatusChanged?.Invoke("Connected");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR connection error: {ex.Message}");
            ConnectionStatusChanged?.Invoke($"Error: {ex.Message}");
        }
        finally
        {
            _isConnecting = false;
        }
    }

    public async Task SendMessageAsync(int houseId, string message)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to chat server");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty");
        }

        try
        {
            await _hubConnection.InvokeAsync("SendMessage", houseId, message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending message: {ex.Message}");
            throw;
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
