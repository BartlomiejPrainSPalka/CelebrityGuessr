using SQLite;

namespace CelebrityGuessr.Models
{
    [Table("celebrities")]
    public class Celebrity
    {
        [PrimaryKey, Column("id")]
        public int Id { get; set; }

        [Column("imie_nazwisko")]
        public string? Name { get; set; }

        [Column("plec")]
        public string? Gender { get; set; }

        [Column("kraj")]
        public string? Nationality { get; set; }

        [Column("profesja")]
        public string? Profession { get; set; }

        [Column("rok_urodzenia")]
        public int BirthYear { get; set; }

        [Column("zdjecie_url")]
        public string? ImageUrl { get; set; }

        // Wzrost w centymetrach. 0 = brak danych.
        [Column("wzrost")]
        public int HeightCm { get; set; }

        [Column("kolor_wlosow")]
        public string? HairColor { get; set; }

        // Liczba obserwujących w tysiącach (np. 15000 = 15 mln). 0 = brak danych.
        [Column("obserwujacy")]
        public int FollowersThousands { get; set; }
    }

    public class GuessResult
    {
        public Celebrity GuessData { get; set; } = new Celebrity();
        public string NameColor { get; set; } = "Gray";
        public string GenderColor { get; set; } = "Gray";
        public string NationalityColor { get; set; } = "Gray";
        public string ProfessionColor { get; set; } = "Gray";
        public string YearColor { get; set; } = "Gray";
        public string YearArrow { get; set; } = "";

        public string HeightColor { get; set; } = "Gray";
        public string HeightArrow { get; set; } = "";

        public string HairColorResult { get; set; } = "Gray";

        public string FollowersColor { get; set; } = "Gray";
        public string FollowersArrow { get; set; } = "";
    }
}