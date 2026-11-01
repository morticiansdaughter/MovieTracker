namespace MovieTracker
{
    public class UserData
    {
        public string UserName { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public DateTime SavedAt { get; set; }
    }
}
