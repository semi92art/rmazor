using System;
using System.Collections.Generic;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze.Views.Common;
using Ticker;
using UI.Entities;
using UI.Factories;
using UI.PanelItems;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace UI.Panels
{
    public interface IDailyBonusDialogPanel : IDialogPanel
    {
        IAction Action { get; set; }
    }
    
    public class DailyBonusPanel : DialogPanelBase, IDailyBonusDialogPanel
    {
        #region nonpublic members
        
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
        private RectTransform m_Content;
        
        #endregion

        #region inject

        public DailyBonusPanel(
            IBigDialogViewer _DialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider)
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider, _ColorProvider) { }

        #endregion
        
        #region api

        public IAction Action { get; set; }
        public override EUiCategory Category => EUiCategory.DailyBonus;
        
        public override void Init()
        {
            base.Init();
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels,
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
                CommonPrefabSetNames.MainMenu,
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
                    Managers.Notify(_SM => _SM.PlayClip(AudioClipNames.UIButtonClick));
                    DialogViewer.CloseAll();
                };
                
                dbi.Init(dbProps, Action);
            }
            Object.Destroy(dbItem);
        }

        private bool IsDailyBonusGotToday()
        {
            var lastDate = SaveUtils.GetValue<DateTime>(SaveKey.DailyBonusLastDate);
            var today = DateTime.Now.Date;
            int daysPast = Mathf.FloorToInt((float)(today - lastDate.Date).TotalDays);
            return daysPast == 0;
        }
        
        private int GetCurrentDailyBonusDay()
        {
            int lastItemClickedDay = SaveUtils.GetValue<int>(SaveKey.DailyBonusLastClickedDay);
            return lastItemClickedDay + 1;
        }
        
        #endregion
    }
}