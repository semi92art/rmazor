using System.Collections.Generic;
using Constants;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using Managers;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;
using UnityGameLoopDI;

namespace UI.Panels
{
    public class PlusMoneyPanel : DialogPanelBase, IMenuUiCategory
    {
        #region notify ids

        public const string NotifyMessageShopButtonClick = nameof(NotifyMessageShopButtonClick);
        public const string NotifyMessageDailyBonusButtonClick = nameof(NotifyMessageDailyBonusButtonClick);
        public const string NotifyMessageWheelOfFortuneButtonClick = nameof(NotifyMessageWheelOfFortuneButtonClick);
        
        #endregion
        
        #region nonpublic members

        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly INotificationViewer m_NotificationViewer;
        private readonly IActionExecutor m_ActionExecutor;
        
        #endregion

        #region api
        
        public MenuUiCategory Category => MenuUiCategory.PlusMoney;
        
        public PlusMoneyPanel(
            IMenuDialogViewer _DialogViewer,
            INotificationViewer _NotificationViewer,
            IActionExecutor _ActionExecutor,
            ITicker _Ticker) : base(_Ticker)
        {
            m_DialogViewer = _DialogViewer;
            m_NotificationViewer = _NotificationViewer;
            m_ActionExecutor = _ActionExecutor;
        }
        
        public override void Init()
        {
            GameObject go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.MainMenuDialogPanels,
                "plus_money_panel");
            
            go.GetCompItem<Button>("shop_button").SetOnClick(OnShopButtonClick);
            go.GetCompItem<Button>("daily_bonus_button").SetOnClick(OnDailyBonusButtonClick);
            go.GetCompItem<Button>("wheel_of_fortune_button").SetOnClick(OnWheelOfFortuneButtonClick);
            Panel = go.RTransform();
        }

        #endregion

        #region nonpublic methods

        private void OnShopButtonClick()
        {
            Notify(this, NotifyMessageShopButtonClick);
            var panel = new ShopPanel(m_DialogViewer.Container, Ticker);
            panel.AddObservers(GetObservers());
            panel.Init();
            m_DialogViewer.Show(panel);
        }

        private void OnDailyBonusButtonClick()
        {
            Notify(this, NotifyMessageDailyBonusButtonClick);
            var panel = new DailyBonusPanel(m_DialogViewer, m_ActionExecutor, Ticker);
            panel.AddObservers(GetObservers());
            panel.Init();
            m_DialogViewer.Show(panel);
        }

        private void OnWheelOfFortuneButtonClick()
        {
            Notify(this, NotifyMessageWheelOfFortuneButtonClick);
            var panel = new WheelOfFortunePanel(m_DialogViewer, m_NotificationViewer, Ticker);
            panel.AddObservers(GetObservers());
            panel.Init();
            m_DialogViewer.Show(panel);
        }

        #endregion
    }
}