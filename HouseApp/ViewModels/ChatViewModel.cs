using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Models;
using HouseApp.Services;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly ChatService _chatService;
    private readonly AuthService _authService;
    private readonly HouseService _houseService;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> messages = new();

    [ObservableProperty]
    private string messageText = string.Empty;

    [ObservableProperty]
    private int currentHouseId;

    [ObservableProperty]
    private bool isConnected;

    public ChatViewModel(ChatService chatService, AuthService authService, HouseService houseService)
    {
        _chatService = chatService;
        _authService = authService;
        _houseService = houseService;

        _chatService.MessageReceived += OnMessageReceived;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var studentId = await _authService.GetCurrentUserIdAsync();
            var house = await _houseService.GetStudentHouseAsync(studentId);
            
            if (house != null)
            {
                CurrentHouseId = house.Id;
                await _chatService.InitializeAsync(CurrentHouseId);
                IsConnected = _chatService.IsConnected;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Chat initialization error: {ex.Message}");
        }
    }

    private void OnMessageReceived(ChatMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Messages.Add(message);
        });
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText)) return;

        try
        {
            await _chatService.SendMessageAsync(CurrentHouseId, MessageText);
            MessageText = string.Empty;
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to send message: {ex.Message}", "OK");
        }
    }

    public async Task DisconnectAsync()
    {
        await _chatService.DisconnectAsync();
        _chatService.MessageReceived -= OnMessageReceived;
    }
}
