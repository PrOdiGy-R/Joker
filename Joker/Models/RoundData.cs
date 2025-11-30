namespace Joker.Models;

public class RoundData
{
    public int CardsDealt { get; set; }
    public int[] Bids { get; set; } = new int[4];
    public int?[] ActualTricks { get; set; } = new int?[4];
    public int[] Scores { get; set; } = new int[4];
    public bool IsCompleted => ActualTricks.All(x => x.HasValue);
}
