namespace MovieTracker;

public partial class AddMoviePage : ContentPage
{
    private bool _isSaving;

    public AddMoviePage()
    {
        InitializeComponent();
        ValidateForm();
    }


    protected override bool OnBackButtonPressed()
    {
        return _isSaving ? true : base.OnBackButtonPressed();
    }

    
    private void OnFormTextChanged(object? sender, TextChangedEventArgs e) => ValidateForm();

    private bool ValidateForm()
    {
        
        if (TitleEntry is null || RuntimeEntry is null || SaveButton is null) return false;

        string title = (TitleEntry.Text ?? "").Trim();
        bool titleOk = title.Length >= 2;
        TitleError.Text = (title.Length > 0 && !titleOk) ? "Název musí mít alespoň 2 znaky." : "";

        string runtimeText = RuntimeEntry.Text ?? "";
        bool runtimeOk = int.TryParse(runtimeText, out int runtime) && runtime >= 1 && runtime <= 1000;
        RuntimeError.Text = (runtimeText.Length > 0 && !runtimeOk) ? "Zadej celé číslo 1 - 1000." : "";

        bool valid = titleOk && runtimeOk;
        SaveButton.IsEnabled = valid && !_isSaving;
        return valid;
    }

    private void OnRatingChanged(object? sender, ValueChangedEventArgs e)
    {
        if (RatingLabel is null) return;

        double rounded = Math.Round(e.NewValue);

        // hodnota se zaokrouhli
        
        if (Math.Abs(RatingSlider.Value - rounded) > 0.001)
        {
            RatingSlider.Value = rounded;
            return;
        }

        RatingLabel.Text = $"Hodnocení: {(int)rounded} / 5";
    }

    
    private void OnSeriesToggled(object? sender, ToggledEventArgs e)
    {
        TypeLabel.Text = e.Value ? "Typ: seriál" : "Typ: film";
        RuntimeCaption.Text = e.Value ? "Délka jedné epizody v minutách" : "Délka v minutách";
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_isSaving) return;
        if (!ValidateForm()) return;

        _isSaving = true;
        SaveButton.IsEnabled = false;       // ochrana proti dvojkliku
        CancelButton.IsEnabled = false;
        SavingIndicator.IsVisible = true;
        SavingIndicator.IsRunning = true;

        bool success = false;
        try
        {
            await Task.Delay(2000);         
            if (FailSwitch.IsToggled)
                throw new InvalidOperationException("Server neodpovídá (simulovaná chyba).");

            MovieStore.Items.Insert(0, new Movie
            {
                Title = TitleEntry.Text!.Trim(),
                Runtime = int.Parse(RuntimeEntry.Text!),
                Rating = (int)RatingSlider.Value,
                IsSeries = SeriesSwitch.IsToggled,
                Watched = WatchedCheck.IsChecked,
                Favorite = FavoriteCheck.IsChecked
            });
            success = true;
        }
        catch (Exception ex)
        {
            
            await DisplayAlert("Chyba", $"Uložení se nezdařilo: {ex.Message}", "OK");
        }
        finally
        {
            SavingIndicator.IsRunning = false;
            SavingIndicator.IsVisible = false;
            CancelButton.IsEnabled = true;
            _isSaving = false;
            ValidateForm();                 
        }

        if (success)
        {
            await DisplayAlert("Úspěch", "Záznam byl uložen", "OK");
            await Shell.Current.GoToAsync("..");  
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        if (_isSaving) return;
        await Shell.Current.GoToAsync("..");
    }
}
