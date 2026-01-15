namespace HouseApp.Services;

public class UserSession
{
    public string? UserName { get; set; }
    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(UserName);
}
