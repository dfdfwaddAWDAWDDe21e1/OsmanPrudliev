namespace HouseApp.Views;

public partial class HomePage : ContentPage
{
    public HomePage(ViewModels.HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
