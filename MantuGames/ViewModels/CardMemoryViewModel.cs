using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Models;

namespace MantuGames.ViewModels;

public class CardViewModel : INotifyPropertyChanged
{
    private bool _isFlipped;
    private bool _isMatched;

    public int    Id     { get; set; }
    public int    PairId { get; set; }
    public string Image  { get; set; }

    public bool IsFlipped
    {
        get => _isFlipped;
        set { _isFlipped = value; OnPropertyChanged(); }
    }

    public bool IsMatched
    {
        get => _isMatched;
        set { _isMatched = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class CardMemoryViewModel : INotifyPropertyChanged
{
    private System.Threading.Timer _timer;
    private bool _isGameOver;
    private int  _timeRemainingSec;
    private int  _currentLevel;
    private int  _matchedPairs;
    private int  _totalSecondsForLevel;
    private int  _columns;
    private int  _pairCount;

    private readonly List<CardViewModel> _flipped  = new();
    private bool _isChecking;

    public ObservableCollection<CardViewModel> Cards { get; } = new();

    public int Columns
    {
        get => _columns;
        private set { _columns = value; OnPropertyChanged(); }
    }

    public int CurrentLevel
    {
        get => _currentLevel;
        private set { _currentLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(LevelDisplay)); }
    }

    public int TimeRemainingSec
    {
        get => _timeRemainingSec;
        set { _timeRemainingSec = value; OnPropertyChanged(); }
    }

    public int TotalSecondsForLevel
    {
        get => _totalSecondsForLevel;
        private set { _totalSecondsForLevel = value; OnPropertyChanged(); }
    }

    public int MatchedPairs
    {
        get => _matchedPairs;
        private set { _matchedPairs = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProgressText)); }
    }

    public bool IsGameOver
    {
        get => _isGameOver;
        private set { _isGameOver = value; OnPropertyChanged(); }
    }

    public string LevelDisplay  => $"Level {CurrentLevel}";
    public string ProgressText  => $"{MatchedPairs}/{_pairCount} pairs";

    public ICommand FlipCommand { get; }

    public event Action<bool> GameEnded;

    public CardMemoryViewModel(int level = 1)
    {
        FlipCommand = new Command<CardViewModel>(OnFlip);
        StartLevel(level);
    }

    public void StartLevel(int level)
    {
        StopTimer();
        _isChecking  = false;
        _flipped.Clear();
        IsGameOver   = false;
        MatchedPairs = 0;

        var puzzle = CardMemoryPuzzle.Generate(level);
        CurrentLevel          = level;
        Columns               = puzzle.Columns;
        _pairCount            = puzzle.PairCount;
        TotalSecondsForLevel  = puzzle.TimerSec;
        TimeRemainingSec      = puzzle.TimerSec;

        Cards.Clear();
        foreach (var item in puzzle.Cards)
        {
            Cards.Add(new CardViewModel
            {
                Id       = item.Id,
                PairId   = item.PairId,
                Image    = item.Image,
                IsFlipped = false,
                IsMatched = false
            });
        }

        StartTimer();
    }

    private async void OnFlip(CardViewModel card)
    {
        if (card == null || IsGameOver || _isChecking) return;
        if (card.IsFlipped || card.IsMatched) return;
        if (_flipped.Count >= 2) return;

        card.IsFlipped = true;
        _flipped.Add(card);

        if (_flipped.Count < 2) return;

        // Two cards flipped — check match after delay
        _isChecking = true;

        await Task.Delay(700);

        var first  = _flipped[0];
        var second = _flipped[1];
        _flipped.Clear();

        if (first.PairId == second.PairId)
        {
            first.IsMatched  = true;
            second.IsMatched = true;
            MatchedPairs++;

            if (MatchedPairs >= _pairCount)
                EndGame(true);
        }
        else
        {
            first.IsFlipped  = false;
            second.IsFlipped = false;
        }

        _isChecking = false;
    }

    private void StartTimer()
    {
        _timer = new System.Threading.Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (TimeRemainingSec > 0)
                    TimeRemainingSec--;
                else
                    EndGame(false);
            });
        }, null, 1000, 1000);
    }

    private void EndGame(bool win)
    {
        StopTimer();
        IsGameOver = true;
        GameEnded?.Invoke(win);
    }

    /// <summary>Flips every unmatched card face up (used by Show Solution).</summary>
    public void RevealAll()
    {
        foreach (var card in Cards)
            if (!card.IsMatched)
                card.IsFlipped = true;
    }

    /// <summary>Forces the game to end (used by Show Solution).</summary>
    public void ForceEnd(bool win)
    {
        if (IsGameOver) return;
        EndGame(win);
    }

    private void StopTimer() { _timer?.Dispose(); _timer = null; }

    public void Cleanup() => StopTimer();
    public void PauseTimer() => StopTimer();
    public void ResumeTimer() => StartTimer();

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
