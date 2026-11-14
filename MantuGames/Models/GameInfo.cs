namespace MantuGames.Models;

public class GameInfo
{
    public string Id { get; init; } = ""; // unique key e.g. "sudoku"
    public string Title { get; init; } = ""; // display name
    public string ImageName { get; init; } = ""; // icon file shown on card, e.g. "gi_sudoku.png"

    public string Description { get; init; } = ""; // short tagline
    public string CardColor { get; init; } = ""; // hex background for card
    public string Route      { get; init; } = ""; // Shell navigation route
    public string IconEmoji  { get; init; } = ""; // emoji shown in bottom bar circle
    public string IconColor  { get; init; } = "#4CAF50"; // circle color for bottom bar icon
}
