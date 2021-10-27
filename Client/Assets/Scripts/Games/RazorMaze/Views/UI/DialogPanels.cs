using UI.Panels;
using UI.Panels.ShopPanels;

namespace Games.RazorMaze.Views.UI
{
    public interface IDialogPanels
    {
        IDailyBonusDialogPanel      DailyBonusDialogPanel { get; }
        ILoadingDialogPanel         LoadingDialogPanel { get; }
        ISettingDialogPanel         SettingDialogPanel { get; }
        ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        IShopDialogPanel            ShopDialogPanel { get; }
        IShopMoneyDialogPanel       ShopMoneyDialogPanel { get; }
        IWheelOfFortuneDialogPanel  WheelOfFortuneDialogPanel { get; }
        IWheelOfFortuneRewardPanel  WheelOfFortuneRewardPanel { get; }
    }

    public class DialogPanels : IDialogPanels
    {
        public IDailyBonusDialogPanel      DailyBonusDialogPanel { get; }
        public ILoadingDialogPanel         LoadingDialogPanel { get; }
        public ISettingDialogPanel         SettingDialogPanel { get; }
        public ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        public IShopDialogPanel            ShopDialogPanel { get; }
        public IShopMoneyDialogPanel       ShopMoneyDialogPanel { get; }
        public IWheelOfFortuneDialogPanel  WheelOfFortuneDialogPanel { get; }
        public IWheelOfFortuneRewardPanel  WheelOfFortuneRewardPanel { get; }
        
        public DialogPanels(
            IDailyBonusDialogPanel _DailyBonusDialogPanel,
            ILoadingDialogPanel _LoadingDialogPanel,
            ISettingDialogPanel _SettingDialogPanel,
            ISettingSelectorDialogPanel _SettingSelectorDialogPanel, 
            IShopDialogPanel _ShopDialogPanel,
            IShopMoneyDialogPanel _ShopMoneyDialogPanel,
            IWheelOfFortuneDialogPanel _WheelOfFortuneDialogPanel,
            IWheelOfFortuneRewardPanel _WheelOfFortuneRewardPanel)
        {
            DailyBonusDialogPanel = _DailyBonusDialogPanel;
            LoadingDialogPanel = _LoadingDialogPanel;
            SettingDialogPanel = _SettingDialogPanel;
            SettingSelectorDialogPanel = _SettingSelectorDialogPanel;
            ShopDialogPanel = _ShopDialogPanel;
            ShopMoneyDialogPanel = _ShopMoneyDialogPanel;
            WheelOfFortuneDialogPanel = _WheelOfFortuneDialogPanel;
            WheelOfFortuneRewardPanel = _WheelOfFortuneRewardPanel;
        }
    }
}