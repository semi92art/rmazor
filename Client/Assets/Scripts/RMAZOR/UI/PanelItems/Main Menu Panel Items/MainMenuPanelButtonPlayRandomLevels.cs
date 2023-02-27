using System.Globalization;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public class MainMenuPanelButtonPlayRandomLevels : MainMenuPanelButtonPlayBase
    {
        #region nonpublic members

        protected override SaveKey<bool> BadgeNewGameModeMustBeHidden =>
            SaveKeysRmazor.MainMenuButtonRandomBadgeNewMustBeHidden;

        #endregion
        
        #region api

        public override void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            var locTextInfo = new LocTextInfo(title, ETextType.MenuUI, "simple_mode",
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            LocalizationManager.AddLocalization(locTextInfo);
        }

        public override void UpdateState() { }
        
        #endregion
    }
}