using UI.Panels;
using UI.Panels.ShopPanels;

namespace Games.RazorMaze.Views.UI
{
    public interface IDialogPanels
    {
        IDailyBonusDialogPanel      DailyBonusDialogPanel      { get; }
        ISettingDialogPanel         SettingDialogPanel         { get; }
        ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        IShopDialogPanel            ShopDialogPanel            { get; }
        IShopMoneyDialogPanel       ShopMoneyDialogPanel       { get; }
        IWofDialogPanel             WofDialogPanel             { get; }
        IWofRewardPanel             WofRewardPanel             { get; }
        ICharacterDiedDialogPanel   CharacterDiedDialogPanel   { get; }
        IRateGameDialogPanel        RateGameDialogPanel        { get; }
    }

    public class DialogPanels : IDialogPanels
    {
        public IDailyBonusDialogPanel      DailyBonusDialogPanel      { get; }
        public ISettingDialogPanel         SettingDialogPanel         { get; }
        public ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        public IShopDialogPanel            ShopDialogPanel            { get; }
        public IShopMoneyDialogPanel       ShopMoneyDialogPanel       { get; }
        public IWofDialogPanel             WofDialogPanel             { get; }
        public IWofRewardPanel             WofRewardPanel             { get; }
        public ICharacterDiedDialogPanel   CharacterDiedDialogPanel   { get; }
        public IRateGameDialogPanel        RateGameDialogPanel        { get; }

        public DialogPanels(
            IDailyBonusDialogPanel      _DailyBonusDialogPanel,
            ISettingDialogPanel         _SettingDialogPanel,
            ISettingSelectorDialogPanel _SettingSelectorDialogPanel, 
            IShopDialogPanel            _ShopDialogPanel,
            IShopMoneyDialogPanel       _ShopMoneyDialogPanel,
            IWofDialogPanel             _WofDialogPanel,
            IWofRewardPanel             _WofRewardPanel, 
            ICharacterDiedDialogPanel   _CharacterDiedDialogPanel, 
            IRateGameDialogPanel        _RateGameDialogPanel)
        {
            DailyBonusDialogPanel       = _DailyBonusDialogPanel;
            SettingDialogPanel          = _SettingDialogPanel;
            SettingSelectorDialogPanel  = _SettingSelectorDialogPanel;
            ShopDialogPanel             = _ShopDialogPanel;
            ShopMoneyDialogPanel        = _ShopMoneyDialogPanel;
            WofDialogPanel              = _WofDialogPanel;
            WofRewardPanel              = _WofRewardPanel;
            CharacterDiedDialogPanel    = _CharacterDiedDialogPanel;
            RateGameDialogPanel         = _RateGameDialogPanel;
        }
    }

    public class DialogPanelsFake : IDialogPanels
    {
        public IDailyBonusDialogPanel      DailyBonusDialogPanel      { get; }
        public ISettingDialogPanel         SettingDialogPanel         { get; }
        public ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        public IShopDialogPanel            ShopDialogPanel            { get; }
        public IShopMoneyDialogPanel       ShopMoneyDialogPanel       { get; }
        public IWofDialogPanel             WofDialogPanel             { get; }
        public IWofRewardPanel             WofRewardPanel             { get; }
        public ICharacterDiedDialogPanel   CharacterDiedDialogPanel   { get; }
        public IRateGameDialogPanel        RateGameDialogPanel        { get; }
    }
}