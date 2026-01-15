using HouseApp.ViewModels;

namespace HouseApp.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
