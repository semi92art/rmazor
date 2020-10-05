using Network.PacketArgs;

public class SaveKey
{
    #region factory
    
    public static SaveKey Login => new SaveKey("login", typeof(string));
    public static SaveKey PasswordHash => new SaveKey("password_hash", typeof(string));
    public static SaveKey Score(int _Id) => new SaveKey("score_", _Id.ToString(), typeof(GetScoreResponseArgs));
    public static SaveKey GameId => new SaveKey("game_id", typeof(int));
    
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