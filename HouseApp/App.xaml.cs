using HouseApp.Models;

namespace HouseApp;

public partial class App : Application
{
    private readonly AppShell _shell;
    private readonly IServiceProvider _serviceProvider;

    public App(AppShell shell, IServiceProvider serviceProvider)
    {
        InitializeComponent();  
        _shell = shell;
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(_shell);

    public void SetShellForUserType(UserType userType)
    {
        MainPage = userType == UserType.Landlord 
            ? _serviceProvider.GetRequiredService<LandlordShell>() 
            : _serviceProvider.GetRequiredService<StudentShell>();
    }
}
