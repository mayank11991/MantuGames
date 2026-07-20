namespace MantuGames.Helpers;

public static class GameRules
{
    public static string GetRules(string gameId) => gameId switch
    {
        "mazerunner" => "Navigate the cat through the maze to reach the destination!\n\n"
            + "• Use the arrow buttons or swipe on the maze to move\n"
            + "• Avoid walls — you cannot walk through them\n"
            + "• Reach the golden exit to complete the level\n"
            + "• Move fast to earn more stars!",

        "sudoku" => "Fill the 5×5 grid so each row, column, and bold region contains every animal exactly once.\n\n"
            + "• Tap an animal button, then tap an empty cell to place it\n"
            + "• Use ⌫ to erase a placed animal\n"
            + "• Each animal can appear only once per row, column, and region\n"
            + "• Complete the puzzle to earn stars!",

        "blockpuzzle" => "Arrange falling blocks to complete horizontal lines.\n\n"
            + "• Swipe left/right to move the falling piece\n"
            + "• Tap to rotate the piece\n"
            + "• Swipe down to drop it faster\n"
            + "• Complete a row to clear it and earn points\n"
            + "• The game ends when blocks reach the top!",

        "cardmemory" => "Flip cards to find matching animal pairs!\n\n"
            + "• Tap a card to flip it and reveal the animal\n"
            + "• Tap another card to find its match\n"
            + "• Matching pairs stay revealed\n"
            + "• Mismatched cards flip back face-down\n"
            + "• Find all pairs to complete the level!",

        "wordfinder" => "Find all the hidden words in the letter grid!\n\n"
            + "• Drag your finger across letters to form words\n"
            + "• Words can be horizontal, vertical, or diagonal\n"
            + "• Find every word in the list to complete the level\n"
            + "• Words must be at least 3 letters long\n"
            + "• Use hints if you get stuck!",

        "hanoi" => "Move all discs from the left peg to the right peg.\n\n"
            + "• Tap a peg to pick up the top disc\n"
            + "• Tap another peg to place it\n"
            + "• You can only place a disc on a larger disc or empty peg\n"
            + "• Move all discs to the right peg in the fewest moves!\n"
            + "• Minimum moves for 3 discs = 7, 4 discs = 15, 5 discs = 31",

        "puzzlepets" => "Drag the animal pieces to their matching positions on the grid.\n\n"
            + "• Each piece has a unique animal and color\n"
            + "• Look at the hint outlines to see where each piece belongs\n"
            + "• Match all pieces correctly to complete the puzzle\n"
            + "• Faster completion earns more stars!",

        "mathgame" => "Solve math equations as fast as you can!\n\n"
            + "• An equation with a missing number will appear\n"
            + "• Choose the correct answer from the four choices\n"
            + "• Answer quickly — you have a limited time per level\n"
            + "• Each correct answer scores points\n"
            + "• See how many you can get right!",

        _ => "No rules available for this game."
    };
}
