using CodeNames.Models;

namespace CodeNames.BuildingBlocks.Helpers;

public static class ColorHelper
{
    public static Dictionary<Color, string> ColorToHexDict = new()
    {
        { Color.Red, "#9c1616" },
        { Color.Blue, "#13489e" },
        { Color.Neutral, "#FFFDD0" },
        { Color.Black, "#000000" },
        { Color.BackgroundBlue, "#cbddfb" },
        { Color.BackgroundRed, "#e58282" },
        { Color.InitialNeutralBackground, "#66b02c"}
    };

    public static Dictionary<Color, Color> OppositeTeamsDict = new()
    {
        { Color.Red, Color.Blue },
        { Color.Blue, Color.Red }
    };

    public static Dictionary<Color, string> OppositeTeamsBackgroundColorDict = new()
    {
        { Color.Blue, ColorToHexDict[Color.BackgroundRed] },
        { Color.Red,  ColorToHexDict[Color.BackgroundBlue] }
    };
}
