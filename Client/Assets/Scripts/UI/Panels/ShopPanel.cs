using System.Collections.Generic;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using LeTai.Asset.TranslucentImage;
using Ticker;
using UI.Entities;
using UI.Factories;
using UI.PanelItems.Shop_Items;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace UI.Panels
{
    public interface IShopDialogPanel : IDialogPanel { }
    
    public class ShopPanel : DialogPanelBase, IShopDialogPanel
    {
        #region constants

        private const string PrefabSetName = "shop_items";

        #endregion
        
        #region nonpublic members

        private RectTransform m_Content;
        
        private readonly RectTransformLite m_ShopMainItemRectLite = new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 0, 0, 0),
            AnchoredPosition = Vector2.zero,
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(300f, 110)
        };

        #endregion

        #region inject
        
        public ShopPanel(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider)
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider)
        { }
        
        #endregion

        #region api

        public override EUiCategory Category => EUiCategory.Shop;
        
        public override void Init()
        {
            base.Init();
            var sp = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels,
                "shop_panel");
            m_Content = sp.GetCompItem<RectTransform>("content");
            var translBack = sp.GetCompItem<TranslucentImage>("translucent_background");
            translBack.source = CameraProvider.MainCamera.GetComponent<TranslucentImageSource>();
            Panel = sp.RTransform();
            m_Content.gameObject.DestroyChildrenSafe();
            InitItems();
            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                m_Content.anchoredPosition = m_Content.anchoredPosition.SetY(m_Content.rect.height * 0.5f);
            }));
        }

        #endregion

        #region nonpublic methods

        private void InitItems()
        {
            var dict = new Dictionary<string, UnityAction>
            {
                {"shop_money_icon", OpenShopMoneyPanel},
                {"shop_heads_icon", OpenShopHeadsPanel},
                {"shop_tails_icon", OpenShopTailsPanel},
                {"shop_backgrounds_icon", OpenShopBackgroundsPanel}
            };
            foreach (var kvp in dict)
            {
                var item = CreateShopMainItem();
                item.Init(
                    Managers,
                    Ticker,
                    kvp.Value,
                    PrefabUtilsEx.GetObject<Sprite>(PrefabSetName, kvp.Key));
            }
        }

        private ShopMainItem CreateShopMainItem()
        {
            var obj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    m_ShopMainItemRectLite),
                PrefabSetName, "shop_main_item");
            return obj.GetComponent<ShopMainItem>();
        }

        private void OpenShopMoneyPanel()
        {
            
        }

        private void OpenShopHeadsPanel()
        {
            
        }

        private void OpenShopTailsPanel()
        {
            
        }

        private void OpenShopBackgroundsPanel()
        {
            
        }

        #endregion
    }
}