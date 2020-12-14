using System;
using System.Collections.Generic;
using Managers;

namespace Entities
{
    public class SaveKey
    {
        #region factory
    
        public static SaveKey AccountId => new SaveKey("account_id", typeof(int));
        public static SaveKey Login => new SaveKey("login", typeof(string));
        public static SaveKey PasswordHash => new SaveKey("password_hash", typeof(string));
        public static SaveKey GameId => new SaveKey("game_id", typeof(int));
        public static SaveKey LastConnectionSucceeded => new SaveKey("last_connection_succeeded", typeof(bool));
        public static SaveKey DailyBonusLastDate => new SaveKey("daily_bonus_last_date", typeof(DateTime));
        public static SaveKey DailyBonusLastItemClickedDay => new SaveKey("daily_bonus_last_item_clicked_date", typeof(int));
        public static SaveKey ShowAds => new SaveKey("show_ads", typeof(bool));
        public static SaveKey SettingSoundOn => new SaveKey("sound_on", typeof(bool));
        public static SaveKey SettingLanguage => new SaveKey("language", typeof(Language));
        public static SaveKey Money => new SaveKey("money", typeof(Dictionary<MoneyType, long>));
        public static SaveKey Scores => new SaveKey("scores", typeof(Dictionary<string, int>));
        public static SaveKey WheelOfFortuneLastDate => new SaveKey("wheel_of_fortune_last_date", typeof(DateTime));
        public static SaveKey ColorScheme => new SaveKey("color_scheme", typeof(string));

        #endregion
    
        public string Key { get; }
        public Type Type { get; }
    
        protected SaveKey(string _Key, Type _Type)
        {
            Key = _Key;
            Type = _Type;
        }
    }
    
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    
    public class SaveKeyDebug : SaveKey
    {
        #region factory
        
        public static SaveKeyDebug ServerUrl => new SaveKeyDebug("debug_server_url", typeof(string));
        public static SaveKeyDebug DebugUtilsOn => new SaveKeyDebug("debug", typeof(bool));
        public static SaveKeyDebug GoodQuality => new SaveKeyDebug("good_quality", typeof(bool));
        
        #endregion
        
        private SaveKeyDebug(string _Key, Type _Type) : base(_Key, _Type) { }
    }
    
#endif
}