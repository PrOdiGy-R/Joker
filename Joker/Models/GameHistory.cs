   namespace Joker.Models;

public class GameHistory
{
    public string[] PlayerNames { get; set; } = new string[4];
    public GameMode Mode { get; set; }
    public PlayType PlayType { get; set; }
    public int[] FinalScores { get; set; } = new int[4];
    public DateTime CompletedAt { get; set; }
}
