using System.Linq;
using Boo.Lang;
using Extentions;
using UICreationSystem.Factories;
using UnityEngine;
using Utils;

namespace UICreationSystem.Panels
{
    public class DailyBonusPanel : IDialogPanel
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
        private readonly IDialogViewer m_DialogViewer;
        private readonly IActionExecuter m_ActionExecuter;
        private RectTransform m_Content;
        
        #endregion
        
        #region api

        public UiCategory Category => UiCategory.DailyBonus;
        public RectTransform Panel { get; private set; }

        public DailyBonusPanel(IDialogViewer _DialogViewer, IActionExecuter _ActionExecuter)
        {
            m_DialogViewer = _DialogViewer;
            m_ActionExecuter = _ActionExecuter;
        }
        
        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show( this);
        }
        
        #endregion
        
        #region private methods
        
        private RectTransform Create()
        {
            GameObject dailyBonusPanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "daily_bonus_panel");
            m_Content = dailyBonusPanel.GetComponentItem<RectTransform>("content");

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
            int k = 0;
            foreach (var dbProps in m_DailyBonusPropsList)
            {
                if (dbProps.Day == dailyBonusDay)
                    dbProps.IsActive = true;
                else if (dbProps.Day == dailyBonusDay + 1)
                    dbProps.IsTomorrow = true;
                var dbItemClone = dbItem.Clone();
                DailyBonusItem dbi = dbItemClone.GetComponent<DailyBonusItem>();

                dbProps.Click = () =>
                {
                    SaveUtils.PutValue(SaveKey.DailyBonusLastItemClickedDay, k++);
                    m_DialogViewer.Show( null, true);
                };
                
                dbi.Init(dbProps, m_ActionExecuter);
            }
            
            Object.Destroy(dbItem);
        }
        
        private int GetCurrentDailyBonusDay()
        {
            int dailyBonusDay = 0;
            System.DateTime lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.DailyBonusLastDate);
            System.DateTime today = System.DateTime.Now.Date;
            int daysPast = Mathf.FloorToInt((float)(today - lastDate.Date).TotalDays);
            if (daysPast > 0)
            {
                dailyBonusDay = 1;
                int lastItemClickedDay = SaveUtils.GetValue<int>(SaveKey.DailyBonusLastItemClickedDay);

                if (daysPast == 1 || SaveUtils.GetValue<bool>(SaveKey.DailyBonusOnDebug))
                {
                    dailyBonusDay = lastItemClickedDay + 1;
                    SaveUtils.PutValue(SaveKey.DailyBonusOnDebug, false);
                }

                if (dailyBonusDay > m_DailyBonusPropsList.Max(_Props => _Props.Day))
                    dailyBonusDay = 1;
            }

            return dailyBonusDay;
        }
        
        #endregion
    }
}