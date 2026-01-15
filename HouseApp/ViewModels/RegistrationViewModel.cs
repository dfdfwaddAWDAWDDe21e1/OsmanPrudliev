using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.DTOs;
using HouseApp.Models;
using HouseApp.Services;

namespace HouseApp.ViewModels;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly UserSession _userSession;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private DateTime dateOfBirth = DateTime.Now.AddYears(-20);

    [ObservableProperty]
    private UserType selectedUserType = UserType.Student;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isLoginMode = true;

    public RegistrationViewModel(AuthService authService, UserSession userSession)
    {
        _authService = authService;
        _userSession = userSession;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Please fill all fields", "OK");
            return;
        }

        IsLoading = true;

        try
        {
            var loginDto = new LoginDto { Email = Email, Password = Password };
            var result = await _authService.LoginAsync(loginDto);

            if (result.Success && result.Data != null)
            {
                _userSession.UserId = result.Data.UserId;
                _userSession.Email = result.Data.Email;
                _userSession.UserType = result.Data.UserType;
                _userSession.FirstName = result.Data.FirstName;
                _userSession.LastName = result.Data.LastName;

                // Store user type in SecureStorage
                await SecureStorage.SetAsync(Constants.UserTypeKey, result.Data.UserType.ToString());

                // Navigate based on user type
                if (result.Data.UserType == UserType.Landlord)
                {
                    await Shell.Current.GoToAsync("///landlorddashboard");
                }
                else // Student
                {
                    await Shell.Current.GoToAsync("///tabs/home");
                }
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", result.Message, "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(PhoneNumber))
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Please fill all fields", "OK");
            return;
        }

        IsLoading = true;

        try
        {
            var registerDto = new RegisterDto
            {
                Email = Email,
                Password = Password,
                FirstName = FirstName,
                LastName = LastName,
                PhoneNumber = PhoneNumber,
                DateOfBirth = DateOfBirth,
                UserType = SelectedUserType
            };

            var result = await _authService.RegisterAsync(registerDto);

            if (result.Success && result.Data != null)
            {
                _userSession.UserId = result.Data.UserId;
                _userSession.Email = result.Data.Email;
                _userSession.UserType = result.Data.UserType;
                _userSession.FirstName = result.Data.FirstName;
                _userSession.LastName = result.Data.LastName;

                // Store user type in SecureStorage
                await SecureStorage.SetAsync(Constants.UserTypeKey, result.Data.UserType.ToString());

                // Navigate based on user type
                if (result.Data.UserType == UserType.Landlord)
                {
                    await Shell.Current.GoToAsync("///landlorddashboard");
                }
                else // Student
                {
                    await Shell.Current.GoToAsync("///tabs/home");
                }
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", result.Message, "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsLoginMode = !IsLoginMode;
    }
}
