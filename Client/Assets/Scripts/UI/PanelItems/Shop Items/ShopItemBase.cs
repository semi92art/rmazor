using Entities;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI.PanelItems.Shop_Items
{
    public class ShopItemArgs
    {
        public bool   BuyForWatchingAd { get; set; }
        public int    UnlockingLevel { get; set; }
        public string Price { get; set; }
        public bool   Ready { get; set; }
        public Sprite Icon { get; set; }
        public string Currency { get; set; }
        public int    Reward { get; set; }
    }
    
    public abstract class ShopItemBase : SimpleUiDialogItemView
    {
        public TextMeshProUGUI price;
        public TextMeshProUGUI title;
        public Image itemIcon;
        [SerializeField] protected Button buyButton;
        [SerializeField] protected Button watchAdButton;
        [SerializeField] protected Animator loadingAnim;

        public virtual void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            UnityAction _Click,
            ShopItemArgs _Args) { }

        protected void InitCore(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            UnityAction _Click,
            ShopItemArgs _Args,
            UnityAction _LoadingFinishAction)
        {
            name = "Shop Item";
            InitCore(_Managers, _UITicker);
            buyButton.onClick.AddListener(SoundOnClick);
            buyButton.onClick.AddListener(_Click);
            itemIcon.sprite = _Args.Icon;
            IndicateLoading(true, _Args.BuyForWatchingAd);
            Coroutines.Run(Coroutines.WaitWhile(
                () => !_Args.Ready,
                () =>
                {
                    IndicateLoading(false, _Args.BuyForWatchingAd);
                    buyButton.SetGoActive(!_Args.BuyForWatchingAd);
                    _LoadingFinishAction?.Invoke();
                }));
        }
        
        protected void IndicateLoading(bool _Indicate, bool _BuyForWatchingAd)
        {
            watchAdButton.SetGoActive(!_Indicate && _BuyForWatchingAd);
            price.SetGoActive(!_Indicate && !_BuyForWatchingAd);
            loadingAnim.SetGoActive(_Indicate);
            loadingAnim.enabled = _Indicate;
        }
    }
}