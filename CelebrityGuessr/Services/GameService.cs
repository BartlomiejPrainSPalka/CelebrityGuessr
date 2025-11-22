using CelebrityGuessr.Models;

namespace CelebrityGuessr.Services
{
    public class GameService
    {
        private List<Celebrity> _allCelebrities;

        private Celebrity _targetCelebrity = null!;

        public GameService()
        {
            _allCelebrities = new List<Celebrity>
            {
                new Celebrity { Name = "Robert Lewandowski", Gender = "Mężczyzna", Nationality = "Polska", Profession = "Sportowiec", BirthYear = 1988 },
                new Celebrity { Name = "Iga Świątek", Gender = "Kobieta", Nationality = "Polska", Profession = "Sportowiec", BirthYear = 2001 },
                new Celebrity { Name = "Brad Pitt", Gender = "Mężczyzna", Nationality = "USA", Profession = "Aktor", BirthYear = 1963 },
            };

            StartNewGame();
        }

        public void StartNewGame()
        {
            var random = new Random();
            _targetCelebrity = _allCelebrities[random.Next(_allCelebrities.Count)];
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
            var result = new GuessResult { GuessData = guessedCeleb };

            result.NameColor = guessedCeleb.Name == _targetCelebrity.Name ? "Green" : "Red";
            result.GenderColor = guessedCeleb.Gender == _targetCelebrity.Gender ? "Green" : "Red";
            result.NationalityColor = guessedCeleb.Nationality == _targetCelebrity.Nationality ? "Green" : "Red";
            result.ProfessionColor = guessedCeleb.Profession == _targetCelebrity.Profession ? "Green" : "Red";

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