using DialogViewers;
using Entities;
using Ticker;
using UI.Entities;
using UI.PanelItems.Shop_Items;
using UnityEngine;

namespace UI.Panels.ShopPanels
{
    public interface IShopTailsDialogPanel : IDialogPanel { }
    
    public class ShopTailsPanel : ShopPanelBase<ShopTailItem>, IShopTailsDialogPanel
    {
        #region constants

        public const string ItemsSet = "shop_tail_items_set";

        #endregion

        #region nonpublic members

        protected override string ItemSetName => ItemsSet;
        protected override string PanelPrefabName => "shop_tails_panel";
        protected override string PanelItemPrefabName => "shop_tail_item";

        protected override RectTransformLite ShopItemRectLite => new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 0, 0, 0),
            AnchoredPosition = Vector2.zero,
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(300f, 110)
        };

        #endregion

        #region inject

        public ShopTailsPanel(
            IManagersGetter _Managers, 
            IUITicker _Ticker,
            IDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider)
            : base(_Managers, _Ticker, _DialogViewer, _CameraProvider)
        { }

        #endregion

        #region api

        public override EUiCategory Category => EUiCategory.Shop;

        #endregion
    }
}