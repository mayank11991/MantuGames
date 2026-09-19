namespace MantuGames.Helpers;

public static class GameRules
{
    public static string GetRules(string gameId) => gameId switch
    {
        "mazerunner" => "Guide the cat to the exit. Swipe or use arrows to move. Avoid walls and reach the golden door!",

        "sudoku" => "Fill the 5x5 grid so each row, column and region has every animal once. Tap to place, tap again to remove.",

        "blockpuzzle" => "Place falling blocks to complete full rows. Swipe to move, tap to rotate. Clear rows to score!",

        "cardmemory" => "Flip cards to find matching pairs. Tap two cards — if they match, they stay. Find all pairs to win!",

        "wordfinder" => "Swipe across letters to form hidden words. Words can be horizontal, vertical or diagonal. Find them all!",

        "towerofhanoi" => "Move all discs from left peg to right. Tap to pick, tap to place. Only smaller discs go on larger ones.",

        "puzzlepets" => "Drag each animal piece to its matching spot on the grid. Match all pieces to complete the puzzle!",

        "mathchallenge" => "Solve math equations before time runs out. Pick the correct answer from four choices. Answer fast for more coins!",

        "connectthedots" => "Connect matching colored dots by swiping between them. Paths cannot cross. Fill the whole board to win!",

        "numbermatch" => "Tap two matching numbers (same value or sum to 10). They must be adjacent or connected through empty spaces. Add new numbers if stuck!",

        "arrowlines" => "Tap arrows to launch them in their direction. They fly until they exit the board or hit another arrow. Clear all arrows to win!",

        _ => "Welcome to this game! Tap a level to start playing. Complete each level to earn stars and unlock new challenges. Have fun!"
    };
}
