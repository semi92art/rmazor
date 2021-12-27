using UI.Panels;
using UI.Panels.ShopPanels;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.UI
{
    public interface IDialogPanels : IInit
    {
        ISettingDialogPanel         SettingDialogPanel         { get; }
        ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        IShopDialogPanel            ShopDialogPanel            { get; }
        IShopMoneyDialogPanel       ShopMoneyDialogPanel       { get; }
        ICharacterDiedDialogPanel   CharacterDiedDialogPanel   { get; }
    }

    public class DialogPanels : IDialogPanels
    {
        public ISettingDialogPanel         SettingDialogPanel         { get; }
        public ISettingSelectorDialogPanel SettingSelectorDialogPanel { get; }
        public IShopDialogPanel            ShopDialogPanel            { get; }
        public IShopMoneyDialogPanel       ShopMoneyDialogPanel       { get; }
        public ICharacterDiedDialogPanel   CharacterDiedDialogPanel   { get; }

        public DialogPanels(
            ISettingDialogPanel         _SettingDialogPanel,
            ISettingSelectorDialogPanel _SettingSelectorDialogPanel, 
            IShopDialogPanel            _ShopDialogPanel,
            IShopMoneyDialogPanel       _ShopMoneyDialogPanel,
            ICharacterDiedDialogPanel   _CharacterDiedDialogPanel)
        {
            SettingDialogPanel          = _SettingDialogPanel;
            SettingSelectorDialogPanel  = _SettingSelectorDialogPanel;
            ShopDialogPanel             = _ShopDialogPanel;
            ShopMoneyDialogPanel        = _ShopMoneyDialogPanel;
            CharacterDiedDialogPanel    = _CharacterDiedDialogPanel;
        }

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        
        public void Init()
        {
            ShopMoneyDialogPanel.Init();
            Initialize?.Invoke();
            Initialized = true;
        }
    }

    public class DialogPanelsFake : IDialogPanels
    {
        public ISettingDialogPanel         SettingDialogPanel         => null;
        public ISettingSelectorDialogPanel SettingSelectorDialogPanel => null;
        public IShopDialogPanel            ShopDialogPanel            => null;
        public IShopMoneyDialogPanel       ShopMoneyDialogPanel       => null;
        public ICharacterDiedDialogPanel   CharacterDiedDialogPanel   => null;
        public bool                        Initialized                => false;
        public event UnityAction           Initialize;
        public void                        Init() { Initialize?.Invoke();}
    }
}