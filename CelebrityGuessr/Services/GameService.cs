// W pliku Services/GameService.cs

using CelebrityGuessr.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace CelebrityGuessr.Services
{
    public class GameService
    {
        private List<Celebrity> _allCelebrities = new List<Celebrity>();
        private Celebrity? _targetCelebrity = null!;
        
        private SQLiteAsyncConnection? _db;
        private const string DbName = "celebrities.db";

        // Konstruktor jest pusty, aby móc być zarejestrowanym w DI
        public GameService()
        {
            InitializeDatabase();
        }

        // --- INICJALIZACJA BAZY DANYCH (ASYNC) ---
        public Task InitializeDatabase()
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
            _db = new SQLiteAsyncConnection(dbPath);

            // 3. Pobieramy wszystkich celebrytów do listy (zamiast hardcode'owania)
            // Dzięki atrybutom [Column] w modelu, SQLite wie jak czytać polskie kolumny
            _allCelebrities = _db.Table<Celebrity>().ToListAsync().Result;

            StartNewGame();
            return Task.CompletedTask;
        }

        // --- POBIERANIE CELU (Dla PictureMode) ---
        public Celebrity? GetTargetCelebrity()
        {
            return _targetCelebrity;
        }

        // --- ROZPOCZĘCIE NOWEJ GRY ---
        public void StartNewGame()
        {
            if (_allCelebrities.Count > 0)
            {
                var random = new Random();
                _targetCelebrity = _allCelebrities[random.Next(_allCelebrities.Count)];
            }
        }

        // --- WYSZUKIWANIE CELEBRYTÓW ---
        public List<Celebrity> SearchCelebrities(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Celebrity>();

            return _allCelebrities
                .Where(c => !string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(query.ToLower()))
                .ToList();
        }

        // --- SPRAWDZANIE ZGADNIĘCIA ---
        public GuessResult CheckGuess(Celebrity guessedCeleb)
        {
            var target = _targetCelebrity!; // Używamy '!', bo ufamy, że cel jest ustawiony

            var result = new GuessResult { GuessData = guessedCeleb };

            // Porównania stringów bez rozróżniania wielkości liter
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

            return result;
        }
    }
}