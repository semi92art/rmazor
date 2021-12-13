using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze.Views.Common;
using Managers;
using ScriptableObjects;
using Ticker;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.PanelItems.Shop_Items;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels.ShopPanels
{
    public abstract class ShopPanelBase<T> : DialogPanelBase where T : ShopItemBase
    {
        #region constants

        protected const string PrefabSetName = "shop_items";

        #endregion
        
        #region nonpublic members

        protected virtual  Vector2           StartContentPos     =>  Vector2.zero;
        protected abstract string            ItemSetName         { get; }
        protected abstract string            PanelPrefabName     { get; }
        protected abstract string            PanelItemPrefabName { get; }
        protected abstract RectTransformLite ShopItemRectLite    { get; }
        
        protected          RectTransform     m_Panel;
        protected          RectTransform     m_Content;
        protected          TextMeshProUGUI   m_MoneyText;
        protected          Image             m_MoneyIcon;

        #endregion

        #region inject

        protected ShopPanelBase(
            IManagersGetter _Managers,
            IUITicker _Ticker, 
            IBigDialogViewer _DialogViewer, 
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider) 
            : base(_Managers, _Ticker, _DialogViewer, _CameraProvider, _ColorProvider)
        { }

        #endregion

        #region api

        public override void LoadPanel()
        {
            base.LoadPanel();
            var sp = Managers.PrefabSetManager.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels,
                PanelPrefabName);
            m_Panel = sp.GetCompItem<RectTransform>("panel");
            m_Content = sp.GetCompItem<RectTransform>("content");
            SetTranslucentBackgroundSource(sp);
            Panel = sp.RTransform();
            m_Content.gameObject.DestroyChildrenSafe();
            InitItems();
            InitMoneyPanel();
            Coroutines.Run(Coroutines.WaitEndOfFrame(
                () => m_Content.anchoredPosition = StartContentPos));
        }

        #endregion

        #region nonpublic methods

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
                    UnlockingLevel = itemInSet.unlockingLevel,
                    Price = itemInSet.price.ToString()
                };
                item.Init(
                    Managers,
                    Ticker,
                    ColorProvider,
                    () =>
                    {
                        
                    },
                    args);
            }
        }
        
        protected void InitMoneyPanel()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Panel,
                    new RectTransformLite
                    {
                        Anchor = UiAnchor.Create(0, 1, 0, 1),
                        AnchoredPosition = new Vector2(180, -84),
                        Pivot = Vector2.one * 0.5f,
                        SizeDelta = new Vector2(306f, 66f)
                    }),
                "ui_game", "money_mini_panel");
            m_MoneyText = obj.GetCompItem<TextMeshProUGUI>("money");
            m_MoneyIcon = obj.GetCompItem<Image>("icon");
            m_MoneyText.color = ColorProvider.GetColor(ColorIds.UiText);
            m_MoneyIcon.color = ColorProvider.GetColor(ColorIds.UI);
            var moneyEntity = Managers.ScoreManager.GetScore(DataFieldIds.Money, true);
            Coroutines.Run(Coroutines.WaitWhile(
                () => moneyEntity.Result == EEntityResult.Pending,
                () =>
                {
                    if (moneyEntity.Result == EEntityResult.Fail)
                    {
                        Dbg.LogError("Failed to load money entity");
                        return;
                    }
                    m_MoneyText.text = moneyEntity.GetFirstScore().ToString();
                }));
            Managers.ScoreManager.OnScoresChanged -= OnScoreChanged; 
            Managers.ScoreManager.OnScoresChanged += OnScoreChanged;
        }
        
        private void OnScoreChanged(ScoresEventArgs _Args)
        {
            ShopUtils.OnScoreChanged(_Args, m_MoneyText);
        }
        
        protected T CreateItem()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    ShopItemRectLite),
                PrefabSetName, PanelItemPrefabName);
            return obj.GetComponent<T>();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.UI)
                m_MoneyIcon.color = _Color;
            else if (_ColorId == ColorIds.UiText)
                m_MoneyText.color = _Color;
        }

        #endregion
    }
}