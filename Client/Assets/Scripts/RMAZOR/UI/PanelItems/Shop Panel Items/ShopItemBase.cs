using System;
using System.Collections;
using Common.Entities;
using Common.Extensions;
using Common.Managers;
using Common.UI;
using Common.Utils;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Shop_Panel_Items
{
    public class ViewShopItemInfo
    {
        public int    PurchaseKey      { get; set; }
        public bool   BuyForWatchingAd { get; set; }
        public string Price            { get; set; }
        public bool   Ready            { get; set; }
        public Sprite Icon             { get; set; }
        public Sprite Background       { get; set; }
        public string Currency         { get; set; }
        public int    Reward           { get; set; }
    }
    
    public abstract class ShopItemBase : SimpleUiItemBase
    {
        #region serialized fields
        
        public                     TextMeshProUGUI price;
        public                     TextMeshProUGUI title;
        public                     Image           itemIcon;
        [SerializeField] protected Button          buyButton;
        [SerializeField] protected Image           watchAdImage;
        [SerializeField] protected Animator        loadingAnim;

        #endregion

        #region nonpublic members
        
        private ViewShopItemInfo m_Info;
        private IEnumerator      m_StopIndicateLoadingCoroutine;

        #endregion

        #region api
        
        public virtual void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            m_Info = _Info;
            LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(title, ETextType.MenuUI));
            LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(price, ETextType.MenuUI));
            watchAdImage.SetGoActive(true);
            loadingAnim.SetGoActive(true);
            name = "Shop Item";
            buyButton.onClick.AddListener(SoundOnClick);
            buyButton.onClick.AddListener(_Click);
            itemIcon.sprite = _Info.Icon;
            if (_Info.Background.IsNotNull())
                background.sprite = _Info.Background;
        }
        
        public override void Init(
            IUITicker            _UITicker, 
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            throw new NotSupportedException();
        }

        public void UpdateState(
            Func<bool>  _Ready,
            UnityAction _OnFinish)
        {
            bool ready = _Ready();
            IndicateLoading(!ready);
            if (ready)
            {
                FinishAction();
                return;
            }
            m_StopIndicateLoadingCoroutine = Cor.WaitWhile(
                () => !_Ready(),
                () =>
                {
                    IndicateLoading(false);
                    _OnFinish?.Invoke();
                    FinishAction();
                });
            Cor.Run(m_StopIndicateLoadingCoroutine);
        }

        #endregion

        #region nonpublic methods
        
        private void IndicateLoading(bool _Indicate)
        {
            watchAdImage.SetGoActive(!_Indicate && m_Info.BuyForWatchingAd);
            price       .SetGoActive(!_Indicate && !m_Info.BuyForWatchingAd);
            loadingAnim .SetGoActive(_Indicate);
            loadingAnim .enabled = _Indicate;
        }

        private void FinishAction()
        {
            price.SetGoActive(!m_Info.BuyForWatchingAd);
            if (m_Info.BuyForWatchingAd) 
                return;
            price.text = $"{m_Info.Price}";
            if (!string.IsNullOrEmpty(price.text))
                return;
            price.text = LocalizationManager.GetTranslation("buy");
        }

        private void OnDestroy()
        {
            Cor.Stop(m_StopIndicateLoadingCoroutine);
        }

        #endregion
    }
}