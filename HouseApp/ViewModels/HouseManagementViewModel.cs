using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Models;
using HouseApp.Services;
using HouseApp.DTOs;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class HouseManagementViewModel : ObservableObject
{
    private readonly ApiService _apiService;

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
    private string? housePassword;

    [ObservableProperty]
    private string? houseCode;

    [ObservableProperty]
    private ObservableCollection<TenantDto> tenants = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string studentEmail = string.Empty;

    public HouseManagementViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadHouseDetails()
    {
        if (HouseId <= 0) return;

        try
        {
            IsLoading = true;

            var house = await _apiService.GetAsync<HouseDto>($"/api/houses/{HouseId}");
            
            if (house != null)
            {
                HouseName = house.Name;
                Address = house.Address;
                MonthlyRent = house.MonthlyRent;
                UtilitiesCost = house.UtilitiesCost;
                WaterBillCost = house.WaterBillCost;
                MaxOccupants = house.MaxOccupants;
                HousePassword = house.Password;
                HouseCode = house.HouseCode;
            }

            await LoadTenants();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load house: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadTenants()
    {
        try
        {
            var tenantsList = await _apiService.GetAsync<List<TenantDto>>($"/api/houses/{HouseId}/tenants");
            
            if (tenantsList != null)
            {
                Tenants = new ObservableCollection<TenantDto>(tenantsList);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading tenants: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveHouse()
    {
        if (string.IsNullOrWhiteSpace(HouseName) || string.IsNullOrWhiteSpace(Address))
        {
            await Shell.Current.DisplayAlert("Error", "Please fill all required fields", "OK");
            return;
        }

        try
        {
            IsLoading = true;

            if (IsEditMode)
            {
                var updateDto = new UpdateHouseDto
                {
                    Name = HouseName,
                    Address = Address,
                    MonthlyRent = MonthlyRent,
                    UtilitiesCost = UtilitiesCost,
                    WaterBillCost = WaterBillCost,
                    MaxOccupants = MaxOccupants,
                    Password = HousePassword
                };

                var response = await _apiService.PutAsync<HouseDto>($"/api/houses/{HouseId}", updateDto);

                if (response != null)
                {
                    await Shell.Current.DisplayAlert("Success", "House updated successfully", "OK");
                }
            }
            else
            {
                var landlordIdStr = await SecureStorage.GetAsync(Constants.UserIdKey);
                if (string.IsNullOrEmpty(landlordIdStr) || !int.TryParse(landlordIdStr, out int landlordId))
                {
                    await Shell.Current.DisplayAlert("Session Expired", "Please log in again to continue.", "OK");
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                var createDto = new HouseDto
                {
                    Name = HouseName,
                    Address = Address,
                    MonthlyRent = MonthlyRent,
                    UtilitiesCost = UtilitiesCost,
                    WaterBillCost = WaterBillCost,
                    MaxOccupants = MaxOccupants,
                    Password = HousePassword,
                    LandlordId = landlordId
                };

                var response = await _apiService.PostAsync<HouseDto, HouseDto>("/api/houses", createDto);

                if (response != null)
                {
                    await Shell.Current.DisplayAlert("Success", "House created successfully", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to save house: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddTenant()
    {
        if (string.IsNullOrWhiteSpace(StudentEmail))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter student email", "OK");
            return;
        }

        try
        {
            IsLoading = true;

            // Find student by email
            var student = await _apiService.GetAsync<UserDto>($"/api/auth/users/search?email={StudentEmail}");

            if (student == null || student.UserType != UserType.Student)
            {
                await Shell.Current.DisplayAlert("Error", "Student not found", "OK");
                return;
            }

            // Add tenant
            var success = await _apiService.PostAsync($"/api/houses/{HouseId}/tenants/{student.Id}", (object?)null);

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", $"Added {student.FirstName} {student.LastName} to house", "OK");
                StudentEmail = string.Empty;
                await LoadTenants();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to add tenant: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveTenant(TenantDto tenant)
    {
        if (tenant == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Confirm", 
            $"Remove {tenant.Name} from house?", 
            "Yes", 
            "No");

        if (!confirm) return;

        try
        {
            IsLoading = true;

            await _apiService.DeleteAsync($"/api/houses/{HouseId}/tenants/{tenant.UserId}");
            
            await Shell.Current.DisplayAlert("Success", "Tenant removed", "OK");
            await LoadTenants();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to remove tenant: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteHouse()
    {
        if (HouseId == 0) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Confirm", "Are you sure you want to delete this house?", "Yes", "No");

        if (!confirm) return;

        try
        {
            IsLoading = true;
            
            var success = await _apiService.DeleteAsync($"/api/houses/{HouseId}");
            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "House deleted", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to delete house", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to delete house: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
