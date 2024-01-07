using CodeNames.Models;

namespace CodeNames
{
    public class StaticDetails
    {
        public static int NumberOfCards { get; set; } = 25;

        public static Dictionary<Color, string> ColorToHexDict = new()
        {
            { Color.Red, "#FF0000" },
            { Color.Blue, "#0000FF" },
            { Color.Neutral, "#FFFDD0" },
            { Color.Black, "#000000" }
        };
    }
}
