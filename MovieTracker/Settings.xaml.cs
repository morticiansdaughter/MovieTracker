using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MovieTracker;

public partial class Settings : ContentPage
{
    private bool _loading = true;    
    private bool _isSaving;
    private int _lastScore;

    private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, "user.json");

    public Settings()
    {
        InitializeComponent();

        switch (Application.Current!.UserAppTheme)
        {
            case AppTheme.Light: LightRadio.IsChecked = true; break;
            case AppTheme.Dark: DarkRadio.IsChecked = true; break;
            default: SystemRadio.IsChecked = true; break;
        }
        _loading = false;

        UpdateStrength("");
        LoadSavedUser();
    }

    
    private void OnThemeChecked(object? sender, CheckedChangedEventArgs e)
    {
        if (_loading || !e.Value) return;

        AppTheme theme = sender == LightRadio ? AppTheme.Light
                       : sender == DarkRadio ? AppTheme.Dark
                       : AppTheme.Unspecified;       // podle systemu

        Application.Current!.UserAppTheme = theme;
        Preferences.Default.Set("theme", theme.ToString());
    }

  
    private void OnShowPasswordToggled(object? sender, ToggledEventArgs e)
    {
        PasswordEntry.IsPassword = !e.Value;
    }

    private void OnPasswordTextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateStrength(e.NewTextValue ?? "");
    }

    private void UpdateStrength(string pw)
    {
        if (StrengthBar is null || StrengthLabel is null || ChecklistLabel is null) return;

        bool len8 = pw.Length >= 8;
        bool len12 = pw.Length >= 12;
        bool digit = pw.Any(char.IsDigit);
        bool lower = pw.Any(char.IsLower);
        bool upper = pw.Any(char.IsUpper);
        bool special = pw.Any(c => !char.IsLetterOrDigit(c));

        int score = pw.Length == 0 ? 0 : new[] { len8, len12, digit, lower, upper, special }.Count(x => x);
        _lastScore = score;

        string text; Color color;
        if (pw.Length == 0) { text = "—"; color = Colors.Gray; }
        else if (score <= 2) { text = "Slabé"; color = Color.FromArgb("#B3261E"); }
        else if (score <= 4) { text = "Střední"; color = Color.FromArgb("#E68A00"); }
        else { text = "Silné"; color = Color.FromArgb("#2E7D32"); }

        StrengthLabel.Text = $"Síla hesla: {text}";
        StrengthBar.Progress = score / 6.0;
        StrengthBar.ProgressColor = color;

        string Mark(bool ok) => ok ? "✔" : "✘";
        ChecklistLabel.Text =
            $"{Mark(len8)} alespoň 8 znaků    {Mark(len12)} alespoň 12 znaků\n" +
            $"{Mark(digit)} číslice    {Mark(lower)} malá písmena\n" +
            $"{Mark(upper)} velká písmena    {Mark(special)} speciální znaky";
    }

 
    private static string Hash(string pw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pw)));

    private async void LoadSavedUser()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            string json = await File.ReadAllTextAsync(FilePath);
            var data = JsonSerializer.Deserialize<UserData>(json);
            if (data is null) return;
            UserNameEntry.Text = data.UserName;
            StatusLabel.Text = $"Uložený profil: {data.UserName} ({data.SavedAt:d. M. yyyy HH:mm})";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Nepodařilo se načíst profil: {ex.Message}";
        }
    }

    private async void OnSaveUserClicked(object? sender, EventArgs e)
    {
        if (_isSaving) return;

        string name = UserNameEntry.Text?.Trim() ?? "";
        string pw = PasswordEntry.Text ?? "";

        if (name.Length < 3)
        {
            await DisplayAlert("Chyba", "Jméno musí mít alespoň 3 znaky.", "OK");
            return;
        }
        if (_lastScore < 3)
        {
            await DisplayAlert("Slabé heslo", "Heslo je příliš slabé. Přidej délku, číslice, velká písmena nebo speciální znaky.", "OK");
            return;
        }

        _isSaving = true;
        SaveUserButton.IsEnabled = false;
        try
        {
            var data = new UserData { UserName = name, PasswordHash = Hash(pw), SavedAt = DateTime.Now };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);

            StatusLabel.Text = $"Uloženo: {FilePath}";
            await DisplayAlert("Uloženo", "Profil byl uložen do JSON souboru.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Chyba", $"Uložení do JSON selhalo: {ex.Message}", "OK");
        }
        finally
        {
            _isSaving = false;
            SaveUserButton.IsEnabled = true;
        }
    }

    private async void OnDeleteUserClicked(object? sender, EventArgs e)
    {
        if (!File.Exists(FilePath))
        {
            await DisplayAlert("Info", "Žádný uložený profil neexistuje.", "OK");
            return;
        }

        bool yes = await DisplayAlert("Smazat profil", "Opravdu chceš smazat uložený profil?", "Smazat", "Zrušit");
        if (!yes) return;

        try
        {
            File.Delete(FilePath);
            UserNameEntry.Text = "";
            PasswordEntry.Text = "";
            StatusLabel.Text = "Profil byl smazán.";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Chyba", $"Smazání selhalo: {ex.Message}", "OK");
        }
    }
}
