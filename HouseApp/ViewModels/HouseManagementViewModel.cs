using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Models;
using HouseApp.Services;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class HouseManagementViewModel : ObservableObject, IQueryAttributable
{
    private readonly HouseService _houseService;
    private readonly AuthService _authService;
    private readonly PaymentService _paymentService;

    [ObservableProperty]
    private int houseId;

    [ObservableProperty]
    private string houseName = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private decimal monthlyRent;

    [ObservableProperty]
    private decimal utilitiesCost;

    [ObservableProperty]
    private decimal waterBillCost;

    [ObservableProperty]
    private int maxOccupants = 1;

    [ObservableProperty]
    private ObservableCollection<HouseTenant> tenants = new();

    [ObservableProperty]
    private ObservableCollection<Payment> payments = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEditMode;

    public HouseManagementViewModel(HouseService houseService, AuthService authService, PaymentService paymentService)
    {
        _houseService = houseService;
        _authService = authService;
        _paymentService = paymentService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("houseId"))
        {
            HouseId = int.Parse(query["houseId"].ToString()!);
            IsEditMode = true;
            _ = LoadHouseAsync();
        }
    }

    private async Task LoadHouseAsync()
    {
        IsLoading = true;

        try
        {
            var house = await _houseService.GetHouseByIdAsync(HouseId);
            if (house != null)
            {
                HouseName = house.Name;
                Address = house.Address;
                MonthlyRent = house.MonthlyRent;
                UtilitiesCost = house.UtilitiesCost;
                WaterBillCost = house.WaterBillCost;
                MaxOccupants = house.MaxOccupants;

                await LoadTenantsAsync();
                await LoadPaymentsAsync();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadTenantsAsync()
    {
        if (HouseId == 0) return;

        var tenantsList = await _houseService.GetHouseTenantsAsync(HouseId);
        Tenants.Clear();
        foreach (var tenant in tenantsList)
        {
            Tenants.Add(tenant);
        }
    }

    [RelayCommand]
    private async Task LoadPaymentsAsync()
    {
        if (HouseId == 0) return;

        var paymentsList = await _paymentService.GetHousePaymentsAsync(HouseId);
        Payments.Clear();
        foreach (var payment in paymentsList)
        {
            Payments.Add(payment);
        }
    }

    [RelayCommand]
    private async Task SaveHouseAsync()
    {
        if (string.IsNullOrWhiteSpace(HouseName) || string.IsNullOrWhiteSpace(Address))
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Please fill all required fields", "OK");
            return;
        }

        IsLoading = true;

        try
        {
            var landlordId = await _authService.GetCurrentUserIdAsync();
            var house = new House
            {
                Id = HouseId,
                Name = HouseName,
                Address = Address,
                MonthlyRent = MonthlyRent,
                UtilitiesCost = UtilitiesCost,
                WaterBillCost = WaterBillCost,
                MaxOccupants = MaxOccupants,
                LandlordId = landlordId
            };

            bool success;
            if (IsEditMode)
            {
                success = await _houseService.UpdateHouseAsync(HouseId, house);
            }
            else
            {
                success = await _houseService.CreateHouseAsync(house);
            }

            if (success)
            {
                await Application.Current!.MainPage!.DisplayAlert("Success", "House saved successfully", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Failed to save house", "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteHouseAsync()
    {
        if (HouseId == 0) return;

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm", "Are you sure you want to delete this house?", "Yes", "No");

        if (!confirm) return;

        IsLoading = true;

        try
        {
            var success = await _houseService.DeleteHouseAsync(HouseId);
            if (success)
            {
                await Application.Current!.MainPage!.DisplayAlert("Success", "House deleted", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Failed to delete house", "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
