namespace MovieTracker;

public partial class WebPage : ContentPage
{
    public WebPage()
    {
        InitializeComponent();
    }

    private static bool IsAllowedHost(string host)
    {
        host = host.ToLowerInvariant();
        return host == "csfd.cz" || host.EndsWith(".csfd.cz");
    }

    private async void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!Uri.TryCreate(e.Url, UriKind.Absolute, out var uri)) return;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return;

        if (!IsAllowedHost(uri.Host))
        {
            e.Cancel = true;     
            await DisplayAlert("Zakázáno",
                $"Na stránku {uri.Host} nemáš povolený přístup. Můžeš zůstat jen na csfd.cz.", "OK");
            return;
        }

        WebLoading.IsVisible = true;
        WebLoading.IsRunning = true;
        LoadBar.IsVisible = true;
        LoadBar.Progress = 0.3;
    }

    private async void OnNavigated(object? sender, WebNavigatedEventArgs e)
    {
        WebLoading.IsRunning = false;
        WebLoading.IsVisible = false;
        LoadBar.Progress = 1;
        LoadBar.IsVisible = false;

        if (e.Result == WebNavigationResult.Failure || e.Result == WebNavigationResult.Timeout)
        {
            await DisplayAlert("Chyba", "Stránku se nepodařilo načíst. Zkontroluj připojení k internetu.", "OK");
        }
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        if (Browser.CanGoBack) Browser.GoBack();
    }

    private void OnReloadClicked(object? sender, EventArgs e) => Browser.Reload();
}
