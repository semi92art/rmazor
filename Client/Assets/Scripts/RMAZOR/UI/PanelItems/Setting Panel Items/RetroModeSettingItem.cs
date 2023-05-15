using System;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class RetroModeSettingItem : SettingItemOnOff
    {
        #region serialized fields

        [SerializeField] private Image           charLevelToUnlockBackground;
        [SerializeField] private TextMeshProUGUI charLevelToUnlockText;
        [SerializeField] private Image           lockIcon;

        #endregion

        #region nonpublic members
        
        private IScoreManager ScoreManager { get; set; }

        #endregion

        #region api

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IScoreManager        _ScoreManager,
            bool                 _IsOn,
            string               _TitleLocalizationKey,
            UnityAction<bool>    _Action)
        {
            ScoreManager = _ScoreManager;
            base.Init(_UITicker, _AudioManager, _LocalizationManager, _IsOn, _TitleLocalizationKey, _Action);
        }
        
        public void CheckIfSettingLocked()
        {
            bool retroModeUnlocked = SaveUtils.GetValue(SaveKeysRmazor.RetroModeUnlocked);
            bool isGameModeLocked;
            if (retroModeUnlocked)
            {
                isGameModeLocked = false;
            }
            else
            {
                var savedGame = ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
                object charXpArg = savedGame.Arguments.GetSafe(ComInComArg.KeyCharacterXp, out _);
                int charXp = Convert.ToInt32(charXpArg);
                int charLevel = RmazorUtils.GetCharacterLevel(
                    charXp, out _, out _);
                isGameModeLocked = charLevel < CommonDataRmazor.RetroModeCharacterLevelToUnlock;
            }
            if (charLevelToUnlockText.IsNull())
                return;
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