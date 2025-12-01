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

        public GameService()
        {
            InitializeDatabase();
        }

        public Task InitializeDatabase()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);

            if (!File.Exists(dbPath))
            {
                Task.Run(async () =>
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync(DbName);
                    using var newStream = File.Create(dbPath);
                    await stream.CopyToAsync(newStream);
                }).Wait();
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
            return _targetCelebrity.ImageUrl;
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

            return result;
        }
    }
}