using System;
using System.Globalization;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public class MainMenuPanelButtonPlayPuzzles : MainMenuPanelButtonPlayBase
    {
        #region nonpublic members

        private Func<int> GetPuzzlesTotalCount    { get; set; }
        private Func<int> GetPuzzlesFinishedCount { get; set; }

        protected override SaveKey<bool> BadgeNewGameModeMustBeHidden =>
            SaveKeysRmazor.MainMenuButtonPuzzlesBadgeNewMustBeHidden;
        
        #endregion
        
        #region api

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            Func<int>            _GetPuzzlesTotalCount,
            Func<int>            _GetPuzzlesFinishedCount)
        {
            (GetPuzzlesTotalCount, GetPuzzlesFinishedCount) = (_GetPuzzlesTotalCount, _GetPuzzlesFinishedCount);
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            var locTextInfo = new LocTextInfo(title, ETextType.MenuUI, "puzzles", 
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            LocalizationManager.AddLocalization(locTextInfo);
            UpdateState();
        }


        public override void UpdateState()
        {
            body.text = GetPuzzlesFinishedCount() + "/" + GetPuzzlesTotalCount();
        }

        #endregion
    }
}