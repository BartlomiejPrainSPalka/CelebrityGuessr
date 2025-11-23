using CelebrityGuessr.Models;
using SQLite; // Potrzebne do obsługi bazy

namespace CelebrityGuessr.Services
{
    public class GameService
    {
        private List<Celebrity> _allCelebrities = new();
        private Celebrity _targetCelebrity = null!;
        public Celebrity TargetCelebrity
        {
            get { return _targetCelebrity; }
            set { _targetCelebrity = value; }
        }

        // Nazwa pliku w Resources/Raw
        private const string DbName = "celebrities.db";
        private SQLiteConnection? _db;

        public GameService()
        {
            // Konstruktor nie może być async, więc inicjalizację wywołujemy osobno
            // lub blokujemy wątek (mniej zalecane, ale tu dla uproszczenia wewnątrz Init)
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Ścieżka docelowa na urządzeniu (Android/iOS/Windows)
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);

            // 1. Jeśli bazy nie ma na urządzeniu, kopiujemy ją z Resources/Raw
            if (!File.Exists(dbPath))
            {
                // Ponieważ OpenAppPackageFileAsync jest asynchroniczne, musimy poczekać
                // W prawdziwej aplikacji lepiej zrobić metodę InitAsync() wywoływaną z UI
                Task.Run(async () =>
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync(DbName);
                    using var newStream = File.Create(dbPath);
                    await stream.CopyToAsync(newStream);
                }).Wait();
            }

            // 2. Łączymy się z bazą
            _db = new SQLiteConnection(dbPath);

            // 3. Pobieramy wszystkich celebrytów do listy (zamiast hardcode'owania)
            // Dzięki atrybutom [Column] w modelu, SQLite wie jak czytać polskie kolumny
            _allCelebrities = _db.Table<Celebrity>().ToList();

            StartNewGame();
        }

        public void StartNewGame()
        {
            if (_allCelebrities.Count == 0) return;

            var random = new Random();
            TargetCelebrity = _allCelebrities[random.Next(_allCelebrities.Count)];
        }

        public List<Celebrity> SearchCelebrities(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Celebrity>();

            return _allCelebrities
                .Where(c => !string.IsNullOrEmpty(c.Name) &&
                            c.Name.ToLower().Contains(query.ToLower()))
                .ToList();
        }

        public GuessResult CheckGuess(Celebrity guessedCeleb)
        {
            var result = new GuessResult { GuessData = guessedCeleb };

            // Logika porównywania (bez zmian)
            // Uwaga: Baza SQL ma małe litery w płci (np. 'kobieta'), 
            // upewnij się, że porównujesz case-insensitive lub znormalizuj dane.

            result.NameColor = string.Equals(guessedCeleb.Name, _targetCelebrity.Name, StringComparison.OrdinalIgnoreCase) ? "Green" : "Red";

            // Przykład normalizacji porównania dla stringów
            result.GenderColor = string.Equals(guessedCeleb.Gender, _targetCelebrity.Gender, StringComparison.OrdinalIgnoreCase) ? "Green" : "Red";

            result.NationalityColor = string.Equals(guessedCeleb.Nationality, _targetCelebrity.Nationality, StringComparison.OrdinalIgnoreCase) ? "Green" : "Red";

            result.ProfessionColor = string.Equals(guessedCeleb.Profession, _targetCelebrity.Profession, StringComparison.OrdinalIgnoreCase) ? "Green" : "Red";

            if (guessedCeleb.BirthYear == _targetCelebrity.BirthYear)
            {
                result.YearColor = "Green";
                result.YearArrow = "";
            }
            else
            {
                result.YearColor = "Red";
                result.YearArrow = guessedCeleb.BirthYear < _targetCelebrity.BirthYear ? "↑" : "↓";
            }

            return result;
        }
    }
}