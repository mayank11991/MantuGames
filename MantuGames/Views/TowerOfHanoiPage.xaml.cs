using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class TowerOfHanoiPage : ContentPage
{
    private HanoiViewModel _vm;
    private int _startLevel = 1;

    // disc size (1=smallest) → color — fixed, never changes during gameplay
    private static readonly Color[] DiscColors =
    {
        Color.FromArgb("#EF5350"), // size 1 smallest = red
        Color.FromArgb("#FF9800"), // size 2 = orange
        Color.FromArgb("#FDD835"), // size 3 = yellow
        Color.FromArgb("#66BB6A"), // size 4 = green
        Color.FromArgb("#42A5F5"), // size 5 largest = blue
    };

    private readonly Grid[] _polePanels = new Grid[3];

    // Tap-to-select state
    private int? _selectedPole = null;
    // Drag source pole (set in DragStarting)
    private int? _dragFromPole = null;

    public string Level
    {
        set { if (int.TryParse(value, out int l)) _startLevel = l; }
    }

    public TowerOfHanoiPage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AudioService.Instance.StartMusic();
        InitVm();
        this.Opacity = 0;
        this.FadeTo(1, 400);
        PauseOverlay.Resumed += OnResumeGame;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseOverlay.Resumed -= OnResumeGame;
        _vm?.Cleanup();
    }

    private void InitVm()
    {
        if (_vm != null)
        {
            _vm.GameEnded -= OnGameEnded;
            _vm.BoardChanged -= OnBoardChanged;
        }

        _vm = new HanoiViewModel(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        _vm.BoardChanged += OnBoardChanged;
        _selectedPole = null;
        _dragFromPole = null;

        BoardGrid.Children.Clear();
        for (int p = 0; p < 3; p++)
        {
            _polePanels[p] = BuildPoleShell(p);
            Grid.SetColumn(_polePanels[p], p);
            BoardGrid.Children.Add(_polePanels[p]);
        }

        this.Loaded += (s, e) => RenderDiscs();
    }

    private void OnBoardChanged() =>
        MainThread.BeginInvokeOnMainThread(RenderDiscs);

    // ── Pause / Resume ──────────────────────────────────────────
    private void OnPause(object sender, EventArgs e)
    {
        try { _vm?.PauseTimer(); } catch { }
        PauseOverlay.Show();
    }

    private void OnResumeGame(object sender, EventArgs e)
    {
        try { _vm?.ResumeTimer(); } catch { }
    }

    private void OnShowSolutionClicked(object sender, EventArgs e)
    {
        if (CoinService.GetCoins("towerofhanoi") < CoinService.SolutionCost)
        {
            CoinShopPopup.ShowForGame("towerofhanoi");
            return;
        }
        CoinService.SpendCoins("towerofhanoi", CoinService.SolutionCost);
        _vm?.ShowSolutionCommand.Execute(null);
    }

    // ── POLE SHELL ───────────────────────────────────────────────
    private Grid BuildPoleShell(int poleIndex)
    {
        var outer = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = 14 },
                new RowDefinition { Height = 20 }
            },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            BackgroundColor = Colors.Transparent
        };

        // Background panel — prevents page background bleeding through corners
        var bgPanel = new Border
        {
            BackgroundColor = Colors.Transparent,
            StrokeThickness = 0,
            Margin = new Thickness(6, 0, 6, 0),
        };
        bgPanel.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 };
        outer.Children.Add(bgPanel);
        Grid.SetRow(bgPanel, 0);
        Grid.SetRowSpan(bgPanel, 2);

        // Rod — 20% of the base bar width, set dynamically on layout
        var rod = new Border
        {
            BackgroundColor = Color.FromArgb("#FACF93"),
            StrokeThickness = 0,
            WidthRequest = 16,
            HeightRequest = 200,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 0, 6),
        };
        rod.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 };
        outer.SizeChanged += (s, e) =>
        {
            if (outer.Width > 0)
                rod.WidthRequest = Math.Max(8, (outer.Width - 12) * 0.2);
        };
        outer.Children.Add(rod);
        Grid.SetRow(rod, 0);

        // Disc stack — VerticalOptions.End so discs pile from bottom
        var discStack = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Fill,
            Spacing = 4,
            Padding = new Thickness(4, 0, 4, 4),
            AutomationId = $"discstack_{poleIndex}"
        };
        outer.Children.Add(discStack);
        Grid.SetRow(discStack, 0);

        // Base bar
        var baseBar = new Border
        {
            BackgroundColor = Color.FromArgb("#FACF93"),
            StrokeThickness = 0,
            HeightRequest = 14,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(6, 0, 6, 0),
            AutomationId = $"base_{poleIndex}"
        };
        baseBar.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 };
        outer.Children.Add(baseBar);
        Grid.SetRow(baseBar, 1);

        // Label
        var lbl = new Label
        {
            Text = poleIndex == _vm.Puzzle.StartPole ? "Start"
                 : poleIndex == _vm.Puzzle.GoalPole ? "Goal"
                 : "Spare",
            FontSize = 12,
            FontFamily = "BrickSans",
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#8D6E63"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            AutomationId = $"label_{poleIndex}"
        };
        outer.Children.Add(lbl);
        Grid.SetRow(lbl, 2);

        // Tap gesture
        int idx = poleIndex;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => OnPoleTapped(idx);
        outer.GestureRecognizers.Add(tap);

        // Drop gesture — accepts discs dragged from other poles
        var drop = new DropGestureRecognizer();
        drop.DragOver += (s, e) => e.AcceptedOperation = DataPackageOperation.Copy;
        drop.Drop += (s, e) =>
        {
            if (_dragFromPole.HasValue)
                OnDrop(_dragFromPole.Value, idx);
        };
        outer.GestureRecognizers.Add(drop);

        return outer;
    }

    // ── RENDER DISCS ─────────────────────────────────────────────
    // GetPileBottomToTop returns [0]=largest(base) … [last]=smallest(top).
    // StackLayout renders top→bottom, so we add smallest first so it appears at visual top.
    private void RenderDiscs()
    {
        for (int p = 0; p < 3; p++)
        {
            StackLayout discStack = null;
            Border baseBar = null;

            foreach (var child in _polePanels[p].Children)
            {
                if (child is StackLayout sl && sl.AutomationId == $"discstack_{p}")
                    discStack = sl;
                if (child is Border b && b.AutomationId == $"base_{p}")
                    baseBar = b;
            }

            if (discStack == null) continue;
            discStack.Children.Clear();

            bool isSelected = _selectedPole == p;

            // Highlight base bar when selected — pole highlight, not disc color change
            if (baseBar != null)
                baseBar.BackgroundColor = isSelected
                    ? Color.FromArgb("#FF9800")
                    : Color.FromArgb("#795548");

            var pile = _vm.Puzzle.GetPileBottomToTop(p);

            // Add smallest first (pile[last]) → renders at visual top
            // Add largest last  (pile[0])    → renders at visual bottom
            for (int i = pile.Count - 1; i >= 0; i--)
            {
                bool isTopDisc = (i == pile.Count - 1);
                var disc = BuildDiscView(pile[i], isTopDisc, isSelected && isTopDisc, p);
                discStack.Children.Add(disc);
            }
        }
    }

    // Disc color is always fixed to the disc's size — never changes on selection or drag.
    // isTopDisc: only the topmost disc on a pole can be dragged or tapped.
    // isSelectedTop: show selection border without altering the background color.
    private Frame BuildDiscView(int discSize, bool isTopDisc, bool isSelectedTop, int poleIndex)
    {
        int idx = Math.Clamp(discSize - 1, 0, DiscColors.Length - 1);
        Color color = DiscColors[idx]; // always the fixed disc color

        // Larger disc = wider. size 1 → ~40% width, size 5 → ~96% width
        double marginEach = (1.0 - (0.38 + (discSize - 1) * 0.145)) * 44.0;

        var frame = new Frame
        {
            BackgroundColor = color,
            BorderColor = isSelectedTop ? Colors.White : Colors.Transparent,
            CornerRadius = 12,
            HasShadow = true,
            Padding = 0,
            HeightRequest = 28,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(Math.Max(0, marginEach), 0)
        };

        if (isTopDisc)
        {
            int fromPole = poleIndex;
            var drag = new DragGestureRecognizer();
            drag.DragStarting += (s, e) =>
            {
                _dragFromPole = fromPole;
                // Clear tap selection when drag begins
                _selectedPole = null;
                e.Data.Properties["fromPole"] = fromPole;
            };
            drag.DropCompleted += (s, e) =>
            {
                _dragFromPole = null;
                RenderDiscs();
            };
            frame.GestureRecognizers.Add(drag);
        }

        return frame;
    }

    // ── TAP HANDLER ──────────────────────────────────────────────
    private async void OnPoleTapped(int poleIndex)
    {
        try
        {
            if (_vm.IsGameOver) return;

            if (_selectedPole == null)
            {
                if (_vm.Puzzle.Poles[poleIndex].Count == 0) return;
                _selectedPole = poleIndex;
                RenderDiscs();
                AudioService.Instance.Play("pop");
                await AnimateBounce(_polePanels[poleIndex]);
            }
            else if (_selectedPole == poleIndex)
            {
                _selectedPole = null;
                RenderDiscs();
            }
            else
            {
                await ExecuteMove(_selectedPole.Value, poleIndex);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnPoleTapped: {ex.Message}");
        }
    }

    // ── DROP HANDLER ─────────────────────────────────────────────
    private async void OnDrop(int fromPole, int toPole)
    {
        try
        {
            if (_vm.IsGameOver) return;
            if (fromPole == toPole) return;
            _selectedPole = null;
            await ExecuteMove(fromPole, toPole);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnDrop: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task ExecuteMove(int from, int to)
    {
        bool moved = _vm.Puzzle.TryMove(from, to);
        _vm.SyncMoves();
        _selectedPole = null;

        if (moved)
            AudioService.Instance.Play("move");
        else
            AudioService.Instance.Play("bump");

        if (!moved)
            await AnimateShake(_polePanels[to]);

        RenderDiscs();

        if (_vm.Puzzle.IsSolved())
            _vm.TriggerWin();
    }

    private async System.Threading.Tasks.Task AnimateBounce(View v)
    {
        await v.ScaleTo(1.06, 80, Easing.CubicOut);
        await v.ScaleTo(1.00, 80, Easing.CubicIn);
    }

    private async System.Threading.Tasks.Task AnimateShake(View v)
    {
        await v.TranslateTo(-8, 0, 50);
        await v.TranslateTo(8, 0, 50);
        await v.TranslateTo(-4, 0, 40);
        await v.TranslateTo(0, 0, 40);
    }

    // ── EVENTS ───────────────────────────────────────────────────
    private async void OnGameEnded(bool isWin)
    {
        try
        {
            int elapsed = _vm.TotalTimerSec - _vm.TimeRemainingSec;
            string reason = _vm.SolutionWasShown ? "Solution Shown" : null;
            int stars  = isWin && reason == null ? ProgressService.CalcStars(elapsed, _vm.TotalTimerSec) : 0;
            int coins = isWin && reason == null ? stars switch { 3 => 5, 2 => 3, 1 => 1, _ => 0 } : 0;
            await ResultPopup.Show(isWin, _vm.CurrentLevel, elapsed, _vm.TotalTimerSec, stars, coins, reason, "towerofhanoi");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnGameEnded: {ex.Message}");
        }
    }

    private void OnNextLevel(object sender, EventArgs e)
    {
        _startLevel = _vm.CurrentLevel + 1;
        InitVm();
    }

    private void OnRetry(object sender, EventArgs e) => InitVm();

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave", "Stay");
            if (!leave) return;
            _vm?.Cleanup();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnBackClicked: {ex.Message}");
        }
    }
}
