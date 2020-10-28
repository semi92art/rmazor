using System.Linq;
using Boo.Lang;
using Extentions;
using UICreationSystem.Factories;
using UnityEngine;
using Utils;

namespace UICreationSystem.Panels
{
    public class DailyBonusPanel
    {
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
        
        private int GetCurrentDailyBonusDay()
        {
            int dailyBonusDay = 0;
            System.DateTime lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.DailyBonusLastDate);
            System.DateTime today = System.DateTime.Now.Date;
            int daysPast = Mathf.FloorToInt((float)(lastDate.Date - today).TotalDays);
            if (daysPast > 0)
            {
                dailyBonusDay = 1;
                int lastItemClickedDay = SaveUtils.GetValue<int>(SaveKey.DailyBonusLastItemClickedDate);

                if (daysPast == 1)
                    dailyBonusDay = lastItemClickedDay + 1;

                if (dailyBonusDay > m_DailyBonusPropsList.Max(_Props => _Props.Day))
                    dailyBonusDay = 0;
            }

            return dailyBonusDay;
        }

        public RectTransform Create(IDialogViewer _DialogViewer, Animator _IconAnimator)
        {
            int dailyBonusDay = GetCurrentDailyBonusDay();
            System.DateTime lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.DailyBonusLastDate);
            System.DateTime today = System.DateTime.Now.Date;
            
            if (lastDate.Date != today)
            {
                SaveUtils.PutValue(SaveKey.DailyBonusLastDate, System.DateTime.Today);
                _IconAnimator.SetTrigger(AnimKeys.Stop);
            }

            GameObject dailyBonusPanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "daily_bonus_panel");
            RectTransform content = dailyBonusPanel.GetComponentItem<RectTransform>("content");
            
            GameObject dbItem = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -60f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 100f)),
                "main_menu",
                "daily_bonus_item");
            
            foreach (var dbProps in m_DailyBonusPropsList)
            {
                if (dbProps.Day == dailyBonusDay)
                    dbProps.IsActive = true;
                var dbItemClone = dbItem.Clone();
                DailyBonusItem dbi = dbItemClone.GetComponent<DailyBonusItem>();
                dbi.Init(dbProps);
            }
            
            Object.Destroy(dbItem);
            return dailyBonusPanel.RTransform();
        }
    }
}