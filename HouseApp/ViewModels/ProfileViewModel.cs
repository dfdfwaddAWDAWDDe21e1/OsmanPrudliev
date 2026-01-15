using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Services;

namespace HouseApp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly UserSession _userSession;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string profilePictureUrl = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public ProfileViewModel(AuthService authService, UserSession userSession)
    {
        _authService = authService;
        _userSession = userSession;
        LoadUserData();
    }

    private void LoadUserData()
    {
        FirstName = _userSession.FirstName;
        LastName = _userSession.LastName;
        Email = _userSession.Email;
    }

    [RelayCommand]
    private async Task PickProfilePictureAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick a profile picture"
            });

            if (result != null)
            {
                ProfilePictureUrl = result.FullPath;
                await Application.Current!.MainPage!.DisplayAlert("Info", "Profile picture updated (UI only)", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm", "Are you sure you want to logout?", "Yes", "No");

        if (!confirm) return;

        await _authService.LogoutAsync();
        _userSession.Clear();
        await Shell.Current.GoToAsync("//login");
    }
}
