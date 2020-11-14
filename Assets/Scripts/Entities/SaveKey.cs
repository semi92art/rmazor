using System.Collections.Generic;
using Managers;
using Network.PacketArgs;

namespace Entities
{
    public class SaveKey
    {
        #region factory
    
        public static SaveKey AccountId => new SaveKey("account_id", typeof(int));
        public static SaveKey Login => new SaveKey("login", typeof(string));
        public static SaveKey PasswordHash => new SaveKey("password_hash", typeof(string));
        public static SaveKey Score(int _Id) => new SaveKey("score_", _Id.ToString(), typeof(Score));
        public static SaveKey GameId => new SaveKey("game_id", typeof(int));
        public static SaveKey LastConnectionSucceeded => new SaveKey("last_connection_succeeded", typeof(bool));
        public static SaveKey DailyBonusLastDate => new SaveKey("daily_bonus_last_date", typeof(System.DateTime));
        public static SaveKey DailyBonusLastItemClickedDay => new SaveKey("daily_bonus_last_item_clicked_date", typeof(int));
        public static SaveKey ShowAds => new SaveKey("show_ads", typeof(bool));
        public static SaveKey SettingSoundOn => new SaveKey("sound_on", typeof(bool));
        public static SaveKey SettingLanguage => new SaveKey("language", typeof(Language));
        public static SaveKey Money => new SaveKey("money", typeof(Dictionary<MoneyType, long>));
        public static SaveKey WheelOfFortuneLastDate => new SaveKey("wheel_of_fortune_last_date", typeof(System.DateTime));
        public static SaveKey CountryKey => new SaveKey("country_key", typeof(string));
        public static SaveKey ColorScheme => new SaveKey("color_scheme", typeof(string));
#if DEBUG
        public static SaveKey DebugServerUrl => new SaveKey("debug_server_url", typeof(string));
        public static SaveKey SettingDebug => new SaveKey("debug", typeof(bool));
#endif

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
}