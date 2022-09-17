using System.Collections.Generic;
using Common;
using Common.Helpers;
using Common.UI;
using Common.UI.DialogViewers;
using RMAZOR.UI.Panels.ShopPanels;

namespace RMAZOR.UI.Panels
{
    public interface IDialogPanelsSet : IInit
    {
        ISettingDialogPanel          SettingDialogPanel          { get; }
        ISettingLanguageDialogPanel  SettingLanguageDialogPanel  { get; }
        IShopDialogPanel             ShopDialogPanel             { get; }
        ICharacterDiedDialogPanel    CharacterDiedDialogPanel    { get; }
        IRateGameDialogPanel         RateGameDialogPanel         { get; }
        ITutorialDialogPanel         TutorialDialogPanel         { get; }
        IFinishLevelGroupDialogPanel FinishLevelGroupDialogPanel { get; }

        IEnumerable<IDialogPanel> GetPanels();
    }

    public class DialogPanelsSet : InitBase, IDialogPanelsSet
    {
        #region inject
        
        private IDialogViewersController     DialogViewersController     { get; }
        public  ISettingDialogPanel          SettingDialogPanel          { get; }
        public  ISettingLanguageDialogPanel  SettingLanguageDialogPanel  { get; }
        public  IShopDialogPanel             ShopDialogPanel             { get; }
        public  ICharacterDiedDialogPanel    CharacterDiedDialogPanel    { get; }
        public  IRateGameDialogPanel         RateGameDialogPanel         { get; }
        public  ITutorialDialogPanel         TutorialDialogPanel         { get; }
        public  IFinishLevelGroupDialogPanel FinishLevelGroupDialogPanel { get; }


        public DialogPanelsSet(
            IDialogViewersController     _DialogViewersController,
            ISettingDialogPanel          _SettingDialogPanel,
            ISettingLanguageDialogPanel  _SettingLanguageDialogPanel, 
            IShopDialogPanel             _ShopDialogPanel,
            ICharacterDiedDialogPanel    _CharacterDiedDialogPanel,
            IRateGameDialogPanel         _RateGameDialogPanel,
            ITutorialDialogPanel         _TutorialDialogPanel,
            IFinishLevelGroupDialogPanel _FinishLevelGroupDialogPanel)
        {
            DialogViewersController     = _DialogViewersController;
            SettingDialogPanel          = _SettingDialogPanel;
            SettingLanguageDialogPanel  = _SettingLanguageDialogPanel;
            ShopDialogPanel             = _ShopDialogPanel;
            CharacterDiedDialogPanel    = _CharacterDiedDialogPanel;
            RateGameDialogPanel         = _RateGameDialogPanel;
            TutorialDialogPanel         = _TutorialDialogPanel;
            FinishLevelGroupDialogPanel = _FinishLevelGroupDialogPanel;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            ShopDialogPanel.Init();
            LoadDialogPanels();
            base.Init();
        }
        
        public IEnumerable<IDialogPanel> GetPanels()
        {
            return new[]
            {
                (IDialogPanel)SettingDialogPanel, 
                SettingLanguageDialogPanel,
                ShopDialogPanel,
                CharacterDiedDialogPanel,
                RateGameDialogPanel,
                TutorialDialogPanel,
                FinishLevelGroupDialogPanel
            };
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
                FinishLevelGroupDialogPanel
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
        public ISettingDialogPanel          SettingDialogPanel          => null;
        public ISettingLanguageDialogPanel  SettingLanguageDialogPanel  => null;
        public IShopDialogPanel             ShopDialogPanel             => null;
        public ICharacterDiedDialogPanel    CharacterDiedDialogPanel    => null;
        public IRateGameDialogPanel         RateGameDialogPanel         => null;
        public ITutorialDialogPanel         TutorialDialogPanel         => null;
        public IFinishLevelGroupDialogPanel FinishLevelGroupDialogPanel => null;
        public IEnumerable<IDialogPanel>    GetPanels()                 => new List<IDialogPanel>();
    }
}