namespace HouseApp.ViewModels;

public class HomeViewModel : BindableObject
{
    public string DayName => DateTime.Now.ToString("dddd");
    public string FullDate => DateTime.Now.ToString("d MMMM yyyy");

    public string MonthlyRentText => "€520.00";
    public string RentSubtitle => "Monthly Rent";
    public string DueInfo => "Due soon";
}
