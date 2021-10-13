using UI.Panels;

namespace Games.RazorMaze.Views.UI
{
    public interface IDialogPanels
    {
        IGameMenuDialogPanel        GameMenuDialogPanel { get; }
        IDailyBonusDialogPanel      DailyBonusDialogPanel { get; }
        ILoadingDialogPanel         LoadingDialogPanel { get; }
        ISelectGameDialogPanel      SelectGameDialogPanel { get; }
        ISettingDialogPanel         SettingDialogPanel { get; }
        ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        IShopDialogPanel            ShopDialogPanel { get; }
        IWheelOfFortuneDialogPanel  WheelOfFortuneDialogPanel { get; }
        IWheelOfFortuneRewardPanel  WheelOfFortuneRewardPanel { get; }
    }

    public class DialogPanels : IDialogPanels
    {
        public IGameMenuDialogPanel        GameMenuDialogPanel { get; }
        public IDailyBonusDialogPanel      DailyBonusDialogPanel { get; }
        public ILoadingDialogPanel         LoadingDialogPanel { get; }
        public ISelectGameDialogPanel      SelectGameDialogPanel { get; }
        public ISettingDialogPanel         SettingDialogPanel { get; }
        public ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        public IShopDialogPanel            ShopDialogPanel { get; }
        public IWheelOfFortuneDialogPanel  WheelOfFortuneDialogPanel { get; }
        public IWheelOfFortuneRewardPanel  WheelOfFortuneRewardPanel { get; }
        
        public DialogPanels(
            IGameMenuDialogPanel _GameMenuDialogPanel,
            IDailyBonusDialogPanel _DailyBonusDialogPanel,
            ILoadingDialogPanel _LoadingDialogPanel,
            ISelectGameDialogPanel _SelectGameDialogPanel, 
            ISettingDialogPanel _SettingDialogPanel,
            ISettingSelectorDialogPanel _SettingSelectorDialogPanel,
             IShopDialogPanel _ShopDialogPanel,
            IWheelOfFortuneDialogPanel _WheelOfFortuneDialogPanel,
            IWheelOfFortuneRewardPanel _WheelOfFortuneRewardPanel)
        {
            GameMenuDialogPanel = _GameMenuDialogPanel;
            DailyBonusDialogPanel = _DailyBonusDialogPanel;
            LoadingDialogPanel = _LoadingDialogPanel;
            SelectGameDialogPanel = _SelectGameDialogPanel;
            SettingDialogPanel = _SettingDialogPanel;
            SettingSelectorDialogPanel = _SettingSelectorDialogPanel;
            ShopDialogPanel = _ShopDialogPanel;
            WheelOfFortuneDialogPanel = _WheelOfFortuneDialogPanel;
            WheelOfFortuneRewardPanel = _WheelOfFortuneRewardPanel;
        }


    }
}