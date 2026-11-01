namespace MovieTracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

       
        var saved = Preferences.Default.Get("theme", AppTheme.Unspecified.ToString());
        if (Enum.TryParse<AppTheme>(saved, out var theme))
            UserAppTheme = theme;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
