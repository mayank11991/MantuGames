namespace MantuGames.Models;

public enum LevelState { Locked, Unlocked, Completed }

public class LevelProgress
{
    public int LevelNumber { get; set; }
    public LevelState State { get; set; } = LevelState.Locked;
    public int Stars { get; set; } = 0;  // 0-3, best stars earned

    public bool IsLocked    => State == LevelState.Locked;
    public bool IsUnlocked  => State == LevelState.Unlocked;
    public bool IsCompleted => State == LevelState.Completed;
}
