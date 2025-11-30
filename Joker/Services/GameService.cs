using Joker.Models;
using System.Text.Json;

namespace Joker.Services;

public class GameService
{
    private const string CURRENT_GAME_KEY = "joker_current_game";
    private const string GAME_HISTORY_KEY = "joker_game_history";
    private const int MAX_HISTORY = 10;
    
    private readonly LocalStorageService _localStorage;
    
    public GameState? CurrentGame { get; private set; }
    
    public event Action? OnGameStateChanged;
    
    public GameService(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }
    
    public async Task LoadCurrentGameAsync()
    {
        var json = await _localStorage.GetItemAsync(CURRENT_GAME_KEY);
        if (!string.IsNullOrEmpty(json))
        {
            CurrentGame = JsonSerializer.Deserialize<GameState>(json);
        }
    }
    
    public async Task SaveCurrentGameAsync()
    {
        if (CurrentGame != null)
        {
            var json = JsonSerializer.Serialize(CurrentGame);
            await _localStorage.SetItemAsync(CURRENT_GAME_KEY, json);
            OnGameStateChanged?.Invoke();
        }
    }
    
    public async Task ClearCurrentGameAsync()
    {
        await _localStorage.RemoveItemAsync(CURRENT_GAME_KEY);
        CurrentGame = null;
        OnGameStateChanged?.Invoke();
    }
    
    public void StartNewGame(string[] playerNames, GameMode mode, PlayType playType)
    {
        CurrentGame = new GameState
        {
            PlayerNames = playerNames,
            Mode = mode,
            PlayType = playType,
            CurrentDealerIndex = 3, // Dealer starts at player 4 (index 3)
            CurrentSegmentIndex = 0,
            CurrentRoundIndex = 0,
            StartedAt = DateTime.UtcNow
        };
        
        InitializeSegments();
    }
    
    private void InitializeSegments()
    {
        if (CurrentGame == null) return;
        
        CurrentGame.Segments.Clear();
        
        if (CurrentGame.Mode == GameMode.Standard)
        {
            // Segment 1: 1-8 cards
            var segment1 = new SegmentData { SegmentNumber = 1 };
            for (int i = 1; i <= 8; i++)
            {
                segment1.Rounds.Add(new RoundData { CardsDealt = i });
            }
            CurrentGame.Segments.Add(segment1);
            
            // Segment 2: 4 rounds of 9 cards
            var segment2 = new SegmentData { SegmentNumber = 2 };
            for (int i = 0; i < 4; i++)
            {
                segment2.Rounds.Add(new RoundData { CardsDealt = 9 });
            }
            CurrentGame.Segments.Add(segment2);
            
            // Segment 3: 8-1 cards
            var segment3 = new SegmentData { SegmentNumber = 3 };
            for (int i = 8; i >= 1; i--)
            {
                segment3.Rounds.Add(new RoundData { CardsDealt = i });
            }
            CurrentGame.Segments.Add(segment3);
            
            // Segment 4: 4 rounds of 9 cards
            var segment4 = new SegmentData { SegmentNumber = 4 };
            for (int i = 0; i < 4; i++)
            {
                segment4.Rounds.Add(new RoundData { CardsDealt = 9 });
            }
            CurrentGame.Segments.Add(segment4);
        }
        else // OnlyNines
        {
            // All segments have 4 rounds of 9 cards each
            for (int seg = 1; seg <= 4; seg++)
            {
                var segment = new SegmentData { SegmentNumber = seg };
                for (int i = 0; i < 4; i++)
                {
                    segment.Rounds.Add(new RoundData { CardsDealt = 9 });
                }
                CurrentGame.Segments.Add(segment);
            }
        }
    }
    
    public int GetFirstBidderIndex()
    {
        if (CurrentGame == null) return 0;
        // First bidder is the player after the dealer
        return (CurrentGame.CurrentDealerIndex + 1) % 4;
    }
    
    public bool ValidateBids(int[] bids, int cardsDealt)
    {
        return bids.Sum() != cardsDealt;
    }
    
