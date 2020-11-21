using System.Linq;
using Boo.Lang;
using DialogViewers;
using Entities;
using Extensions;
using Helpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using Utils;

namespace UI.Panels
{
    public class DailyBonusPanel : IMenuDialogPanel
    {
        #region private members
        
        private readonly List<DailyBonusProps> m_DailyBonusPropsList = new List<DailyBonusProps>
        {
            new DailyBonusProps( 1, 300, _Icon: PrefabInitializer.GetObject<Sprite>("daily_bonus", "day_1_icon")),
            new DailyBonusProps( 2, 1000, _Icon: PrefabInitializer.GetObject<Sprite>("daily_bonus", "day_2_icon")),
            new DailyBonusProps( 3, _Diamonds: 3, _Icon: PrefabInitializer.GetObject<Sprite>("daily_bonus", "day_3_icon")),
            new DailyBonusProps( 4, 3000, _Icon: PrefabInitializer.GetObject<Sprite>("daily_bonus", "day_4_icon")),
            new DailyBonusProps( 5, _Diamonds: 5, _Icon: PrefabInitializer.GetObject<Sprite>("daily_bonus", "day_5_icon")),
            new DailyBonusProps( 6, 5000, _Icon: PrefabInitializer.GetObject<Sprite>("daily_bonus", "day_6_icon")),
            new DailyBonusProps( 7, _Diamonds: 8, _Icon: PrefabInitializer.GetObject<Sprite>("daily_bonus", "day_7_icon")),
        };

        private RectTransform m_Panel;
        private readonly IMenuDialogViewer m_MenuDialogViewer;
        private readonly IActionExecuter m_ActionExecuter;
        private RectTransform m_Content;
        
        #endregion
        
        #region api

        public MenuUiCategory Category => MenuUiCategory.DailyBonus;
        public RectTransform Panel { get; private set; }

        public DailyBonusPanel(IMenuDialogViewer _MenuDialogViewer, IActionExecuter _ActionExecuter)
        {
            m_MenuDialogViewer = _MenuDialogViewer;
            m_ActionExecuter = _ActionExecuter;
        }
        
        public void Show()
        {
            Panel = Create();
            m_MenuDialogViewer.Show( this);
        }
        
        #endregion
        
        #region private methods
        
        private RectTransform Create()
        {
            GameObject dailyBonusPanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MenuDialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "daily_bonus_panel");
            m_Content = dailyBonusPanel.GetCompItem<RectTransform>("content");

            CreateItems();
            return dailyBonusPanel.RTransform();
        }


        private void CreateItems()
        {
            GameObject dbItem = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -60f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 100f)),
                "main_menu",
                "daily_bonus_item");
            
            int dailyBonusDay = GetCurrentDailyBonusDay();
            
            foreach (var dbProps in m_DailyBonusPropsList)
            {
                if (dbProps.Day == dailyBonusDay && !IsDailyBonusGotToday())
                    dbProps.IsActive = true;
                if (dbProps.Day == dailyBonusDay + 1 && !IsDailyBonusGotToday()
                    || dbProps.Day == dailyBonusDay && IsDailyBonusGotToday())
                    dbProps.IsTomorrow = true;
                var dbItemClone = dbItem.Clone();
                DailyBonusItem dbi = dbItemClone.GetComponent<DailyBonusItem>();

                dbProps.Click = () =>
                {
                    m_MenuDialogViewer.Back();
                };
                
                dbi.Init(dbProps, m_ActionExecuter);
            }
            
            Object.Destroy(dbItem);
        }

        private bool IsDailyBonusGotToday()
        {
            var lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.DailyBonusLastDate);
            var today = System.DateTime.Now.Date;
            int daysPast = Mathf.FloorToInt((float)(today - lastDate.Date).TotalDays);
            return daysPast == 0;
        }
        
        private int GetCurrentDailyBonusDay()
        {
            int lastItemClickedDay = SaveUtils.GetValue<int>(SaveKey.DailyBonusLastItemClickedDay);
            return lastItemClickedDay + 1;
        }
        
        #endregion
    }
}