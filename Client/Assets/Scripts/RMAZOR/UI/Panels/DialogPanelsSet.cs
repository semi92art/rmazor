using System.Linq;
using Common;
using Common.Helpers;
using Common.UI;
using Common.UI.DialogViewers;
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
            ILevelsDialogPanel           _LevelsDialogPanel)
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
                LevelsDialogPanel
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
                // SettingLanguageDialogPanel,
                ShopDialogPanel,
                RateGameDialogPanel,
                // TutorialDialogPanel,
                FinishLevelGroupDialogPanel,
                PlayBonusLevelDialogPanel,
                DailyGiftPanel,
                LevelsDialogPanel
            };
            foreach (var panel in panelsToLoad)
            {
                var dv = DialogViewersController.GetViewer(
                    panel.DialogViewerType);
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