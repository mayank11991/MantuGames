namespace MantuGames.Models;

public class MathQuestion
{
    public string QuestionText { get; set; } = "";
    public int CorrectAnswer { get; set; }
    public List<int> Choices { get; set; } = null!;
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

    // ── QUESTION GENERATOR ────────────────────────────────────────
    private MathQuestion GenerateQuestion(int level)
    {
        // Level 1-2 : Simple arithmetic only
        // Level 3-4 : Arithmetic + pattern questions
        // Level 5-7 : Arithmetic + patterns + funny word problems
        // Level 8-10: Arithmetic + patterns + word problems + logic
        // Level 11+: Everything, harder numbers

        int totalTypes = level <= 2 ? 1
                       : level <= 4 ? 2
                       : level <= 7 ? 3
                       : 4;

        int type = _rng.Next(totalTypes);

        return type switch
        {
            0 => GenerateArithmetic(level),
            1 => GeneratePattern(level),
            2 => GenerateFunnyWordProblem(level),
            3 => GenerateLogicPuzzle(level),
            _ => GenerateArithmetic(level)
        };
    }

    // ── TYPE 0: ARITHMETIC (original) ─────────────────────────────
    private MathQuestion GenerateArithmetic(int level)
    {
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
            int max = level <= 3 ? 15 : 20;
            a = _rng.Next(2, max);
            b = op == "-" ? _rng.Next(1, a + 1) : _rng.Next(2, max);
        }
        else if (level <= 7)
        {
            var ops = new[] { "+", "+", "-", "×" };
            op = ops[_rng.Next(ops.Length)];
            a = _rng.Next(2, 21);
            b = op == "×" ? _rng.Next(2, 10)
                : op == "-" ? _rng.Next(1, a + 1)
                : _rng.Next(2, 21);
        }
        else if (level <= 10)
        {
            var ops = new[] { "+", "-", "×", "×" };
            op = ops[_rng.Next(ops.Length)];
            a = _rng.Next(5, 51);
            b = op == "×" ? _rng.Next(2, 13)
                : op == "-" ? _rng.Next(1, a + 1)
                : _rng.Next(5, 51);
        }
        else if (level <= 14)
        {
            var ops = new[] { "+", "-", "×", "÷" };
            op = ops[_rng.Next(ops.Length)];
            if (op == "÷")
            {
                b = _rng.Next(2, 13);
                answer = _rng.Next(2, 13);
                a = b * answer;
                return MakeQuestion($"{a} ÷ {b} = ?", answer);
            }
            a = _rng.Next(5, 51);
            b = op == "×" ? _rng.Next(2, 13)
                : op == "-" ? _rng.Next(1, a + 1)
                : _rng.Next(5, 51);
        }
        else
        {
            var ops = new[] { "+", "-", "×", "÷" };
            op = ops[_rng.Next(ops.Length)];
            if (op == "÷")
            {
                b = _rng.Next(3, 16);
                answer = _rng.Next(3, 16);
                a = b * answer;
                return MakeQuestion($"{a} ÷ {b} = ?", answer);
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

        return MakeQuestion($"{a} {op} {b} = ?", answer);
    }

    // ── TYPE 1: PATTERN RECOGNITION ───────────────────────────────
    private MathQuestion GeneratePattern(int level)
    {
        int patternType = _rng.Next(level <= 5 ? 3 : 5);

        return patternType switch
        {
            0 => PatternAddConstant(level),
            1 => PatternAddIncreasing(level),
            2 => PatternMultiply(level),
            3 => PatternSkipCount(level),
            _ => PatternFibonacci(level)
        };
    }

    private MathQuestion PatternAddConstant(int level)
    {
        int start = _rng.Next(1, level <= 5 ? 20 : 50);
        int step = _rng.Next(2, level <= 5 ? 6 : 12);
        // Show first 4 terms, ask for 5th
        int answer = start + 4 * step;
        return MakeQuestion($"{start}, {start + step}, {start + 2 * step}, {start + 3 * step}, ?  = ?", answer);
    }

    private MathQuestion PatternAddIncreasing(int level)
    {
        int start = _rng.Next(1, 10);
        int inc = _rng.Next(1, level <= 5 ? 4 : 7);
        // Diffs increase: inc, inc+1, inc+2, inc+3
        int t1 = start;
        int t2 = t1 + inc;
        int t3 = t2 + inc + 1;
        int t4 = t3 + inc + 2;
        int answer = t4 + inc + 3;
        return MakeQuestion($"{t1}, {t2}, {t3}, {t4}, ?  = ?", answer);
    }

    private MathQuestion PatternMultiply(int level)
    {
        int base_ = _rng.Next(2, level <= 5 ? 5 : 8);
        int mult = _rng.Next(2, level <= 5 ? 4 : 6);
        // Show first 4 terms, ask for 5th
        int t1 = base_;
        int t2 = base_ * mult;
        int t3 = base_ * mult * mult;
        int t4 = base_ * mult * mult * mult;
        int answer = base_ * mult * mult * mult * mult;
        return MakeQuestion($"{t1}, {t2}, {t3}, {t4}, ?  = ?", answer);
    }

    private MathQuestion PatternSkipCount(int level)
    {
        int start = _rng.Next(2, 10);
        int skip = _rng.Next(2, level <= 5 ? 5 : 8);
        int answer = start + 4 * skip;
        return MakeQuestion($"{start}, {start + skip}, {start + 2 * skip}, {start + 3 * skip}, ?  = ?", answer);
    }

    private MathQuestion PatternFibonacci(int level)
    {
        int a = _rng.Next(1, 5);
        int b = _rng.Next(1, 5);
        // seq: a, b, a+b, a+2b, 2a+3b, 3a+5b...
        int t1 = a;
        int t2 = b;
        int t3 = a + b;
        int t4 = a + 2 * b;
        int answer = 2 * a + 3 * b;
        return MakeQuestion($"{t1}, {t2}, {t3}, {t4}, ?  = ?", answer);
    }

    // ── TYPE 2: FUNNY WORD PROBLEMS ───────────────────────────────
    private MathQuestion GenerateFunnyWordProblem(int level)
    {
        int type = _rng.Next(level <= 7 ? 5 : 8);

        return type switch
        {
            0 => FunnyPizza(level),
            1 => FunnyCatMice(level),
            2 => FunnyHomework(level),
            3 => FunnyFingers(level),
            4 => FunnyBanana(level),
            5 => FunnyAge(level),
            6 => FunnyBus(level),
            _ => FunnyEggs(level)
        };
    }

    private MathQuestion FunnyPizza(int level)
    {
        int slices = _rng.Next(4, 9);
        int friends = _rng.Next(2, slices);
        int answer = slices / friends;
        return MakeQuestion($"Pizza has {slices} slices. You share equally among {friends} friends. Each gets?", answer);
    }

    private MathQuestion FunnyCatMice(int level)
    {
        int caught = _rng.Next(2, 8);
        int ran = _rng.Next(1, 6);
        return MakeQuestion($"A cat caught {caught} mice and {ran} ran away. How many mice is the cat holding?", caught);
    }

    private MathQuestion FunnyHomework(int level)
    {
        int total = _rng.Next(10, 30);
        int done = _rng.Next(3, total);
        return MakeQuestion($"You have {total} homework problems. You did {done}. How many left?", total - done);
    }

    private MathQuestion FunnyFingers(int level)
    {
        int hands = _rng.Next(1, 4);
        int extra = _rng.Next(0, 5);
        return MakeQuestion($"{hands} hands + {extra} extra fingers = ? fingers total", hands * 5 + extra);
    }

    private MathQuestion FunnyBanana(int level)
    {
        int bought = _rng.Next(3, 12);
        int ate = _rng.Next(1, bought);
        return MakeQuestion($"You bought {bought} bananas and ate {ate}. How many are left?", bought - ate);
    }

    private MathQuestion FunnyAge(int level)
    {
        int child = _rng.Next(5, 12);
        int diff = _rng.Next(20, 35);
        return MakeQuestion($"A kid is {child} years old. Dad is {diff} years older. Dad's age?", child + diff);
    }

    private MathQuestion FunnyBus(int level)
    {
        int on1 = _rng.Next(3, 10);
        int off1 = _rng.Next(1, on1);
        int on2 = _rng.Next(2, 8);
        int total = on1 - off1 + on2;
        return MakeQuestion($"{on1} people board a bus. {off1} get off. {on2} board. How many on bus?", total);
    }

    private MathQuestion FunnyEggs(int level)
    {
        int cartons = _rng.Next(2, 5);
        int per = 6;
        int broken = _rng.Next(1, 4);
        return MakeQuestion($"{cartons} egg cartons with {per} each. {broken} broken. Good eggs?", cartons * per - broken);
    }

    // ── TYPE 3: LOGIC / REASONING ─────────────────────────────────
    private MathQuestion GenerateLogicPuzzle(int level)
    {
        int type = _rng.Next(level <= 10 ? 5 : 7);

        return type switch
        {
            0 => LogicMissingNumber(level),
            1 => LogicCompare(level),
            2 => LogicSequence(level),
            3 => LogicTrueFalse(level),
            4 => LogicWordRiddle(level),
            5 => LogicMultiStep(level),
            _ => LogicNumberTrick(level)
        };
    }

    private MathQuestion LogicMissingNumber(int level)
    {
        int a = _rng.Next(2, level <= 10 ? 15 : 30);
        int b = _rng.Next(2, level <= 10 ? 15 : 30);
        int answer = a + b;
        int blank = _rng.Next(3);
        string text = blank switch
        {
            0 => $"? + {b} = {answer}  ? =",
            1 => $"{a} + ? = {answer}  ? =",
            _ => $"{a} + {b} = ?  ? ="
        };
        return MakeQuestion(text, answer);
    }

    private MathQuestion LogicCompare(int level)
    {
        int a = _rng.Next(2, 20);
        int b = _rng.Next(2, 20);
        while (b == a) b = _rng.Next(2, 20);
        int diff = Math.Abs(a - b);
        string text = a > b
            ? $"{a} is how much more than {b}?"
            : $"{b} is how much more than {a}?";
        return MakeQuestion(text, diff);
    }

    private MathQuestion LogicSequence(int level)
    {
        int a = _rng.Next(2, 10);
        int b = _rng.Next(a + 1, a + 10);
        int mid = (a + b) / 2;
        return MakeQuestion($"If A + B = {a + b} and A - B = {b - a}. What is A?", b);
    }

    private MathQuestion LogicTrueFalse(int level)
    {
        int x = _rng.Next(3, 20);
        int y = _rng.Next(3, 20);
        bool first = _rng.Next(2) == 0;
        string text;
        int answer;
        if (first)
        {
            int wrong = x + y + _rng.Next(1, 5);
            text = $"Which is correct:  {x}+{y}={x + y}  or  {x}+{y}={wrong}?  (type the correct sum)";
            answer = x + y;
        }
        else
        {
            int wrong = x * y + _rng.Next(1, 5);
            text = $"Which is correct:  {x}×{y}={x * y}  or  {x}×{y}={wrong}?  (type the correct product)";
            answer = x * y;
        }
        return MakeQuestion(text, answer);
    }

    private MathQuestion LogicWordRiddle(int level)
    {
        int type = _rng.Next(4);
        return type switch
        {
            0 => RiddleDozen(),
            1 => RiddleScore(),
            2 => RiddleGross(),
            _ => RiddleBakerDozen()
        };
    }

    private MathQuestion RiddleDozen()
    {
        int dozens = _rng.Next(2, 6);
        return MakeQuestion($"{dozens} dozen eggs = ? eggs", dozens * 12);
    }

    private MathQuestion RiddleScore()
    {
        int scores = _rng.Next(1, 5);
        return MakeQuestion($"{scores} score years = ? years (1 score = 20)", scores * 20);
    }

    private MathQuestion RiddleGross()
    {
        int gross = _rng.Next(1, 4);
        return MakeQuestion($"{gross} gross of items = ? items (1 gross = 144)", gross * 144);
    }

    private MathQuestion RiddleBakerDozen()
    {
        int bd = _rng.Next(2, 6);
        return MakeQuestion($"{bd} baker's dozen = ? items (baker's dozen = 13)", bd * 13);
    }

    private MathQuestion LogicMultiStep(int level)
    {
        int a = _rng.Next(3, 15);
        int b = _rng.Next(2, 10);
        int c = _rng.Next(1, 8);
        int answer = a * b + c;
        return MakeQuestion($"{a} × {b} + {c} = ?", answer);
    }

    private MathQuestion LogicNumberTrick(int level)
    {
        int x = _rng.Next(5, 50);
        return MakeQuestion($"Double {x}, then add 10. Result?", x * 2 + 10);
    }

    // ── HELPER ────────────────────────────────────────────────────
    private MathQuestion MakeQuestion(string text, int answer)
    {
        var choices = new HashSet<int> { answer };

        // Smart distractors: common mistakes
        int[] candidates = {
            answer + 1, answer - 1,
            answer + 2, answer - 2,
            answer + 5, answer - 5,
            answer * 2,
            answer > 1 ? answer / 2 : answer + 10,
            answer + 3, answer - 3,
            answer + 10, answer - 10,
            answer + 7, answer - 7,
        };

        // Shuffle candidates and pick valid ones
        var shuffled = candidates.OrderBy(_ => _rng.Next()).ToArray();
        foreach (int c in shuffled)
        {
            if (choices.Count >= 4) break;
            if (c != answer && c >= 0)
                choices.Add(c);
        }

        // Fallback: random offsets
        int spread = Math.Max(4, Math.Abs(answer) / 4 + 3);
        int attempts = 0;
        while (choices.Count < 4 && attempts++ < 100)
        {
            int delta = _rng.Next(-spread, spread + 1);
            int wrong = answer + delta;
            if (wrong != answer && wrong >= 0)
                choices.Add(wrong);
        }

        // Last resort
        for (int offset = 1; choices.Count < 4; offset++)
            choices.Add(answer + offset);

        return new MathQuestion
        {
            QuestionText = text,
            CorrectAnswer = answer,
            Choices = choices.OrderBy(_ => _rng.Next()).ToList()
        };
    }
}
