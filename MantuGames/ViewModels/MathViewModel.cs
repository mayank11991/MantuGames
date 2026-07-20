using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Models;

namespace MantuGames.ViewModels;

public class MathViewModel : INotifyPropertyChanged
{
    private MathPuzzle _puzzle;
    private System.Threading.Timer _timer;
    private bool _isGameOver, _isWin;
    private int _timeRemainingSec = 210;
    private int _currentLevel;
    private int _currentIndex = 0;
    private int _correctCount = 0;
    private int? _selectedChoice = null;
    private bool _answerRevealed = false;

    public bool SolutionWasShown { get; private set; }

    // Current question data
    private string _questionText;
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

    private string _progressBarColor = "#FF8A65";
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
    public double TimerProgress => TimeRemainingSec / 210.0;

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

    public event Action<bool> GameEnded;
    public event Action QuestionChanged;

    public MathViewModel(int level = 1)
    {
        AnswerCommand = new Command<ChoiceViewModel>(Answer, c => !_answerRevealed && !IsGameOver);
        ShowSolutionCommand = new Command(ShowSolution);
        StartLevel(level);
    }

    private void StartLevel(int level)
    {
        CurrentLevel = level;
        TimeRemainingSec = 210;
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
            _choices.Add(new ChoiceViewModel { Value = c, State = ChoiceState.Normal });

        _answerRevealed = false;
        _selectedChoice = null;

        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressValue));
        ProgressBarColor = "#FF8A65";
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
        ProgressBarColor = correct ? "#4CAF50" : "#EF5350";

        // Wait briefly so player sees the result, then advance
        await System.Threading.Tasks.Task.Delay(800);

        _currentIndex++;
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

        IsWin = false;
        GameEnded?.Invoke(false);
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string n = null) =>
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

    public ChoiceState State
    {
        get => _state;
        set
        {
            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BgColor));
            OnPropertyChanged(nameof(TextColor));
        }
    }

    public string BgColor => State switch
    {
        ChoiceState.Correct => "#4CAF50",
        ChoiceState.Wrong => "#F44336",
        _ => "#7C4DFF"
    };

    public string TextColor => "#FFFFFF";

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}