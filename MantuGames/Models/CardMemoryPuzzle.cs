namespace MantuGames.Models;

public class CardItem
{
    public int    Id        { get; set; }
    public int    PairId    { get; set; }
    public string Image     { get; set; } = "";
    public bool   IsFlipped { get; set; }
    public bool   IsMatched { get; set; }
}

public class CardMemoryPuzzle
{
    public List<CardItem> Cards     { get; private set; } = null!;
    public int            Columns   { get; private set; }
    public int            PairCount { get; private set; }
    public int            TimerSec  { get; private set; }

    private static readonly string[] AnimalPool =
    {
        "cat.png", "elephant.png", "lion.png", "owl.png", "panda.png", "penguin.png",
        "sheep.png", "sloth.png", "dino.png", "hedge.png", "crab.png", "chick.png",
        "chicken.png", "butterfly.png", "bee.png", "bat.png", "octopus.png", "giraffe.png"
    };

    public static CardMemoryPuzzle Generate(int level, IReadOnlyList<string>? pool = null)
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

        // Rotate pool based on level so different levels feel different
        var source = pool is { Count: > 0 } ? pool.ToArray() : AnimalPool;
        int offset = (level - 1) * 3 % source.Length;
        var rotated = source.Skip(offset).Concat(source.Take(offset)).ToList();

        var images = rotated.Take(pairCount).ToList();

        // Create pairs and shuffle
        var cardList = new List<CardItem>();
        for (int i = 0; i < images.Count; i++)
        {
            cardList.Add(new CardItem { Id = i * 2,     PairId = i, Image = images[i] });
            cardList.Add(new CardItem { Id = i * 2 + 1, PairId = i, Image = images[i] });
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
