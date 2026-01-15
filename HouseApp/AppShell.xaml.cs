using HouseApp.Views;

namespace HouseApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("housemanagement", typeof(HouseManagementPage));
            Routing.RegisterRoute("payment", typeof(PaymentPage));
            Routing.RegisterRoute("landlorddashboard", typeof(LandlordDashboardPage));
        }
    }
}
