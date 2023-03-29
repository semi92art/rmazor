using System;
using System.Globalization;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public class MainMenuPanelButtonPlayDailyChallenges : MainMenuPanelButtonPlayBase 
    {
        #region nonpublic members

        private Func<int> GetDailyChallengesTotalCount    { get; set; }
        private Func<int> GetDailyChallengesFinishedCount { get; set; }

        protected override SaveKey<bool> BadgeNewGameModeMustBeHidden => null;
        
        #endregion
        
        #region api

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            Func<int>            _GetDailyChallengesTotalCount,
            Func<int>            _GetDailyChallengesFinishedCount)
        {
            (GetDailyChallengesTotalCount, GetDailyChallengesFinishedCount) =
                (_GetDailyChallengesTotalCount, _GetDailyChallengesFinishedCount);
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            LocalizeTextObjectsOnInit();
        }
        
        public override void UpdateState()
        {
            base.UpdateState();
            body.text = GetDailyChallengesFinishedCount() + "/" + GetDailyChallengesTotalCount();
        }

        #endregion

        #region nonpublic methods

        private void LocalizeTextObjectsOnInit()
        {
            var locTextInfos = new[]
            {
                new LocTextInfo(title, ETextType.MenuUI_H1, "daily_challenges",
                    _T => _T.ToUpper(CultureInfo.CurrentUICulture)),
                new LocTextInfo(body,  ETextType.MenuUI_H1, "empty_key",
                    _T => GetDailyChallengesFinishedCount() + "/" + GetDailyChallengesTotalCount())
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
        }
        
        #endregion
    }
}