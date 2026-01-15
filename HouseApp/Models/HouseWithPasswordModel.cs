namespace HouseApp.Models;

public class HouseWithPasswordModel : House
{
    public string Password { get; set; } = string.Empty;
    public int AvailableSpots { get; set; }
}
