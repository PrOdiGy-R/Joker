namespace Joker.Models;

public class SegmentData
{
    public int SegmentNumber { get; set; }
    public List<RoundData> Rounds { get; set; } = new();
    public int[] Subtotals { get; set; } = new int[4];
    public int[] BonusAdjustments { get; set; } = new int[4];
}
