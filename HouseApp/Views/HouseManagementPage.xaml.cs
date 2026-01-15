using HouseApp.ViewModels;

namespace HouseApp.Views;

public partial class HouseManagementPage : ContentPage
{
    public HouseManagementPage(HouseManagementViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
