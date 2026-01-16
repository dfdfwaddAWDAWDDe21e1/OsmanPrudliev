using HouseApp.ViewModels;

namespace HouseApp.Views;

public partial class HouseSearchPage : ContentPage
{
    public HouseSearchPage(HouseSearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
