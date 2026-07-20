namespace MantuGames.Models;

public class CardItem
{
    public int    Id        { get; set; }
    public string Emoji     { get; set; }
    public bool   IsFlipped { get; set; }
    public bool   IsMatched { get; set; }
}

public class CardMemoryPuzzle
{
    public List<CardItem> Cards     { get; private set; }
    public int            Columns   { get; private set; }
    public int            PairCount { get; private set; }
    public int            TimerSec  { get; private set; }

    private static readonly string[] Tier1 = { "🐶","🐱","🐭","🐹","🐰","🦊","🐻","🐼","🐨","🐯" };
    private static readonly string[] Tier2 = { "🍕","🍔","🌮","🍦","🎂","🍩","🍎","🍊","🍋","🍇","🍓","🍒" };
    private static readonly string[] Tier3 = { "🚀","🚗","✈️","🚢","🚁","🚂","🏠","🏰","⚽","🎸","🎨","🎮","🎯","💎","🌈" };

    public static CardMemoryPuzzle Generate(int level)
    {
        int pairCount, cols, timerSec;

        if (level <= 5)
        {
            pairCount = 6;
            cols      = 3;
            timerSec  = 60;
        }
        else if (level <= 12)
        {
            pairCount = 8;
            cols      = 4;
            timerSec  = 90;
        }
        else
        {
            pairCount = 10;
            cols      = 4;
            timerSec  = 120;
        }

        // Build a combined emoji pool, cycle through tiers based on level
        var pool = new List<string>();
        pool.AddRange(Tier1);
        pool.AddRange(Tier2);
        pool.AddRange(Tier3);

        // Rotate pool based on level so different levels feel different
        int offset = (level - 1) * 3 % pool.Count;
        var rotated = pool.Skip(offset).Concat(pool.Take(offset)).ToList();

        var emojis = rotated.Take(pairCount).ToList();

        // Create pairs and shuffle
        var cardList = new List<CardItem>();
        for (int i = 0; i < emojis.Count; i++)
        {
            cardList.Add(new CardItem { Id = i * 2,     Emoji = emojis[i] });
            cardList.Add(new CardItem { Id = i * 2 + 1, Emoji = emojis[i] });
        }

        var rng = new Random(level * 7919 + Environment.TickCount);
        for (int i = cardList.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (cardList[i], cardList[j]) = (cardList[j], cardList[i]);
        }

        return new CardMemoryPuzzle
        {
            Cards     = cardList,
            Columns   = cols,
            PairCount = pairCount,
            TimerSec  = timerSec
        };
    }
}
