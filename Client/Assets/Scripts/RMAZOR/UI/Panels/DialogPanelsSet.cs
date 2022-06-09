using Common;
using Common.Helpers;
using RMAZOR.UI.Panels.ShopPanels;

namespace RMAZOR.UI.Panels
{
    public interface IDialogPanelsSet : IInit
    {
        ISettingDialogPanel         SettingDialogPanel         { get; }
        ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        IShopDialogPanel            ShopDialogPanel            { get; }
        IShopMoneyDialogPanel       ShopMoneyDialogPanel       { get; }
        ICharacterDiedDialogPanel   CharacterDiedDialogPanel   { get; }
        IRateGameDialogPanel        RateGameDialogPanel        { get; }
        ITutorialDialogPanel        TutorialDialogPanel        { get; }
    }

    public class DialogPanelsSet : InitBase, IDialogPanelsSet
    {
        public ISettingDialogPanel         SettingDialogPanel         { get; }
        public ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        public IShopDialogPanel            ShopDialogPanel            { get; }
        public IShopMoneyDialogPanel       ShopMoneyDialogPanel       { get; }
        public ICharacterDiedDialogPanel   CharacterDiedDialogPanel   { get; }
        public IRateGameDialogPanel        RateGameDialogPanel        { get; }
        public ITutorialDialogPanel        TutorialDialogPanel        { get; }

        public DialogPanelsSet(
            ISettingDialogPanel         _SettingDialogPanel,
            ISettingSelectorDialogPanel _SettingSelectorDialogPanel, 
            IShopDialogPanel            _ShopDialogPanel,
            IShopMoneyDialogPanel       _ShopMoneyDialogPanel,
            ICharacterDiedDialogPanel   _CharacterDiedDialogPanel,
            IRateGameDialogPanel        _RateGameDialogPanel,
            ITutorialDialogPanel        _TutorialDialogPanel)
        {
            SettingDialogPanel          = _SettingDialogPanel;
            SettingSelectorDialogPanel  = _SettingSelectorDialogPanel;
            ShopDialogPanel             = _ShopDialogPanel;
            ShopMoneyDialogPanel        = _ShopMoneyDialogPanel;
            CharacterDiedDialogPanel    = _CharacterDiedDialogPanel;
            RateGameDialogPanel         = _RateGameDialogPanel;
            TutorialDialogPanel         = _TutorialDialogPanel;
        }

        public override void Init()
        {
            ShopMoneyDialogPanel.Init();
            base.Init();
        }
    }

    public class DialogPanelsSetFake : InitBase, IDialogPanelsSet
    {
        public ISettingDialogPanel         SettingDialogPanel         => null;
        public ISettingSelectorDialogPanel SettingSelectorDialogPanel => null;
        public IShopDialogPanel            ShopDialogPanel            => null;
        public IShopMoneyDialogPanel       ShopMoneyDialogPanel       => null;
        public ICharacterDiedDialogPanel   CharacterDiedDialogPanel   => null;
        public IRateGameDialogPanel        RateGameDialogPanel        => null;
        public ITutorialDialogPanel        TutorialDialogPanel        => null;
    }
}