using System;
using System.Globalization;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Daily_Challenge_Panel_Items
{
    public class DailyChallengeInfo
    {
        public string   ChallengeType { get; set; }
        public int      IndexToday    { get; set; }
        public long     LevelIndex    { get; set; }
        public int      RewardMoney   { get; set; }
        public int      RewardXp      { get; set; }
        public object   Goal          { get; set; }
        public bool     Finished      { get; set; }
        public DateTime Day           { get; set; }
    }
    
    public class DailyChallengePanelItemArgs
    {
        public UnityAction        OnClick     { get; set; }
        public Sprite             RewardIcon  { get; set; }
        public DailyChallengeInfo Info        { get; set; }
    }
    
    public abstract class DailyChallengePanelItemBase : SimpleUiItem
    {
        #region serialized fields

        [SerializeField] protected Button          button;
        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected TextMeshProUGUI rewardTitle;
        [SerializeField] protected TextMeshProUGUI rewardMoneyCountText;
        [SerializeField] protected Image           rewardMoneyIcon;
        [SerializeField] protected Image           iconComplete;
        [SerializeField] private   TextMeshProUGUI rewardXpCountText;
        [SerializeField] private   TextMeshProUGUI rewardXpText;

        #endregion

        #region nonpublic members

        protected abstract string TitleLocalizationKey            { get; }
        protected abstract string GoalMeasureItemsLocalizationKey { get; }
        protected abstract string GoalColorString                 { get; }

        #endregion

        #region api

        public void Init(
            IUITicker                   _UITicker,
            IAudioManager               _AudioManager,
            ILocalizationManager        _LocalizationManager,
            DailyChallengePanelItemArgs _Args)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            Arguments = _Args;
            SubscribeButtonEvents();
            LocalizeTextObjects();
            CompleteChallenge(false);
        }
        
        public DailyChallengePanelItemArgs Arguments { get; private set; }
        
        public void CompleteChallenge()
        {
            CompleteChallenge(true);
        }

        #endregion

        #region nonpublic methods

        private void CompleteChallenge(bool _Complete)
        {
            rewardTitle         .enabled = !_Complete;
            rewardMoneyCountText.enabled = !_Complete;
            rewardMoneyIcon     .enabled = !_Complete;
            rewardXpCountText   .enabled = !_Complete;
            rewardXpText        .enabled = !_Complete;
            iconComplete        .enabled = _Complete;
            button.interactable          = !_Complete;
        }

        private void SubscribeButtonEvents()
        {
            button.onClick.AddListener(Arguments.OnClick);
        }

        private void LocalizeTextObjects()
        {
            var locTextInfos = new[]
            {
                new LocTextInfo(title, ETextType.MenuUI_H1, TitleLocalizationKey,
                    _T => _T.ToUpper(CultureInfo.CurrentUICulture)
                          + " "
                          + $"<size=32><color={GoalColorString}>{Arguments.Info.Goal}</color></size>"
                          + " "
                          + LocalizationManager.GetTranslation(GoalMeasureItemsLocalizationKey).ToUpper(CultureInfo.CurrentUICulture)),
                new LocTextInfo(rewardTitle, ETextType.MenuUI_H1, "reward",
                    _T => _T.ToUpper(CultureInfo.CurrentUICulture)),
                new LocTextInfo(rewardMoneyCountText, ETextType.MenuUI_H1, "empty",
                    _T => Arguments.Info.RewardMoney.ToString()),
                new LocTextInfo(rewardXpCountText, ETextType.MenuUI_H1, "empty",
                    _T => Arguments.Info.RewardXp.ToString())
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
        }

        #endregion
    }
}