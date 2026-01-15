using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Models;
using HouseApp.Services;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class LandlordDashboardViewModel : ObservableObject
{
    private readonly HouseService _houseService;
    private readonly AuthService _authService;
    private readonly PaymentService _paymentService;

    [ObservableProperty]
    private ObservableCollection<House> houses = new();

    [ObservableProperty]
    private decimal totalMonthlyIncome;

    [ObservableProperty]
    private int totalProperties;

    [ObservableProperty]
    private int totalTenants;

    [ObservableProperty]
    private bool isLoading;

    public LandlordDashboardViewModel(HouseService houseService, AuthService authService, PaymentService paymentService)
    {
        _houseService = houseService;
        _authService = authService;
        _paymentService = paymentService;
    }

    public async Task InitializeAsync()
    {
        await LoadHousesAsync();
    }

    [RelayCommand]
    private async Task LoadHousesAsync()
    {
        IsLoading = true;

        try
        {
            var landlordId = await _authService.GetCurrentUserIdAsync();
            var housesList = await _houseService.GetLandlordHousesAsync(landlordId);

            Houses.Clear();
            foreach (var house in housesList)
            {
                Houses.Add(house);
            }

            CalculateStats();
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load houses: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateStats()
    {
        TotalProperties = Houses.Count;
        TotalTenants = Houses.Sum(h => h.CurrentOccupants);
        TotalMonthlyIncome = Houses.Sum(h => h.MonthlyRent * h.CurrentOccupants);
    }

    [RelayCommand]
    private async Task NavigateToHouseManagement(House house)
    {
        await Shell.Current.GoToAsync($"housemanagement?houseId={house.Id}");
    }

    [RelayCommand]
    private async Task AddHouseAsync()
    {
        await Shell.Current.GoToAsync("housemanagement");
    }
}
