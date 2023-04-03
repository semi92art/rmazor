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
        #region nonpublic members

        private IDialogPanel[] m_PanelsCached;

        #endregion
        
        #region inject

        private IDialogViewersController     DialogViewersController     { get; }
        private ISettingDialogPanel          SettingDialogPanel          { get; }
        private ISettingLanguageDialogPanel  SettingLanguageDialogPanel  { get; }
        private IShopDialogPanel             ShopDialogPanel             { get; }
        private IShopMoneyDialogPanel        ShopMoneyDialogPanel        { get; }
        private ISpecialOfferDialogPanel     SpecialOfferDialogPanel     { get; }
        private ICharacterDiedDialogPanel    CharacterDiedDialogPanel    { get; }
        private IRateGameDialogPanel         RateGameDialogPanel         { get; }
        private ITutorialDialogPanel         TutorialDialogPanel         { get; }
        private IFinishLevelGroupDialogPanel FinishLevelGroupDialogPanel { get; }
        private IPlayBonusLevelDialogPanel   PlayBonusLevelDialogPanel   { get; }
        private IDailyGiftPanel              DailyGiftPanel              { get; }
        private ILevelsDialogPanelMain       LevelsDialogPanelMain       { get; }
        private ILevelsDialogPanelPuzzles    LevelsDialogPanelPuzzles    { get; }
        private IConfirmLoadLevelDialogPanel ConfirmLoadLevelDialogPanel { get; }
        private IDisableAdsDialogPanel       DisableAdsDialogPanel       { get; }
        private IMainMenuPanel               MainMenuPanel               { get; }
        private IDailyChallengePanel         DailyChallengePanel         { get; }
        private IHintDialogPanel             HintDialogPanel             { get; }
        private IRandomGenerationParamsPanel RandomGenerationParamsPanel { get; }
        private ICustomizeCharacterPanel     CustomizeCharacterPanel     { get; }
        private IConfirmGoToMainMenuPanel    ConfirmGoToMainMenuPanel    { get; }


        public DialogPanelsSet(
            IDialogViewersController    _DialogViewersController,
            ISettingDialogPanel         _SettingDialogPanel,
            ISettingLanguageDialogPanel _SettingLanguageDialogPanel, 
            IShopDialogPanel            _ShopDialogPanel,
            IShopMoneyDialogPanel       _ShopMoneyDialogPanel,
            ISpecialOfferDialogPanel    _SpecialOfferDialogPanel,
            ICharacterDiedDialogPanel    _CharacterDiedDialogPanel,
            IRateGameDialogPanel         _RateGameDialogPanel,
            ITutorialDialogPanel         _TutorialDialogPanel,
            IFinishLevelGroupDialogPanel _FinishLevelGroupDialogPanel,
            IPlayBonusLevelDialogPanel   _PlayBonusLevelDialogPanel,
            IDailyGiftPanel              _DailyGiftPanel,
            ILevelsDialogPanelMain       _LevelsDialogPanelMain,
            ILevelsDialogPanelPuzzles    _LevelsDialogPanelPuzzles,
            IConfirmLoadLevelDialogPanel _ConfirmLoadLevelDialogPanel,
            IDisableAdsDialogPanel       _DisableAdsDialogPanel,
            IMainMenuPanel               _MainMenuPanel,
            IDailyChallengePanel         _DailyChallengePanel,
            IHintDialogPanel             _HintDialogPanel,
            IRandomGenerationParamsPanel _RandomGenerationParamsPanel,
            ICustomizeCharacterPanel     _CustomizeCharacterPanel,
            IConfirmGoToMainMenuPanel    _ConfirmGoToMainMenuPanel)
        {
            DialogViewersController     = _DialogViewersController;
            SettingDialogPanel          = _SettingDialogPanel;
            SettingLanguageDialogPanel  = _SettingLanguageDialogPanel;
            ShopDialogPanel             = _ShopDialogPanel;
            ShopMoneyDialogPanel        = _ShopMoneyDialogPanel;
            SpecialOfferDialogPanel     = _SpecialOfferDialogPanel;
            CharacterDiedDialogPanel    = _CharacterDiedDialogPanel;
            RateGameDialogPanel         = _RateGameDialogPanel;
            TutorialDialogPanel         = _TutorialDialogPanel;
            FinishLevelGroupDialogPanel = _FinishLevelGroupDialogPanel;
            PlayBonusLevelDialogPanel   = _PlayBonusLevelDialogPanel;
            DailyGiftPanel              = _DailyGiftPanel;
            LevelsDialogPanelMain       = _LevelsDialogPanelMain;
            LevelsDialogPanelPuzzles    = _LevelsDialogPanelPuzzles;
            ConfirmLoadLevelDialogPanel = _ConfirmLoadLevelDialogPanel;
            DisableAdsDialogPanel       = _DisableAdsDialogPanel;
            MainMenuPanel               = _MainMenuPanel;
            DailyChallengePanel         = _DailyChallengePanel;
            HintDialogPanel             = _HintDialogPanel;
            RandomGenerationParamsPanel = _RandomGenerationParamsPanel;
            CustomizeCharacterPanel     = _CustomizeCharacterPanel;
            ConfirmGoToMainMenuPanel    = _ConfirmGoToMainMenuPanel;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            ShopMoneyDialogPanel.Init();
            CachePanels();
            LoadDialogPanels();
            base.Init();
        }

        public T GetPanel<T>() where T : IDialogPanel
        {
            var result = m_PanelsCached.FirstOrDefault(_Panel => _Panel is T);
            return (T)result;
        }

        #endregion

        #region nonpublic methods

        private void CachePanels()
        {
            m_PanelsCached = new IDialogPanel[]
            {
                MainMenuPanel,
                SettingDialogPanel,
                SettingLanguageDialogPanel,
                ShopMoneyDialogPanel,
                SpecialOfferDialogPanel,
                CharacterDiedDialogPanel,
                RateGameDialogPanel,
                TutorialDialogPanel,
                FinishLevelGroupDialogPanel,
                PlayBonusLevelDialogPanel,
                DailyGiftPanel,
                LevelsDialogPanelMain,
                LevelsDialogPanelPuzzles,
                ConfirmLoadLevelDialogPanel,
                DisableAdsDialogPanel,
                DailyChallengePanel,
                HintDialogPanel,
                RandomGenerationParamsPanel,
                CustomizeCharacterPanel,
                ConfirmGoToMainMenuPanel,
                ShopDialogPanel,
            };
        }
        
        private void LoadDialogPanels()
        {
            var pansToLoad = m_PanelsCached
                .Except(new IDialogPanel[] {SettingLanguageDialogPanel, TutorialDialogPanel});
            foreach (var panel in pansToLoad)
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