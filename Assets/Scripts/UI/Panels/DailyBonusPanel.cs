using System.Collections.Generic;
using Constants;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using Utils;

namespace UI.Panels
{
    public class DailyBonusPanel : DialogPanelBase, IMenuUiCategory
    {
        #region private members
        
        private readonly List<DailyBonusProps> m_DailyBonusPropsList = new List<DailyBonusProps>
        {
            new DailyBonusProps( 1, 300, _Icon: PrefabUtilsEx.GetObject<Sprite>("daily_bonus", "day_1_icon")),
            new DailyBonusProps( 2, 1000, _Icon: PrefabUtilsEx.GetObject<Sprite>("daily_bonus", "day_2_icon")),
            new DailyBonusProps( 3, _Diamonds: 3, _Icon: PrefabUtilsEx.GetObject<Sprite>("daily_bonus", "day_3_icon")),
            new DailyBonusProps( 4, 3000, _Icon: PrefabUtilsEx.GetObject<Sprite>("daily_bonus", "day_4_icon")),
            new DailyBonusProps( 5, _Diamonds: 5, _Icon: PrefabUtilsEx.GetObject<Sprite>("daily_bonus", "day_5_icon")),
            new DailyBonusProps( 6, 5000, _Icon: PrefabUtilsEx.GetObject<Sprite>("daily_bonus", "day_6_icon")),
            new DailyBonusProps( 7, _Diamonds: 8, _Icon: PrefabUtilsEx.GetObject<Sprite>("daily_bonus", "day_7_icon")),
        };

        private RectTransform m_Panel;
        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly IActionExecutor m_ActionExecutor;
        private RectTransform m_Content;
        
        #endregion
        
        #region api

        public MenuUiCategory Category => MenuUiCategory.DailyBonus;

        public DailyBonusPanel(
            IMenuDialogViewer _DialogViewer, 
            IActionExecutor _ActionExecutor)
        {
            m_DialogViewer = _DialogViewer;
            m_ActionExecutor = _ActionExecutor;
        }

        public override void Init()
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                CommonStyleNames.MainMenuDialogPanels,
                "daily_bonus_panel");
            m_Content = go.GetCompItem<RectTransform>("content");
            CreateItems();
            Panel = go.RTransform();
        }

        #endregion
        
        #region nonpublic methods

        private void CreateItems()
        {
            GameObject dbItem = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -60f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 100f)),
                CommonStyleNames.MainMenu,
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
                    Notify(this, CommonNotifyMessages.UiButtonClick, dbProps.Day);
                    m_DialogViewer.Back();
                };
                
                dbi.Init(dbProps, m_ActionExecutor);
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