namespace MantuGames.Helpers;

public static class GameRules
{
    public static string GetRules(string gameId) => gameId switch
    {
        "mazerunner" => "Navigate the cat through the maze to reach the destination! Use the arrow buttons or swipe on the maze to move. Avoid walls — you cannot walk through them. Reach the golden exit to complete the level. Move fast to earn more stars!",

        "sudoku" => "Fill the 5×5 grid so each row, column, and bold region contains every animal exactly once. Tap an animal button, then tap an empty cell to place it. Use ⌫ to erase a placed animal. Each animal can appear only once per row, column, and region. Complete the puzzle to earn stars!",

        "blockpuzzle" => "Arrange falling blocks to complete horizontal lines. Swipe left/right to move the falling piece. Tap to rotate the piece. Swipe down to drop it faster. Complete a row to clear it and earn coins. The game ends when blocks reach the top!",

        "cardmemory" => "Flip cards to find matching animal pairs! Tap a card to flip it and reveal the animal. Tap another card to find its match. Matching pairs stay revealed. Mismatched cards flip back face-down. Find all pairs to complete the level!",

        "wordfinder" => "Find all the hidden words in the letter grid! Drag your finger across letters to form words. Words can be horizontal, vertical, or diagonal. Find every word in the list to complete the level. Words must be at least 3 letters long. Use hints if you get stuck!",

        "towerofhanoi" => "Move all discs from the left peg to the right peg. Tap a peg to pick up the top disc. Tap another peg to place it. You can only place a disc on a larger disc or empty peg. Move all discs to the right peg in the fewest moves! Minimum moves for 3 discs = 7, 4 discs = 15, 5 discs = 31.",

        "puzzlepets" => "Drag the animal pieces to their matching positions on the grid. Each piece has a unique animal and color. Look at the hint outlines to see where each piece belongs. Match all pieces correctly to complete the puzzle. Faster completion earns more stars!",

        "mathchallenge" => "Solve math equations as fast as you can! An equation with a missing number will appear. Choose the correct answer from the four choices. Answer quickly — you have a limited time per level. Each correct answer earns coins. See how many you can get right!",

        _ => "Welcome to this game! Tap a level to start playing. Complete each level to earn stars and unlock new challenges. Have fun!"
    };
}