    public bool CanLastPlayerBid(int[] bids, int lastPlayerBid, int cardsDealt, int lastPlayerIndex)
    {
        // Calculate sum of other players' bids
        int sumOfOthers = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i != lastPlayerIndex)
            {
                sumOfOthers += bids[i];
            }
        }
        
        // Check if this bid would make the total equal to cards dealt
        return (sumOfOthers + lastPlayerBid) != cardsDealt;
    }
    
    public int CalculateScore(int bid, int actual, int cardsDealt, int segmentNumber, GameMode mode)
    {
        // Correct guess
        if (bid == actual)
        {
            if (bid == 0) return 50;
            if (bid == cardsDealt) return bid * 100;
            
            return bid switch
            {
                1 => 100,
                2 => 150,
                3 => 200,
                4 => 250,
                5 => 300,
                6 => 350,
                7 => 400,
                8 => 450,
                _ => bid * 50 + 50
            };
        }
        
        // Incorrect guess
        int score = actual * 10;
        
        // Apply penalty if bid > 0 but took 0
        if (bid > 0 && actual == 0)
        {
            bool isNineSegment = (mode == GameMode.Standard && (segmentNumber == 2 || segmentNumber == 4)) 
                                 || mode == GameMode.OnlyNines;
            int penalty = isNineSegment ? 500 : 200;
            score -= penalty;
        }
        
        return score;
    }
    
    public void CalculateSegmentBonuses(SegmentData segment, GameMode mode)
    {
        // Reset bonuses
        segment.BonusAdjustments = new int[4];
        
        // Check who guessed all rounds correctly
        bool[] allCorrect = new bool[4];
        
        for (int p = 0; p < 4; p++)
        {
            allCorrect[p] = segment.Rounds.All(r => 
                r.ActualTricks[p].HasValue && r.Bids[p] == r.ActualTricks[p].Value);
        }
        
        for (int p = 0; p < 4; p++)
        {
            if (allCorrect[p])
            {
                // Find highest score for this player
                int maxScore = segment.Rounds.Max(r => r.Scores[p]);
                
                // Find the LAST round with the highest score (not first)
                int maxScoreRoundIndex = -1;
                for (int i = segment.Rounds.Count - 1; i >= 0; i--)
                {
                    if (segment.Rounds[i].Scores[p] == maxScore)
                    {
                        maxScoreRoundIndex = i;
                        break;
                    }
                }
                
                // Double the highest score
                segment.Rounds[maxScoreRoundIndex].Scores[p] *= 2;
                segment.BonusAdjustments[p] += maxScore; // Track the bonus added
                
                // Deduct from opponents
                if (CurrentGame?.PlayType == PlayType.Pairs)
                {
                    int partnerIndex = p == 0 || p == 2 ? (p == 0 ? 2 : 0) : (p == 1 ? 3 : 1);
                    bool partnerAlsoAllCorrect = allCorrect[partnerIndex];
                    
                    // Determine which opponents to deduct from
                    List<int> opponentsToDeduct = new List<int>();
                    for (int opp = 0; opp < 4; opp++)
                    {
                        if (opp != p && opp != partnerIndex && !allCorrect[opp])
                        {
                            opponentsToDeduct.Add(opp);
                        }
                    }
                    
                    foreach (var opp in opponentsToDeduct)
                    {
                        ApplyDeduction(segment, opp);
                    }
                }
                else // Individuals
                {
                    // Deduct from all other players who didn't get all correct
                    for (int opp = 0; opp < 4; opp++)
                    {
                        if (opp != p && !allCorrect[opp])
                        {
                            ApplyDeduction(segment, opp);
                        }
                    }
                }
            }
        }
    }
    
    private void ApplyDeduction(SegmentData segment, int playerIndex)
    {
        // Find highest score
        int maxScore = segment.Rounds.Max(r => r.Scores[playerIndex]);
        
        // Find LAST round with highest score
        int maxScoreRoundIndex = -1;
        for (int i = segment.Rounds.Count - 1; i >= 0; i--)
        {
            if (segment.Rounds[i].Scores[playerIndex] == maxScore)
            {
                maxScoreRoundIndex = i;
                break;
            }
        }
        
        if (maxScoreRoundIndex >= 0)
        {
            // If it's the last round of the segment, use next-highest
            if (maxScoreRoundIndex == segment.Rounds.Count - 1)
            {
                // Find next highest score
                var scoresExcludingMax = segment.Rounds
                    .Select((r, idx) => new { Score = r.Scores[playerIndex], Index = idx })
                    .Where(x => x.Index != maxScoreRoundIndex || x.Score != maxScore)
                    .OrderByDescending(x => x.Score)
                    .ToList();
                
                if (scoresExcludingMax.Any())
                {
                    var nextHighest = scoresExcludingMax[0];
                    maxScoreRoundIndex = nextHighest.Index;
                    maxScore = nextHighest.Score;
                }
                else
                {
                    // If all rounds have the same score, deduct that score from any round
                    maxScore = segment.Rounds[0].Scores[playerIndex];
                    maxScoreRoundIndex = 0;
                }
            }
            
            segment.Rounds[maxScoreRoundIndex].Scores[playerIndex] -= maxScore;
            segment.BonusAdjustments[playerIndex] -= maxScore;
        }
    }
    
    public void RecalculateAllScores()
    {
        if (CurrentGame == null) return;
        
        foreach (var segment in CurrentGame.Segments)
        {
            // First recalculate base scores
            foreach (var round in segment.Rounds)
            {
                for (int p = 0; p < 4; p++)
                {
                    if (round.ActualTricks[p].HasValue)
                    {
                        round.Scores[p] = CalculateScore(
                            round.Bids[p],
                            round.ActualTricks[p].Value,
                            round.CardsDealt,
                            segment.SegmentNumber,
                            CurrentGame.Mode
                        );
                    }
                }
            }
            
            // Then apply bonuses if segment is complete
            if (segment.Rounds.All(r => r.IsCompleted))
            {
                CalculateSegmentBonuses(segment, CurrentGame.Mode);
            }
            
            // Calculate subtotals
            for (int p = 0; p < 4; p++)
            {
                segment.Subtotals[p] = segment.Rounds.Sum(r => r.Scores[p]);
            }
        }
    }
    
    public async Task CompleteGameAsync()
    {
        if (CurrentGame == null) return;
        
        CurrentGame.IsCompleted = true;
        CurrentGame.CompletedAt = DateTime.UtcNow;
        
        // Save to history
        var history = new GameHistory
        {
            PlayerNames = CurrentGame.PlayerNames,
            Mode = CurrentGame.Mode,
            PlayType = CurrentGame.PlayType,
            FinalScores = GetFinalScores(),
            CompletedAt = CurrentGame.CompletedAt.Value
        };
        
        await AddToHistoryAsync(history);
        await ClearCurrentGameAsync();
    }
    
    public int[] GetFinalScores()
    {
        if (CurrentGame == null) return new int[4];
        
        var scores = new int[4];
        
        // Calculate scores directly from rounds instead of relying on Subtotals
        foreach (var segment in CurrentGame.Segments)
        {
            foreach (var round in segment.Rounds)
            {
                // Only count completed rounds
                if (round.IsCompleted)
                {
                    for (int p = 0; p < 4; p++)
                    {
                        scores[p] += round.Scores[p];
                    }
                }
            }
        }
        
        // For pairs mode, combine scores
        if (CurrentGame.PlayType == PlayType.Pairs)
        {
            int pair1Total = scores[0] + scores[2]; // P1 + P3
            int pair2Total = scores[1] + scores[3]; // P2 + P4
            scores[0] = pair1Total;
            scores[1] = pair2Total;
            scores[2] = pair1Total;
            scores[3] = pair2Total;
        }
        
        return scores;
    }
    
    private async Task AddToHistoryAsync(GameHistory history)
    {
        var json = await _localStorage.GetItemAsync(GAME_HISTORY_KEY);
        var historyList = string.IsNullOrEmpty(json) 
            ? new List<GameHistory>() 
            : JsonSerializer.Deserialize<List<GameHistory>>(json) ?? new List<GameHistory>();
        
        historyList.Insert(0, history);
        
        // Keep only last 10
        if (historyList.Count > MAX_HISTORY)
        {
            historyList = historyList.Take(MAX_HISTORY).ToList();
        }
        
        var newJson = JsonSerializer.Serialize(historyList);
        await _localStorage.SetItemAsync(GAME_HISTORY_KEY, newJson);
    }
    
    public async Task<List<GameHistory>> GetHistoryAsync()
    {
        var json = await _localStorage.GetItemAsync(GAME_HISTORY_KEY);
        return string.IsNullOrEmpty(json) 
            ? new List<GameHistory>() 
            : JsonSerializer.Deserialize<List<GameHistory>>(json) ?? new List<GameHistory>();
    }
}
