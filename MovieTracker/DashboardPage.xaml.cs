namespace MovieTracker;

public partial class DashboardPage : ContentPage
{
    private const int YearGoal = 24;

    private string _typeFilter = "all";   // all / movie / series
    private bool _firstLoad = true;
    private bool _isNavigating;           // ochrana proti dvojkliku

    public DashboardPage()
    {
        InitializeComponent();
        SizeChanged += OnPageSizeChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isNavigating = false;   // po návratu z formuláře můžeme znovu klikat

        if (_firstLoad)
        {
            _firstLoad = false;
            await Task.Delay(1000);          
            Loading.IsRunning = false;
            Loading.IsVisible = false;
            MainContent.IsVisible = true;
        }

        RefreshAll();
    }

   
    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0) return;
        bool wide = Width >= 600;

        var cards = new[] { CardWatched, CardSeries, CardHours, CardRating };
        for (int i = 0; i < cards.Length; i++)
        {
            if (wide)
            {
                Grid.SetRow(cards[i], 0);
                Grid.SetColumn(cards[i], i);
                Grid.SetColumnSpan(cards[i], 1);
            }
            else
            {
                Grid.SetRow(cards[i], i / 2);
                Grid.SetColumn(cards[i], (i % 2) * 2);
                Grid.SetColumnSpan(cards[i], 2);
            }
        }
    }

  
    private async void OnAddClicked(object? sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;
        try
        {
            await Shell.Current.GoToAsync(nameof(AddMoviePage));
        }
        catch (Exception ex)
        {
            _isNavigating = false;
            await DisplayAlert("Chyba", $"Nepodařilo se otevřít formulář: {ex.Message}", "OK");
        }
    }

   
    private void OnTypeFilterClicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is string value)
        {
            _typeFilter = value;
            RefreshAll();
        }
    }

    private void OnWatchedToggled(object? sender, ToggledEventArgs e) => RefreshAll();
    private void OnFavChanged(object? sender, CheckedChangedEventArgs e) => RefreshAll();


    private void RefreshAll()
    {
        //cela sbirka statistiky
        var watched = MovieStore.Items.Where(m => m.Watched).ToList();
        int films = watched.Count(m => !m.IsSeries);
        int series = watched.Count(m => m.IsSeries);
        double hours = watched.Sum(m => m.Runtime) / 60.0;
        double avg = watched.Count == 0 ? 0 : watched.Average(m => m.Rating);

        WatchedValue.Text = films.ToString();
        SeriesValue.Text = series.ToString();
        HoursValue.Text = hours.ToString("0.#");
        RatingValue.Text = avg.ToString("0.0");

        double progress = Math.Min(1.0, watched.Count / (double)YearGoal);
        YearProgress.Progress = progress;
        ProgressLabel.Text = $"{watched.Count} / {YearGoal} ({(int)(progress * 100)} %)";

        //filtrovany seznams
        IEnumerable<Movie> q = MovieStore.Items;
        if (_typeFilter == "movie") q = q.Where(m => !m.IsSeries);
        if (_typeFilter == "series") q = q.Where(m => m.IsSeries);
        if (OnlyWatchedSwitch.IsToggled) q = q.Where(m => m.Watched);
        if (OnlyFavCheck.IsChecked) q = q.Where(m => m.Favorite);

        var list = q.ToList();
        BindableLayout.SetItemsSource(MoviesLayout, list);
        EmptyLabel.IsVisible = list.Count == 0;

        string typeText = _typeFilter switch { "movie" => "filmy", "series" => "seriály", _ => "vše" };
        FilterInfo.Text = $"Zobrazeno: {typeText} ({list.Count})";
    }
}
