using Network.PacketArgs;

public class SaveKey
{
    #region factory
    
    public static SaveKey AccountId => new SaveKey("account_id", typeof(int));
    public static SaveKey Login => new SaveKey("login", typeof(string));
    public static SaveKey PasswordHash => new SaveKey("password_hash", typeof(string));
    public static SaveKey Score(int _Id) => new SaveKey("score_", _Id.ToString(), typeof(GetScoreResponseArgs));
    public static SaveKey GameId => new SaveKey("game_id", typeof(int));
    public static SaveKey DailyBonusLastDate => new SaveKey("daily_bonus_last_date", typeof(System.DateTime));
    public static SaveKey DailyBonusLastItemClickedDay => new SaveKey("daily_bonus_last_item_clicked_date", typeof(int));
    public static SaveKey DailyBonusOnDebug => new SaveKey("daily_bonus_on_debug", typeof(bool));
    public static SaveKey ShowAds => new SaveKey("show_ads", typeof(bool));
    public static SaveKey SettingSoundOn => new SaveKey("sound_on", typeof(bool));
    public static SaveKey SettingLanguage => new SaveKey("language", typeof(Language));
    public static SaveKey MoneyGold => new SaveKey("money_gold", typeof(int));
    public static SaveKey MoneyDiamonds => new SaveKey("money_diamonds", typeof(int));
    #endregion
    
    public string Key { get; }
    public System.Type Type { get; }
    
    private SaveKey(string _Key, System.Type _Type)
    {
        Key = _Key;
        Type = _Type;
    }
    
    private SaveKey(string _Key, string _KeySuffix, System.Type _Type)
    {
        Key = _Key + _KeySuffix;
        Type = _Type;
    }
}