using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CelebrityGuessr.Models
{
    public class Celebrity
    {
        // Dodanie '?' sprawia, że pole może być puste (null), co naprawia błąd CS8618
        public string? Name { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? Profession { get; set; }
        public int BirthYear { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class GuessResult
    {
        public Celebrity GuessData { get; set; } = new Celebrity(); // Inicjalizacja domyślna

        // Dajemy wartości domyślne "Gray" lub pusty string, żeby nie było nulli
        public string NameColor { get; set; } = "Gray";
        public string GenderColor { get; set; } = "Gray";
        public string NationalityColor { get; set; } = "Gray";
        public string ProfessionColor { get; set; } = "Gray";

        public string YearColor { get; set; } = "Gray";
        public string YearArrow { get; set; } = "";
    }
}
