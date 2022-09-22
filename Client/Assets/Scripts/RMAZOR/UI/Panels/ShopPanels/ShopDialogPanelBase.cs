using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities.UI;
using Common.Extensions;
using Common.Providers;
using Common.ScriptableObjects;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.PanelItems.Shop_Items;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels.ShopPanels
{
    public abstract class ShopDialogPanelBase<T> : DialogPanelBase where T : ShopItemBase
    {
        private IViewInputCommandsProceeder CommandsProceeder { get; }

        #region constants

        protected const string PrefabSetName = "shop_items";

        #endregion
        
        #region nonpublic members

        protected virtual  Vector2           StartContentPos     =>  Vector2.zero;
        protected abstract string            ItemSetName         { get; }
        protected abstract string            PanelPrefabName     { get; }
        protected abstract string            PanelItemPrefabName { get; }
        protected abstract RectTransformLite ShopItemRectLite    { get; }

        protected GameObject      PanelObj;
        protected RectTransform   Content;
        private   TextMeshProUGUI m_MoneyText;
        private   Image           m_MoneyIcon;

        #endregion

        #region inject

        protected ShopDialogPanelBase(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder) 
            : base(
                _Managers,
                _Ticker,
                _CameraProvider,
                _ColorProvider)
        {
            CommandsProceeder = _CommandsProceeder;
        }

        #endregion

        #region api

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var sp = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels,
                PanelPrefabName);
            Content = sp.GetCompItem<RectTransform>("content");
            var closeButton = sp.GetCompItem<Button>("close_button");
            closeButton.onClick.AddListener(OnButtonCloseClick);
            PanelRectTransform = sp.RTransform();
            Content.gameObject.DestroyChildrenSafe();
            InitItems();
            PanelObj = sp;
        }

        #endregion

        #region nonpublic methods

        protected virtual void OnButtonCloseClick()
        {
            base.OnClose(() =>
            {
                CommandsProceeder.RaiseCommand(
                    EInputCommand.UnPauseLevel, null, true);
            });
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }

        protected virtual void InitItems()
        {
            var set = Managers.PrefabSetManager.GetObject<ShopItemsScriptableObject>(
                PrefabSetName, ItemSetName).set;
            foreach (var itemInSet in set)
            {
                var item = CreateItem();
                var args = new ViewShopItemInfo
                {
                    BuyForWatchingAd = itemInSet.watchingAds,
                    Icon = itemInSet.icon,
                    Price = itemInSet.price.ToString()
                };
                item.Init(
                    Ticker,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    null,
                    args);
            }
        }

        protected T CreateItem()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    Content,
                    ShopItemRectLite),
                PrefabSetName, PanelItemPrefabName);
            return obj.GetComponent<T>();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.UI     when m_MoneyIcon.IsNotNull(): m_MoneyIcon.color = _Color; break;
                case ColorIds.UiText when m_MoneyText.IsNotNull(): m_MoneyText.color = _Color; break;
            }
        }

        #endregion
    }
}