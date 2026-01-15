using HouseApp.ViewModels;

namespace HouseApp.Views;

public partial class LandlordDashboardPage : ContentPage
{
    private readonly LandlordDashboardViewModel _viewModel;

    public LandlordDashboardPage(LandlordDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
