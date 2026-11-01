namespace MovieTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registrace trasy pro stránku, která není v menu
        Routing.RegisterRoute(nameof(AddMoviePage), typeof(AddMoviePage));
    }
}
