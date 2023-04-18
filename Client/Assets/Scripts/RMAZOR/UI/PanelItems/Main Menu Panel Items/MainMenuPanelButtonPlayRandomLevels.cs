using System;
using System.Globalization;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public class MainMenuPanelButtonPlayRandomLevels : MainMenuPanelButtonPlayBase
    {
        #region serialized fields

        [SerializeField] private Image           charLevelToUnlockBackground;
        [SerializeField] private TextMeshProUGUI charLevelToUnlockText;
        [SerializeField] private Image           lockIcon;
        [SerializeField] private bool            doLock;

        #endregion
        
        #region nonpublic members

        protected override SaveKey<bool> BadgeNewGameModeMustBeHidden =>
            SaveKeysRmazor.MainMenuButtonRandomBadgeNewMustBeHidden;
        
        private IScoreManager ScoreManager { get; set; }

        #endregion
        
        #region api

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IScoreManager        _ScoreManager)
        {
            ScoreManager = _ScoreManager;
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            var locTextInfo = new LocTextInfo(title, ETextType.MenuUI_H1, "simple_mode",
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            LocalizationManager.AddLocalization(locTextInfo);
        }
        
#pragma warning disable 0809
        [Obsolete]
        public override void Init(
#pragma warning restore 0809
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            throw new NotSupportedException();
        }
        
        public override void UpdateState()
        {
            base.UpdateState();
            CheckIfGameModeLocked();
        }
        
        #endregion
        

        #region nonpublic methods

        private void CheckIfGameModeLocked()
        {
            bool retroModeUnlocked = SaveUtils.GetValue(SaveKeysRmazor.RetroModeUnlocked);
            bool isGameModeLocked;
            if (retroModeUnlocked)
            {
                isGameModeLocked = false;
            }
            else
            {
                var savedGame = ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
                object charXpArg = savedGame.Arguments.GetSafe(ComInComArg.KeyCharacterXp, out _);
                int charXp = Convert.ToInt32(charXpArg);
                int charLevel = RmazorUtils.GetCharacterLevel(
                    charXp, out _, out _);
                isGameModeLocked = charLevel < CommonDataRmazor.RetroModeCharacterLevelToUnlock;
            }
            if (charLevelToUnlockText.IsNull())
                return;
            isGameModeLocked &= doLock;
            charLevelToUnlockText.text          = CommonDataRmazor.RetroModeCharacterLevelToUnlock.ToString();
            charLevelToUnlockText.enabled       = isGameModeLocked;
            charLevelToUnlockBackground.enabled = isGameModeLocked;
            lockIcon.enabled                    = isGameModeLocked;
            button.interactable                 = !isGameModeLocked;
            if (LocalizationManager.GetCurrentLanguage() == ELanguage.Russian)
                return;
            title.color = isGameModeLocked ? Color.gray : Color.white;
        }

        #endregion
    }
}