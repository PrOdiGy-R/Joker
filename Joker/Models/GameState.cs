namespace Joker.Models;

public class GameState
{
    public string[] PlayerNames { get; set; } = new string[4];
    public GameMode Mode { get; set; }
    public PlayType PlayType { get; set; }
    public int CurrentDealerIndex { get; set; }
    public int CurrentSegmentIndex { get; set; }
    public int CurrentRoundIndex { get; set; }
    public List<SegmentData> Segments { get; set; } = new();
    public bool IsCompleted { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public int GetCurrentDealer()
    {
        return CurrentDealerIndex;
    }
    
    public void AdvanceDealer()
    {
        CurrentDealerIndex = (CurrentDealerIndex + 1) % 4;
    }
}
