using HouseApp.Views;

namespace HouseApp
{
    public partial class StudentShell : Shell
    {
        public StudentShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("housesearch", typeof(HouseSearchPage));
            Routing.RegisterRoute("housemanagement", typeof(HouseManagementPage));
            Routing.RegisterRoute("payment", typeof(PaymentPage));
        }
    }
}
