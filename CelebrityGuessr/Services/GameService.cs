using CelebrityGuessr.Models;
using SQLite;

namespace CelebrityGuessr.Services
{
    public class GameService
    {
        private List<Celebrity> _allCelebrities = new List<Celebrity>();
        private Celebrity? _targetCelebrity = null!;

        private SQLiteAsyncConnection? _db;
        private const string DbName = "celebrities.db";

        // PODNIEŚ ten numer za każdym razem, gdy podmieniasz zawartość/schemat celebrities.db
        // w projekcie (nowe kolumny, poprawione dane itd.). Dzięki temu stara kopia zapisana
        // na urządzeniu zostanie usunięta i zastąpiona świeżą wersją z assetów.
        private const int DbVersion = 2;
        private const string DbVersionPrefKey = "celebrities_db_version";

        // Progi tolerancji używane przy kolorowaniu "zbliżonych" wyników
        private const int HeightCloseThresholdCm = 5;
        private const int HeightYellowThresholdCm = 15;

        // Grupy podobieństwa kolorów włosów - naturalne odcienie traktowane jako "blisko"
        private static readonly List<HashSet<string>> HairColorGroups = new()
        {
            new HashSet<string> { "blond", "jasny blond", "ciemny blond" },
            new HashSet<string> { "brązowe", "jasny brąz", "ciemny brąz", "kasztanowe" },
            new HashSet<string> { "czarne", "ciemny brąz" },
            new HashSet<string> { "rude", "kasztanowe" },
            new HashSet<string> { "siwe", "szpakowate" },
        };

        public GameService()
        {
            InitializeDatabase();
        }

        public Task InitializeDatabase()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);

            int savedVersion = Preferences.Get(DbVersionPrefKey, 0);
            bool needsFreshCopy = !File.Exists(dbPath) || savedVersion < DbVersion;

            if (needsFreshCopy)
            {
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }

                Task.Run(async () =>
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync(DbName);
                    using var newStream = File.Create(dbPath);
                    await stream.CopyToAsync(newStream);
                }).Wait();

                Preferences.Set(DbVersionPrefKey, DbVersion);
            }

            _db = new SQLiteAsyncConnection(dbPath);

            _allCelebrities = _db.Table<Celebrity>().ToListAsync().Result;

            StartNewGame();
            return Task.CompletedTask;
        }

        public Celebrity GetTargetCelebrity()
        {
            return _targetCelebrity ?? new Celebrity();
        }

        public string GetTargetCelebrityImageUrl()
        {
            return _targetCelebrity!.ImageUrl!;
        }

        public void StartNewGame()
        {
            if (_allCelebrities.Count > 0)
            {
                var random = new Random();
                _targetCelebrity = _allCelebrities[random.Next(_allCelebrities.Count)];
            }
        }

        public List<Celebrity> SearchCelebrities(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Celebrity>();

            return _allCelebrities
                .Where(c => !string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(query.ToLower()))
                .ToList();
        }

        public GuessResult CheckGuess(Celebrity guessedCeleb)
        {
            var target = _targetCelebrity!;

            var result = new GuessResult { GuessData = guessedCeleb };

            result.NameColor = string.Equals(guessedCeleb.Name, target.Name, StringComparison.OrdinalIgnoreCase) ? "Green" : "Red";
            result.GenderColor = string.Equals(guessedCeleb.Gender, target.Gender, StringComparison.OrdinalIgnoreCase) ? "Green" : "Red";
            result.NationalityColor = string.Equals(guessedCeleb.Nationality, target.Nationality, StringComparison.OrdinalIgnoreCase) ? "Green" : "Red";
            result.ProfessionColor = string.Equals(guessedCeleb.Profession, target.Profession, StringComparison.OrdinalIgnoreCase) ? "Green" : "Red";

            if (guessedCeleb.BirthYear == target.BirthYear)
            {
                result.YearColor = "Green";
                result.YearArrow = "";
            }
            else
            {
                result.YearColor = "Red";
                result.YearArrow = guessedCeleb.BirthYear < target.BirthYear ? "↑" : "↓";
            }

            ApplyHeightComparison(result, guessedCeleb, target);
            ApplyHairColorComparison(result, guessedCeleb, target);
            ApplyFollowersComparison(result, guessedCeleb, target);

            return result;
        }

        private static void ApplyHeightComparison(GuessResult result, Celebrity guessed, Celebrity target)
        {
            // Brak danych po którejkolwiek stronie -> nie oceniamy
            if (guessed.HeightCm <= 0 || target.HeightCm <= 0)
            {
                result.HeightColor = "Gray";
                result.HeightArrow = "";
                return;
            }

            int diff = Math.Abs(guessed.HeightCm - target.HeightCm);

            if (guessed.HeightCm == target.HeightCm)
            {
                result.HeightColor = "Green";
                result.HeightArrow = "";
            }
            else
            {
                result.HeightColor = diff <= HeightYellowThresholdCm ? "Orange" : "Red";
                result.HeightArrow = guessed.HeightCm < target.HeightCm ? "↑" : "↓";
            }
        }

        private static void ApplyHairColorComparison(GuessResult result, Celebrity guessed, Celebrity target)
        {
            var guessedColor = guessed.HairColor?.Trim().ToLowerInvariant();
            var targetColor = target.HairColor?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(guessedColor) || string.IsNullOrEmpty(targetColor))
            {
                result.HairColorResult = "Gray";
                return;
            }

            if (guessedColor == targetColor)
            {
                result.HairColorResult = "Green";
                return;
            }

            bool sameFamily = HairColorGroups.Any(group => group.Contains(guessedColor) && group.Contains(targetColor));
            result.HairColorResult = sameFamily ? "Orange" : "Red";
        }

        private static void ApplyFollowersComparison(GuessResult result, Celebrity guessed, Celebrity target)
        {
            if (guessed.FollowersThousands <= 0 || target.FollowersThousands <= 0)
            {
                result.FollowersColor = "Gray";
                result.FollowersArrow = "";
                return;
            }

            if (guessed.FollowersThousands == target.FollowersThousands)
            {
                result.FollowersColor = "Green";
                result.FollowersArrow = "";
                return;
            }

            double ratio = (double)guessed.FollowersThousands / target.FollowersThousands;
            bool isClose = ratio >= 0.5 && ratio <= 2.0; // "ten sam rząd wielkości"

            result.FollowersColor = isClose ? "Orange" : "Red";
            result.FollowersArrow = guessed.FollowersThousands < target.FollowersThousands ? "↑" : "↓";
        }
    }
}