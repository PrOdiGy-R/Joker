namespace Joker.Services;

public class LanguageService
{
    public event Action? OnLanguageChanged;
    
    public string CurrentLanguage { get; private set; } = "en";
    
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        ["en"] = new Dictionary<string, string>
        {
            // Setup page
            ["setup_title"] = "New Game Setup",
            ["player"] = "Player",
            ["player_name"] = "Player Name",
            ["game_mode"] = "Game Mode",
            ["mode_standard"] = "Standard",
            ["mode_only_nines"] = "Only Nines",
            ["play_type"] = "Play Type",
            ["type_individuals"] = "Individuals",
            ["type_pairs"] = "Pairs",
            ["start_game"] = "Start Game",
            ["resume_game"] = "Resume Game",
            ["player_required"] = "All player names are required",
            
            // Scoreboard
            ["scoreboard_title"] = "Scoreboard",
            ["cards"] = "",
            ["segment"] = "Segment",
            ["subtotal"] = "Subtotal",
            ["total"] = "Total",
            ["bid"] = "Bid",
            ["took"] = "Took",
            ["dealer"] = "Dealer",
            ["enter_bids"] = "Enter Bids",
            ["enter_actual"] = "Enter Actual Tricks",
            ["save"] = "Save",
            ["cancel"] = "Cancel",
            ["edit"] = "Edit",
            ["finish_game"] = "Finish Game",
            ["new_game"] = "New Game",
            ["view_history"] = "View History",
            ["bid_sum_error"] = "Total bids cannot equal cards dealt!",
            ["confirm_end_game"] = "End Game Early?",
            ["game_not_complete_warning"] = "The game is not complete yet. Are you sure you want to finish and save the current scores?",
            ["tearing_away"] = "Tearing away",
            ["extra"] = "Extra",
            
            // Game complete
            ["game_complete"] = "Game Complete!",
            ["winner"] = "Winner",
            ["winners"] = "Winners",
            ["final_scores"] = "Final Scores",
            ["pair"] = "Pair",
            ["close"] = "Close",
            
            // History
            ["history_title"] = "Game History",
            ["no_history"] = "No games played yet",
            ["completed"] = "Completed",
            ["mode"] = "Mode",
            ["back"] = "Back",
            ["delete"] = "Delete",
            ["confirm_delete"] = "Delete Game?",
            ["confirm_delete_message"] = "Are you sure you want to delete this game from history?",
            
            // Common
            ["confirm"] = "Confirm",
            ["yes"] = "Yes",
            ["no"] = "No"
        },
        ["ka"] = new Dictionary<string, string>
        {
            // Setup page
            ["setup_title"] = "ახალი თამაშის დაწყება",
            ["player"] = "მოთამაშე",
            ["player_name"] = "მოთამაშის სახელი",
            ["game_mode"] = "თამაშის რეჟიმი",
            ["mode_standard"] = "სტანდარტული",
            ["mode_only_nines"] = "მხოლოდ ცხრიანები",
            ["play_type"] = "თამაშის ტიპი",
            ["type_individuals"] = "ინდივიდუალური",
            ["type_pairs"] = "წყვილები",
            ["start_game"] = "თამაშის დაწყება",
            ["resume_game"] = "თამაშის გაგრძელება",
            ["player_required"] = "ყველა მოთამაშის სახელი სავალდებულოა",
            
            // Scoreboard
            ["scoreboard_title"] = "ცოლები და ქმრები",
            ["cards"] = "",
            ["segment"] = "პულკა",
            ["subtotal"] = "ჯამი",
            ["total"] = "სულ",
            ["bid"] = "ვზიატკა",
            ["took"] = "აიღო",
            ["dealer"] = "დამრიგებელი",
            ["enter_bids"] = "თქვით ახლა ვზიატკა!",
            ["enter_actual"] = "ჰა ვინ რა წაიღეთ?",
            ["save"] = "შენახვა",
            ["cancel"] = "გაუქმება",
            ["edit"] = "შეცვალე",
            ["finish_game"] = "თამაშის დასრულება",
            ["new_game"] = "ახალი თამაში",
            ["view_history"] = "ისტორიის ნახვა",
            ["bid_sum_error"] = "ვზიატკების ჯამი არ უნდა იყოს კარტების რაოდენობის ტოლი!",
            ["confirm_end_game"] = "რაო ნებდებით? სემიჩკაა",
            ["game_not_complete_warning"] = "უეჭ ნებდებით? იშო ნე ვეჩერ",
            ["tearing_away"] = "წაგლეჯვაა",
            ["extra"] = "შეტენვაა",
            
            // Game complete
            ["game_complete"] = "თამაში დასრულდა!",
            ["winner"] = "გამარჯვებული",
            ["winners"] = "გამარჯვებულები",
            ["final_scores"] = "საბოლოო ქულები",
            ["pair"] = "წყვილი",
            ["close"] = "დახურვა",
            
            // History
            ["history_title"] = "თამაშების ისტორია",
            ["no_history"] = "ჯერ არ არის ჩატარებული თამაშები",
            ["completed"] = "დასრულდა",
            ["mode"] = "რეჟიმი",
            ["back"] = "უკან",
            ["delete"] = "წაშლა",
            ["confirm_delete"] = "თამაშის წაშლა?",
            ["confirm_delete_message"] = "დარწმუნებული ხართ, რომ გსურთ ამ თამაშის ისტორიიდან წაშლა?",
            
            // Common
            ["confirm"] = "დადასტურება",
            ["yes"] = "კი",
            ["no"] = "არა"
        }
    };
    
    public void SetLanguage(string language)
    {
        if (_translations.ContainsKey(language))
        {
            CurrentLanguage = language;
            OnLanguageChanged?.Invoke();
        }
    }
    
    public string T(string key)
    {
        if (_translations.TryGetValue(CurrentLanguage, out var langDict) 
            && langDict.TryGetValue(key, out var translation))
        {
            return translation;
        }
        return key;
    }
}
