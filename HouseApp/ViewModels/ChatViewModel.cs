using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Models;
using HouseApp.Services;
using HouseApp.DTOs;
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

    [ObservableProperty]
    private string connectionStatus = "Connecting...";

    [ObservableProperty]
    private string houseName = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public ChatViewModel(ChatService chatService, AuthService authService, HouseService houseService)
    {
        _chatService = chatService;
        _authService = authService;
        _houseService = houseService;

        _chatService.MessageReceived += OnMessageReceived;
        _chatService.ConnectionStatusChanged += HandleConnectionStatusChanged;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await LoadUserHouse();
            
            if (CurrentHouseId > 0)
            {
                await _chatService.InitializeAsync(CurrentHouseId);
                IsConnected = _chatService.IsConnected;
            }
            else
            {
                ConnectionStatus = "Not assigned to a house";
                IsConnected = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Chat initialization error: {ex.Message}");
            ConnectionStatus = $"Error: {ex.Message}";
            IsConnected = false;
        }
    }

    private async Task LoadUserHouse()
    {
        try
        {
            var userId = int.Parse(await SecureStorage.GetAsync(Constants.UserIdKey) ?? "0");
            if (userId == 0) return;

            // Get user's house assignment using new endpoint
            var houseTenant = await _houseService.GetStudentHouseAsync(userId);
            
            if (houseTenant != null)
            {
                CurrentHouseId = houseTenant.Id;
                HouseName = houseTenant.Name;
            }
            else
            {
                ConnectionStatus = "Not assigned to a house";
                IsConnected = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading house: {ex.Message}");
        }
    }

    private void HandleConnectionStatusChanged(string status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ConnectionStatus = status;
            IsConnected = status == "Connected";
        });
    }

    private void OnMessageReceived(ChatMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Messages.Add(message);
        });
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(MessageText))
            return;

        if (!_chatService.IsConnected)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", 
                "Not connected to chat. Please wait...", 
                "OK");
            return;
        }

        if (CurrentHouseId <= 0)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", 
                "You are not assigned to a house yet.", 
                "OK");
            return;
        }

        string message = string.Empty;
        try
        {
            IsBusy = true;
            
            message = MessageText;
            MessageText = string.Empty; // Clear immediately
            
            await _chatService.SendMessageAsync(CurrentHouseId, message);
            
            // Message will be received via SignalR callback
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", 
                $"Failed to send message: {ex.Message}", 
                "OK");
            
            // Restore message on error
            MessageText = message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DisconnectAsync()
    {
        await _chatService.DisconnectAsync();
        _chatService.MessageReceived -= OnMessageReceived;
        _chatService.ConnectionStatusChanged -= HandleConnectionStatusChanged;
    }
}
