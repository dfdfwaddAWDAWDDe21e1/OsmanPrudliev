using HouseApp.ViewModels;

namespace HouseApp.Views;

public partial class PaymentPage : ContentPage
{
    public PaymentPage(PaymentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
