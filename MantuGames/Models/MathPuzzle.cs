namespace MantuGames.Models;

public class MathQuestion
{
    public string QuestionText { get; set; }
    public int CorrectAnswer { get; set; }
    public List<int> Choices { get; set; }
}

public class MathPuzzle
{
    public List<MathQuestion> Questions { get; } = new();
    public int TotalQuestions => Questions.Count;

    private static readonly Random _rng = new();
    private const int QuestionsPerLevel = 5;

    public static MathPuzzle Generate(int level)
    {
        var p = new MathPuzzle();
        p.Build(level);
        return p;
    }

    private void Build(int level)
    {
        for (int i = 0; i < QuestionsPerLevel; i++)
            Questions.Add(GenerateQuestion(level));
    }

    private MathQuestion GenerateQuestion(int level)
    {
        // Tier definitions — each tier mixes single AND double digits
        // as level climbs within the tier
        //
        // Level  1-2 : +  -          numbers  1–9
        // Level  3-5 : +  -          numbers  1–19  (intro double digits)
        // Level  6-8 : +  -  ×       numbers  2–20, ×  up to 9×9
        // Level  9-12: +  -  ×       numbers  5–50, ×  up to 12×12
        // Level 13-16: +  -  ×  ÷   numbers  5–50, ÷  up to 12×12
        // Level 17-20: +  -  ×  ÷   numbers 10–99, ÷  up to 15×15
        // Level 21+  : +  -  ×  ÷   numbers 10–99, larger multipliers

        string op;
        int a, b, answer;

        if (level <= 2)
        {
            op = _rng.Next(2) == 0 ? "+" : "-";
            a = _rng.Next(1, 10);
            b = op == "-" ? _rng.Next(1, a + 1) : _rng.Next(1, 10);
        }
        else if (level <= 5)
        {
            op = _rng.Next(2) == 0 ? "+" : "-";
            // Mix: ~50% single digit, ~50% double digit
            int max = level <= 3 ? 15 : 20;
            a = _rng.Next(2, max);
            b = op == "-" ? _rng.Next(1, a + 1) : _rng.Next(2, max);
        }
        else if (level <= 8)
        {
            var ops = new[] { "+", "+", "-", "×" };
            op = ops[_rng.Next(ops.Length)];
            a = _rng.Next(2, 21);
            b = op == "×" ? _rng.Next(2, 10)
                : op == "-" ? _rng.Next(1, a + 1)
                : _rng.Next(2, 21);
        }
        else if (level <= 12)
        {
            var ops = new[] { "+", "-", "×", "×" };
            op = ops[_rng.Next(ops.Length)];
            a = _rng.Next(5, 51);
            b = op == "×" ? _rng.Next(2, 13)
                : op == "-" ? _rng.Next(1, a + 1)
                : _rng.Next(5, 51);
        }
        else if (level <= 16)
        {
            var ops = new[] { "+", "-", "×", "÷" };
            op = ops[_rng.Next(ops.Length)];
            if (op == "÷")
            {
                b = _rng.Next(2, 13);
                answer = _rng.Next(2, 13);
                a = b * answer;
                return MakeQuestion($"{a} ÷ {b}", answer);
            }

            a = _rng.Next(5, 51);
            b = op == "×" ? _rng.Next(2, 13)
                : op == "-" ? _rng.Next(1, a + 1)
                : _rng.Next(5, 51);
        }
        else if (level <= 20)
        {
            var ops = new[] { "+", "-", "×", "÷" };
            op = ops[_rng.Next(ops.Length)];
            if (op == "÷")
            {
                b = _rng.Next(2, 16);
                answer = _rng.Next(2, 16);
                a = b * answer;
                return MakeQuestion($"{a} ÷ {b}", answer);
            }

            a = _rng.Next(10, 100);
            b = op == "×" ? _rng.Next(2, 13)
                : op == "-" ? _rng.Next(1, a + 1)
                : _rng.Next(10, 100);
        }
        else
        {
            // Level 21+ — fully double-digit everything
            var ops = new[] { "+", "-", "×", "÷" };
            op = ops[_rng.Next(ops.Length)];
            if (op == "÷")
            {
                b = _rng.Next(3, 16);
                answer = _rng.Next(3, 16);
                a = b * answer;
                return MakeQuestion($"{a} ÷ {b}", answer);
            }

            a = _rng.Next(10, 100);
            b = op == "×" ? _rng.Next(11, 20)
                : op == "-" ? _rng.Next(1, a + 1)
                : _rng.Next(10, 100);
        }

        answer = op switch
        {
            "+" => a + b,
            "-" => a - b,
            "×" => a * b,
            _ => a / b
        };

        return MakeQuestion($"{a} {op} {b}", answer);
    }

    private MathQuestion MakeQuestion(string text, int answer)
    {
        var choices = new HashSet<int> { answer };

        // Plausible distractors — scale spread with answer size
        int spread = Math.Max(4, Math.Abs(answer) / 4 + 3);
        int attempts = 0;
        while (choices.Count < 4 && attempts++ < 200)
        {
            int delta = _rng.Next(-spread, spread + 1);
            int wrong = answer + delta;
            if (wrong != answer && wrong >= 0)
                choices.Add(wrong);
        }

        for (int offset = 1; choices.Count < 4; offset++)
            choices.Add(answer + offset);

        return new MathQuestion
        {
            QuestionText = $"{text} = ?",
            CorrectAnswer = answer,
            Choices = choices.OrderBy(_ => _rng.Next()).ToList()
        };
    }
}