using System;
using System.Globalization;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using RMAZOR.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public class MainMenuPanelButtonPlayPuzzles : MainMenuPanelButtonPlayBase
    {
        #region constants

        private const int CharacterLevelToUnlock = 3;

        #endregion
        
        #region serialized fields

        [SerializeField] private Image           charLevelToUnlockBackground;
        [SerializeField] private TextMeshProUGUI charLevelToUnlockText;
        [SerializeField] private Image           lockIcon;
        [SerializeField] private bool            doLock;

        #endregion
        
        #region nonpublic members

        private Func<int> GetPuzzlesTotalCount    { get; set; }
        private Func<int> GetPuzzlesFinishedCount { get; set; }

        protected override SaveKey<bool> BadgeNewGameModeMustBeHidden => null;

        private IScoreManager ScoreManager { get; set; }
        
        #endregion
        
        #region api

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IScoreManager        _ScoreManager,
            Func<int>            _GetPuzzlesTotalCount,
            Func<int>            _GetPuzzlesFinishedCount)
        {
            ScoreManager = _ScoreManager;
            (GetPuzzlesTotalCount, GetPuzzlesFinishedCount) = (_GetPuzzlesTotalCount, _GetPuzzlesFinishedCount);
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            var locTextInfos = new[]
            {
                new LocTextInfo(title, ETextType.MenuUI_H1, "puzzles", TextFormula),
                new LocTextInfo(body,  ETextType.MenuUI_H1, "empty_key",
                    _TextLocalizationType: ETextLocalizationType.OnlyFont),
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
            UpdateState();
            LocalizationManager.LanguageChanged -= OnLanguageChanged;
            LocalizationManager.LanguageChanged += OnLanguageChanged;
        }
        
        public override void UpdateState()
        {
            base.UpdateState();
            body.text = GetPuzzlesFinishedCount() + "/" + GetPuzzlesTotalCount();
            CheckIfGameModeLocked();
        }

        #endregion

        #region nonpublic methods

        private void OnLanguageChanged(ELanguage _Language)
        {
            UpdateState();
        }

        private void CheckIfGameModeLocked()
        {
            var savedGame = ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
            object charXpArg = savedGame.Arguments.GetSafe(ComInComArg.KeyCharacterXp, out _);
            int charXp = Convert.ToInt32(charXpArg);
            int charLevel = RmazorUtils.GetCharacterLevel(
                charXp, out _, out _);
            bool isGameModeLocked = charLevel < CharacterLevelToUnlock;
            isGameModeLocked &= doLock;            
            if (charLevelToUnlockText.IsNull())
                return;
            charLevelToUnlockText.text          = CharacterLevelToUnlock.ToString();
            charLevelToUnlockText.enabled       = isGameModeLocked;
            charLevelToUnlockBackground.enabled = isGameModeLocked;
            lockIcon.enabled                    = isGameModeLocked;
            button.interactable                 = !isGameModeLocked;
        }

        #endregion
    }
}