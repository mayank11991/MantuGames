using MantuGames.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Models;

namespace MantuGames.ViewModels;

public class MathViewModel : INotifyPropertyChanged
{
    private MathPuzzle _puzzle = null!;
    private System.Threading.Timer? _timer;
    private bool _isGameOver, _isWin;
    private int _timeRemainingSec = ProgressService.ClassicTimerSeconds;
    private int _currentLevel;
    private int _currentIndex = 0;
    private int _correctCount = 0;
    private bool _answerRevealed = false;

    public bool SolutionWasShown { get; private set; }

    // Current question data
    private string _questionText = null!;
    private ObservableCollection<ChoiceViewModel> _choices = new();

    public string QuestionText
    {
        get => _questionText;
        set
        {
            _questionText = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ChoiceViewModel> Choices => _choices;

    public string ProgressText => $"{_currentIndex + 1} / {_puzzle?.TotalQuestions}";
    public double ProgressValue => _puzzle == null ? 0 : (double)_currentIndex / _puzzle.TotalQuestions;
    public int CorrectCount => _correctCount;

    private string _progressBarColor = "#FFC83D";
    public string ProgressBarColor
    {
        get => _progressBarColor;
        set { _progressBarColor = value; OnPropertyChanged(); }
    }

    public int TimeRemainingSec
    {
        get => _timeRemainingSec;
        set
        {
            _timeRemainingSec = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimerDisplay));
            OnPropertyChanged(nameof(TimerProgress));
        }
    }

    public string TimerDisplay => $"{TimeRemainingSec / 60}:{TimeRemainingSec % 60:D2}";
    public double TimerProgress => TimeRemainingSec / (double)ProgressService.ClassicTimerSeconds;

    public bool IsGameOver
    {
        get => _isGameOver;
        set
        {
            _isGameOver = value;
            OnPropertyChanged();
        }
    }

    public bool IsWin
    {
        get => _isWin;
        set
        {
            _isWin = value;
            OnPropertyChanged();
        }
    }

    public int CurrentLevel
    {
        get => _currentLevel;
        set
        {
            _currentLevel = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LevelDisplay));
        }
    }

    public string LevelDisplay => $"Level {CurrentLevel}";

    public ICommand AnswerCommand { get; }
    public ICommand ShowSolutionCommand { get; }

    public event Action<bool>? GameEnded;
    public event Action? QuestionChanged;

    public MathViewModel(int level = 1)
    {
        AnswerCommand = new Command<ChoiceViewModel>(Answer, c => !_answerRevealed && !IsGameOver);
        ShowSolutionCommand = new Command(ShowSolution);
        StartLevel(level);
    }

    private void StartLevel(int level)
    {
        CurrentLevel = level;
        TimeRemainingSec = ProgressService.ClassicTimerSeconds;
        IsGameOver = false;
        IsWin = false;
        SolutionWasShown = false;
        _currentIndex = 0;
        _correctCount = 0;
        _answerRevealed = false;

        _puzzle = MathPuzzle.Generate(level);
        LoadQuestion();
        StopTimer();
        StartTimer();
    }

    private void LoadQuestion()
    {
        if (_currentIndex >= _puzzle.TotalQuestions)
        {
            EndGame(true);
            return;
        }

        var q = _puzzle.Questions[_currentIndex];
        QuestionText = q.QuestionText;

        _choices.Clear();
        foreach (var c in q.Choices)
            _choices.Add(new ChoiceViewModel { Value = c, Slot = _choices.Count, State = ChoiceState.Normal });

        _answerRevealed = false;

        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressValue));
        ProgressBarColor = "#FFC83D";
        QuestionChanged?.Invoke();
    }

    private async void Answer(ChoiceViewModel chosen)
    {
        if (_answerRevealed || IsGameOver || chosen == null) return;
        _answerRevealed = true;

        var q = _puzzle.Questions[_currentIndex];
        bool correct = chosen.Value == q.CorrectAnswer;

        // Reveal correct/wrong states
        foreach (var c in _choices)
        {
            if (c.Value == q.CorrectAnswer) c.State = ChoiceState.Correct;
            else if (c == chosen && !correct) c.State = ChoiceState.Wrong;
        }

        if (correct) _correctCount++;
        ProgressBarColor = correct ? "#35D49A" : "#FF5C7A";

        // Wait briefly so player sees the result, then advance
        await System.Threading.Tasks.Task.Delay(800);

        _currentIndex++;
        OnPropertyChanged(nameof(ProgressValue));
        OnPropertyChanged(nameof(ProgressText));
        if (_currentIndex >= _puzzle.TotalQuestions)
        {
            // Win = answered at least 4/5 correctly
            EndGame(_correctCount >= (int)Math.Ceiling(_puzzle.TotalQuestions * 0.8));
        }
        else
        {
            LoadQuestion();
        }
    }

    private void ShowSolution()
    {
        if (IsGameOver) return;
        StopTimer();
        IsGameOver = true;
        SolutionWasShown = true;

        // Reveal correct answer for current question
        var q = _puzzle.Questions[_currentIndex];
        foreach (var c in _choices)
            if (c.Value == q.CorrectAnswer)
                c.State = ChoiceState.Correct;

        IsWin = true;
        GameEnded?.Invoke(true);
    }

    private void StartTimer()
    {
        _timer = new System.Threading.Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (TimeRemainingSec > 0) TimeRemainingSec--;
                else EndGame(false);
            });
        }, null, 1000, 1000);
    }

    public void Cleanup() => StopTimer();
    public void PauseTimer() => StopTimer();
    public void ResumeTimer() => StartTimer();

    private void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private void EndGame(bool win)
    {
        StopTimer();
        IsWin = win;
        IsGameOver = true;
        GameEnded?.Invoke(win);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public enum ChoiceState
{
    Normal,
    Correct,
    Wrong
}

public class ChoiceViewModel : INotifyPropertyChanged
{
    private ChoiceState _state = ChoiceState.Normal;
    public int Value { get; set; }

    /// <summary>Slot in the 2x2 grid; picks the candy colour of the button.</summary>
    public int Slot { get; set; }

    private static readonly string[] Faces = { "#2F8CFF", "#9B6BFF", "#3DC86A", "#FF8A3D" };
    private static readonly string[] Lips = { "#1A62C9", "#6E43D6", "#239448", "#D4611A" };

    public string LipColor => State switch
    {
        ChoiceState.Correct => "#168A50",
        ChoiceState.Wrong => "#C93A55",
        _ => Lips[Slot % Lips.Length]
    };

    public ChoiceState State
    {
        get => _state;
        set
        {
            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BgColor));
            OnPropertyChanged(nameof(TextColor));
            OnPropertyChanged(nameof(LipColor));
        }
    }

    public string BgColor => State switch
    {
        ChoiceState.Correct => "#20B26B",
        ChoiceState.Wrong => "#FF5C7A",
        _ => Faces[Slot % Faces.Length]
    };

    public string TextColor => "#FFFFFF";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}