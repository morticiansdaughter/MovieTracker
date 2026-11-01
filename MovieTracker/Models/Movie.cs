namespace MovieTracker
{
    public class Movie
    {
        public string Title { get; set; } = "";
        public bool IsSeries { get; set; }
        public int Runtime { get; set; }
        public int Rating { get; set; }
        public bool Watched { get; set; }
        public bool Favorite { get; set; }

        // Vlastnosti 
        public string Icon => IsSeries ? "icon_tv.png" : "icon_movie.png";
        public string Subtitle =>
            $"{(IsSeries ? "Seriál" : "Film")} • {Runtime} min • {(Watched ? "zhlédnuto" : "ke zhlédnutí")}";
        public string RatingText => Watched ? $"★ {Rating}/5" : "—";
        public string FavoriteMark => Favorite ? "♥" : "";
    }


    public static class MovieStore
    {
        public static List<Movie> Items { get; } = new()
        {
            new Movie { Title = "Avatar: Fire and ash",IsSeries = false, Runtime = 197, Rating = 5, Watched = true,  Favorite = true  },
            new Movie { Title = "Breaking Bad",       IsSeries = true,  Runtime = 47,  Rating = 5, Watched = true,  Favorite = true  },
            new Movie { Title = "Shrek",         IsSeries = false, Runtime = 90,  Rating = 4, Watched = true,  Favorite = false },
            new Movie { Title = "Stranger Things",  IsSeries = true,  Runtime = 50,  Rating = 2, Watched = true,  Favorite = false },
            new Movie { Title = "Project Hail Mary",   IsSeries = false, Runtime = 156, Rating = 5, Watched = true, Favorite = true },
            new Movie { Title = "The Last of Us",   IsSeries = true,  Runtime = 55,  Rating = 0, Watched = false, Favorite = true  },
        };
    }
}
