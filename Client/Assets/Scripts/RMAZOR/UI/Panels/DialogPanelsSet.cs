using System.Linq;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using RMAZOR.UI.Panels.ShopPanels;

namespace RMAZOR.UI.Panels
{
    public interface IDialogPanelsSet : IInit
    {
        T GetPanel<T>() where T : IDialogPanel;
    }

    public class DialogPanelsSet : InitBase, IDialogPanelsSet
    {
        #region inject

        private IDialogViewersController     DialogViewersController     { get; }
        private ISettingDialogPanel          SettingDialogPanel          { get; }
        private ISettingLanguageDialogPanel  SettingLanguageDialogPanel  { get; }
        private IShopDialogPanel             ShopDialogPanel             { get; }
        private ICharacterDiedDialogPanel    CharacterDiedDialogPanel    { get; }
        private IRateGameDialogPanel         RateGameDialogPanel         { get; }
        private ITutorialDialogPanel         TutorialDialogPanel         { get; }
        private IFinishLevelGroupDialogPanel FinishLevelGroupDialogPanel { get; }
        private IPlayBonusLevelDialogPanel   PlayBonusLevelDialogPanel   { get; }
        private IDailyGiftPanel              DailyGiftPanel              { get; }
        private ILevelsDialogPanel           LevelsDialogPanel           { get; }
        private IConfirmLoadLevelDialogPanel ConfirmLoadLevelDialogPanel { get; }
        private IDisableAdsDialogPanel       DisableAdsDialogPanel       { get; }
        private IMainMenuPanel               MainMenuPanel               { get; }


        public DialogPanelsSet(
            IDialogViewersController     _DialogViewersController,
            ISettingDialogPanel          _SettingDialogPanel,
            ISettingLanguageDialogPanel  _SettingLanguageDialogPanel, 
            IShopDialogPanel             _ShopDialogPanel,
            ICharacterDiedDialogPanel    _CharacterDiedDialogPanel,
            IRateGameDialogPanel         _RateGameDialogPanel,
            ITutorialDialogPanel         _TutorialDialogPanel,
            IFinishLevelGroupDialogPanel _FinishLevelGroupDialogPanel,
            IPlayBonusLevelDialogPanel   _PlayBonusLevelDialogPanel,
            IDailyGiftPanel              _DailyGiftPanel,
            ILevelsDialogPanel           _LevelsDialogPanel,
            IConfirmLoadLevelDialogPanel _ConfirmLoadLevelDialogPanel,
            IDisableAdsDialogPanel       _DisableAdsDialogPanel,
            IMainMenuPanel               _MainMenuPanel)
        {
            DialogViewersController     = _DialogViewersController;
            SettingDialogPanel          = _SettingDialogPanel;
            SettingLanguageDialogPanel  = _SettingLanguageDialogPanel;
            ShopDialogPanel             = _ShopDialogPanel;
            CharacterDiedDialogPanel    = _CharacterDiedDialogPanel;
            RateGameDialogPanel         = _RateGameDialogPanel;
            TutorialDialogPanel         = _TutorialDialogPanel;
            FinishLevelGroupDialogPanel = _FinishLevelGroupDialogPanel;
            PlayBonusLevelDialogPanel   = _PlayBonusLevelDialogPanel;
            DailyGiftPanel              = _DailyGiftPanel;
            LevelsDialogPanel           = _LevelsDialogPanel;
            ConfirmLoadLevelDialogPanel = _ConfirmLoadLevelDialogPanel;
            DisableAdsDialogPanel       = _DisableAdsDialogPanel;
            MainMenuPanel               = _MainMenuPanel;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            ShopDialogPanel.Init();
            LoadDialogPanels();
            base.Init();
        }

        public T GetPanel<T>() where T : IDialogPanel
        {
            var panels = new[]
            {
                (IDialogPanel) SettingDialogPanel,
                SettingLanguageDialogPanel,
                ShopDialogPanel,
                CharacterDiedDialogPanel,
                RateGameDialogPanel,
                TutorialDialogPanel,
                FinishLevelGroupDialogPanel,
                PlayBonusLevelDialogPanel,
                DailyGiftPanel,
                LevelsDialogPanel,
                ConfirmLoadLevelDialogPanel,
                DisableAdsDialogPanel,
                MainMenuPanel
            };
            var result = panels.FirstOrDefault(_Panel => _Panel is T);
            return (T)result;
        }

        #endregion

        #region nonpublic methods
        
        private void LoadDialogPanels()
        {
            var panelsToLoad = new[]
            {
                CharacterDiedDialogPanel,
                (IDialogPanel)SettingDialogPanel, 
                // SettingLanguageDialogPanel, // инициализировать на старте не нужно
                ShopDialogPanel,
                RateGameDialogPanel,
                // TutorialDialogPanel, // инициализировать на старте не нужно
                FinishLevelGroupDialogPanel,
                PlayBonusLevelDialogPanel,
                DailyGiftPanel,
                LevelsDialogPanel,
                ConfirmLoadLevelDialogPanel,
                DisableAdsDialogPanel,
                MainMenuPanel
            };
            foreach (var panel in panelsToLoad)
            {
                var dv = DialogViewersController.GetViewer(panel.DialogViewerId);
                panel.LoadPanel(dv.Container, dv.Back);
            }
        }

        #endregion
    }

    public class DialogPanelsSetFake : InitBase, IDialogPanelsSet
    {
        public T GetPanel<T>() where T : IDialogPanel => default;
    }
}