namespace MantuGames.Models;

public class GameInfo
{
    public string Id { get; init; } // unique key e.g. "sudoku"
    public string Title { get; init; } // display name
    public string Emoji { get; init; } // icon shown on card
    public string ImageName { get; init; } // e.g. "game_sudoku.svg"

    public string Description { get; init; } // short tagline
    public string CardColor { get; init; } // hex background for card
    public string Route      { get; init; } // Shell navigation route
}