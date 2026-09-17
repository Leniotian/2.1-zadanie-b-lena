using System.Windows.Media;

namespace SNAKE.Models;

public enum SkinPattern
{
    Smooth,
    Striped,
    Spotted,
    Diamond
}

public class SnakeSkin
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Color HeadColor { get; set; }
    public Color HeadOutlineColor { get; set; }
    public Color BodyColorA { get; set; }
    public Color BodyColorB { get; set; }
    public Color EyeColor { get; set; }
    public Color PupilColor { get; set; }
    public Color TongueColor { get; set; }
    public Color PatternColor { get; set; }
    public SkinPattern Pattern { get; set; } = SkinPattern.Smooth;

    public static IReadOnlyList<SnakeSkin> AllSkins { get; } = new List<SnakeSkin>
    {
        new SnakeSkin
        {
            Id = "forest_viper",
            Name = "Leśny Zaskroniec",
            Description = "Klasyczny wąż o soczysto-zielonych łuskach z leśnymi akcentami.",
            HeadColor = Color.FromRgb(76, 175, 80),        // Vibrant green
            HeadOutlineColor = Color.FromRgb(46, 125, 50),
            BodyColorA = Color.FromRgb(67, 160, 71),
            BodyColorB = Color.FromRgb(102, 187, 106),
            EyeColor = Colors.White,
            PupilColor = Color.FromRgb(27, 94, 32),
            TongueColor = Color.FromRgb(244, 67, 54),
            PatternColor = Color.FromRgb(46, 125, 50),
            Pattern = SkinPattern.Spotted
        },
        new SnakeSkin
        {
            Id = "autumn_serpent",
            Name = "Jesienny Szelest",
            Description = "Płomienne barwy opadających liści dębu i klonu.",
            HeadColor = Color.FromRgb(230, 81, 0),         // Fiery amber orange
            HeadOutlineColor = Color.FromRgb(191, 54, 12),
            BodyColorA = Color.FromRgb(239, 108, 0),
            BodyColorB = Color.FromRgb(251, 140, 0),
            EyeColor = Color.FromRgb(255, 248, 225),
            PupilColor = Color.FromRgb(127, 0, 0),
            TongueColor = Color.FromRgb(255, 179, 0),
            PatternColor = Color.FromRgb(183, 28, 28),
            Pattern = SkinPattern.Diamond
        },
        new SnakeSkin
        {
            Id = "midnight_glow",
            Name = "Nocny Błysk",
            Description = "Mroczny wąż z leśnych głębin o neonowo świecących ślepiach.",
            HeadColor = Color.FromRgb(26, 35, 126),        // Deep midnight blue
            HeadOutlineColor = Color.FromRgb(0, 188, 212),
            BodyColorA = Color.FromRgb(13, 71, 161),
            BodyColorB = Color.FromRgb(21, 101, 192),
            EyeColor = Color.FromRgb(0, 229, 255),
            PupilColor = Color.FromRgb(0, 0, 80),
            TongueColor = Color.FromRgb(0, 229, 255),
            PatternColor = Color.FromRgb(0, 229, 255),
            Pattern = SkinPattern.Striped
        },
        new SnakeSkin
        {
            Id = "golden_viper",
            Name = "Złota Żmija",
            Description = "Królewski gad z lśniącymi łuskami i rubinowymi oczami.",
            HeadColor = Color.FromRgb(255, 193, 7),        // Amber gold
            HeadOutlineColor = Color.FromRgb(218, 148, 0),
            BodyColorA = Color.FromRgb(255, 179, 0),
            BodyColorB = Color.FromRgb(255, 213, 79),
            EyeColor = Colors.White,
            PupilColor = Color.FromRgb(198, 40, 40),       // Ruby
            TongueColor = Color.FromRgb(229, 57, 53),
            PatternColor = Color.FromRgb(218, 148, 0),
            Pattern = SkinPattern.Diamond
        },
        new SnakeSkin
        {
            Id = "amanita_mushroom",
            Name = "Leśny Muchomor",
            Description = "Czerwień kapelusza muchomora usiana białymi zarodnikami.",
            HeadColor = Color.FromRgb(229, 57, 53),        // Crimson red
            HeadOutlineColor = Color.FromRgb(183, 28, 28),
            BodyColorA = Color.FromRgb(211, 47, 47),
            BodyColorB = Color.FromRgb(239, 83, 80),
            EyeColor = Colors.White,
            PupilColor = Color.FromRgb(66, 66, 66),
            TongueColor = Color.FromRgb(255, 205, 210),
            PatternColor = Color.FromRgb(255, 255, 255),
            Pattern = SkinPattern.Spotted
        },
        new SnakeSkin
        {
            Id = "mossy_forest",
            Name = "Gęsty Mech",
            Description = "Maska kamuflażu z mchu i kory prastarych dębów.",
            HeadColor = Color.FromRgb(51, 105, 30),        // Deep olive moss
            HeadOutlineColor = Color.FromRgb(27, 94, 32),
            BodyColorA = Color.FromRgb(69, 90, 100),
            BodyColorB = Color.FromRgb(85, 139, 47),
            EyeColor = Color.FromRgb(220, 237, 200),
            PupilColor = Color.FromRgb(30, 40, 10),
            TongueColor = Color.FromRgb(141, 110, 99),
            PatternColor = Color.FromRgb(93, 64, 55),
            Pattern = SkinPattern.Striped
        }
    };
}
