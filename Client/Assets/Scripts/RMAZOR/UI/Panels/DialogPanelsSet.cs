using Common;
using Common.Helpers;
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
    }

    public class DialogPanelsSet : InitBase, IDialogPanelsSet
    {
        public ISettingDialogPanel          SettingDialogPanel          { get; }
        public ISettingLanguageDialogPanel  SettingLanguageDialogPanel  { get; }
        public IShopDialogPanel             ShopDialogPanel             { get; }
        public ICharacterDiedDialogPanel    CharacterDiedDialogPanel    { get; }
        public IRateGameDialogPanel         RateGameDialogPanel         { get; }
        public ITutorialDialogPanel         TutorialDialogPanel         { get; }
        public IFinishLevelGroupDialogPanel FinishLevelGroupDialogPanel { get; }

        public DialogPanelsSet(
            ISettingDialogPanel         _SettingDialogPanel,
            ISettingLanguageDialogPanel _SettingLanguageDialogPanel, 
            IShopDialogPanel            _ShopDialogPanel,
            ICharacterDiedDialogPanel   _CharacterDiedDialogPanel,
            IRateGameDialogPanel        _RateGameDialogPanel,
            ITutorialDialogPanel        _TutorialDialogPanel,
            IFinishLevelGroupDialogPanel _FinishLevelGroupDialogPanel)
        {
            SettingDialogPanel          = _SettingDialogPanel;
            SettingLanguageDialogPanel  = _SettingLanguageDialogPanel;
            ShopDialogPanel             = _ShopDialogPanel;
            CharacterDiedDialogPanel    = _CharacterDiedDialogPanel;
            RateGameDialogPanel         = _RateGameDialogPanel;
            TutorialDialogPanel         = _TutorialDialogPanel;
            FinishLevelGroupDialogPanel = _FinishLevelGroupDialogPanel;
        }

        public override void Init()
        {
            ShopDialogPanel.Init();
            base.Init();
        }
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
    }
}