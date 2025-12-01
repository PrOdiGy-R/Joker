namespace Joker.Models;

public class GameHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string[] PlayerNames { get; set; } = new string[4];
    public GameMode Mode { get; set; }
    public PlayType PlayType { get; set; }
    public int[] FinalScores { get; set; } = new int[4];
    public DateTime CompletedAt { get; set; }
    public DateTime StartedAt { get; set; }
    
    // Store complete game data for viewing scoreboard
    public List<SegmentData> Segments { get; set; } = new();
}
