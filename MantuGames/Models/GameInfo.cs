namespace MantuGames.Models;

public class GameInfo
{
    public string Id { get; init; } // unique key e.g. "sudoku"
    public string Title { get; init; } // display name
    public string ImageName { get; init; } // icon file shown on card, e.g. "sudoku.png"

    public string Description { get; init; } // short tagline
    public string CardColor { get; init; } // hex background for card
    public string Route      { get; init; } // Shell navigation route
}
