namespace Joker.Models;

public class RoundData
{
    public int CardsDealt { get; set; }
    public int[] Bids { get; set; } = new int[4];
    public int?[] ActualTricks { get; set; } = new int?[4];
    public int[] Scores { get; set; } = new int[4];
    public bool IsCompleted => ActualTricks.All(x => x.HasValue);
    
    // Track which players have bonuses/deductions applied in this round
    public bool[] HasBonus { get; set; } = new bool[4];
    public bool[] HasDeduction { get; set; } = new bool[4];
    
    // Store original scores before deduction for display purposes
    public int[] OriginalScores { get; set; } = new int[4];
}
