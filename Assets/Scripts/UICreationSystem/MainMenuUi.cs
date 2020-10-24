using System.Collections.Generic;
using System.Linq;
using UICreationSystem.Factories;
using UICreationSystem.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UICreationSystem
{
    public class MainMenuUi
    {
        private readonly List<ChooseGameItemProps> m_CgiPropsList = new List<ChooseGameItemProps>
        {
            new ChooseGameItemProps(null, "POINT CLICKER", false, true, null),
            new ChooseGameItemProps(null, "FIGURE DRAWER", false, true, null),
            new ChooseGameItemProps(null, "MATH TRAIN", true, true, null),
            new ChooseGameItemProps(null, "TILE PATHER", true, false, null),
            new ChooseGameItemProps(null, "BALANCE DRAWER", true, false, null)
        };

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

        private readonly List<ShopItemProps> m_ShopItemPropsList = new List<ShopItemProps>
        {
            new ShopItemProps("No Ads", "9.99$", "20$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_0_icon")),
            new ShopItemProps("30,000", "9.99$", "20$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_1_icon")),
            new ShopItemProps("30", "9.99$", "20$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_2_icon")),
            new ShopItemProps("80,000", "19.99$", "40$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_3_icon")),
            new ShopItemProps("80", "19.99$", "40$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_4_icon")),
            new ShopItemProps("200,000", "39.99$", "80$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_5_icon")),
            new ShopItemProps("200", "39.99$", "80$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_6_icon")),
            new ShopItemProps("500,000", "79.99$", "180$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_7_icon")),
            new ShopItemProps("500", "79.99$", "180$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_8_icon"))
        };

        private static bool _isDailyBonusClicked;
        private Animator m_DailyBonusAnimator;
        private IDialogViewer m_DialogViewer;
        private RectTransform m_MainMenu;
        private RectTransform m_GameTitleContainer;
        private GameObject m_GameTitle;
        private System.Action m_OnLoginClick;

        public MainMenuUi(RectTransform _Parent,
            IDialogViewer _DialogViewer,
            System.Action _OnLoginClick)
        {
            InitContainers(_Parent);
            m_DialogViewer = _DialogViewer;
            m_OnLoginClick = _OnLoginClick;
            
            SetGameTitle(SaveUtils.GetValue<int>(SaveKey.GameId));
            InitCenterButtonsScrollView();
            InitBottomButtonsScrollView();
            InitSmallButtons();
            
            m_DialogViewer.SetNotDialogItems(new [] {m_MainMenu});

            CheckIfDailyBonusNotChosenToday();
        }

        private void InitContainers(RectTransform _Parent)
        {
            m_MainMenu = UiFactory.UiRectTransform(
                _Parent,
                "Main Menu",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero);

            m_GameTitleContainer = UiFactory.UiRectTransform(
                m_MainMenu,
                "Game Title Container",
                UiAnchor.Create(0.5f, 1f, 0.5f, 1f), 
                new Vector2(0, -47f),
                Vector2.one * 0.5f,
                new Vector2(486f, 92f));
        }

        private void SetGameTitle(int _GameId)
        {
            GameObject selectGameButon = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_GameTitleContainer,
                    UiAnchor.Create(1, 1, 1, 1),
                    new Vector2(-68f, -87f),
                    Vector2.one * 0.5f,
                    new Vector2(113f, 113f)),
                "main_menu_buttons",
                "select_game_button");
            selectGameButon.GetComponentItem<Button>("button").SetOnClick(OnOpenSelectGamePanel);

            var commonPos = new RectTransformLite
            {
                Anchor = UiAnchor.Create(0, 0, 1, 1),
                AnchoredPosition = Vector2.zero,
                Pivot = Vector2.one * 0.5f,
                SizeDelta = Vector2.zero
            };
            
            m_GameTitle = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(m_GameTitleContainer, commonPos),
                "main_menu", $"game_{_GameId}_title");
            
            SaveUtils.PutValue(SaveKey.GameId, _GameId);
        }

        private void InitCenterButtonsScrollView()
        {
            GameObject centerButtonsScrollView = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(0.5f, 0.5f, 0.5f, 0.5f),
                    Vector2.zero,
                    Vector2.one * 0.5f,
                    new Vector2(280, 499)),
                "main_menu", "center_buttons_scroll_view");
            
            RectTransform content = centerButtonsScrollView.GetComponentItem<RectTransform>("content");
            var rtrLite = new RectTransformLite
            {
                Anchor = UiAnchor.Create(0, 1, 0, 1),
                AnchoredPosition = new Vector2(243f, -539.5f),
                Pivot = Vector2.one * 0.5f,
                SizeDelta = new Vector2(280, 96)
            };
            
            var playButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    rtrLite),
                "main_menu_buttons",
                "play_button");
            playButton.GetComponent<Button>().SetOnClick(OnPlayButtonClick);
            
            var dailyBonusButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    rtrLite),
                "main_menu_buttons",
                "daily_bonus_button");
            
            dailyBonusButton.GetComponentItem<Button>("button").SetOnClick(OnDailyBonusButtonClick);
            m_DailyBonusAnimator = dailyBonusButton.GetComponentItem<Animator>("animator");
            
            var wheelOfFortuneButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    rtrLite),
                "main_menu_buttons",
                "wheel_of_fortune_button");
            wheelOfFortuneButton.GetComponent<Button>().SetOnClick(OnWheelOfFortuneButtonClick);
        }

        private void InitBottomButtonsScrollView()
        {
            GameObject bottomButtonsScrollView = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(0, 0, 1, 0),
                    Vector2.up * 85f,
                    Vector2.one * 0.5f,
                    Vector2.up * 170f),
                "main_menu",
                "bottom_buttons_scroll_view");
            
            RectTransform contentRtr = bottomButtonsScrollView.GetComponentItem<RectTransform>("content");
            
            var rTrLite = new RectTransformLite
            {
                Anchor = UiAnchor.Create(1, 0, 1, 0),
                AnchoredPosition = Vector2.zero,
                Pivot = Vector2.one * 0.5f,
                SizeDelta = Vector2.one * 100f
            };
            
            var profileButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(contentRtr, rTrLite),
                "main_menu_buttons",
                "profile_button");
            profileButton.GetComponent<Button>().SetOnClick(OnProfileButtonClick);

            var loginButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(contentRtr, rTrLite),
                "main_menu_buttons",
                "login_button");
            loginButton.GetComponent<Button>().SetOnClick(OnLoginButtonClick);

            var shopButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(contentRtr, rTrLite),
                "main_menu_buttons",
                "shop_button");
            shopButton.GetComponent<Button>().SetOnClick(OnShopButtonClick);
        }

        private void InitSmallButtons()
        {
            var settingsButtonSmall = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(1, 0, 1, 0),
                    new Vector2(-43, 35),
                    Vector2.one * 0.5f,
                    new Vector2(63, 54)),
                "main_menu_buttons",
                "settings_button_small");
            
            settingsButtonSmall.GetComponent<Button>().SetOnClick(OnSettingsButtonClick);
            settingsButtonSmall.SetParent(m_MainMenu);
        }

        private void CheckIfDailyBonusNotChosenToday()
        {
            System.DateTime lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.DailyBonusLastDate);
            if (lastDate.Date !=  System.DateTime.Now.Date)
                m_DailyBonusAnimator.SetTrigger(AnimKeys.Anim);
        }

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

        #region event methods

        private void OnOpenSelectGamePanel()
        {
            GameObject selectGamePanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "select_game_panel");
            RectTransform content = selectGamePanel.GetComponentItem<RectTransform>("content");
            
            GameObject cgiObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -43f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 66f)),
                "main_menu",
                "select_game_item");
            
            foreach (var cgiProps in m_CgiPropsList)
            {
                var cgiObjClone = cgiObj.Clone();
                ChooseGameItem cgi = cgiObjClone.GetComponent<ChooseGameItem>();
                cgi.Init(cgiProps);
            }
            
            Object.Destroy(cgiObj);
            m_DialogViewer.Show(null, selectGamePanel.RTransform());
        }
        
        private void OnProfileButtonClick()
        {
            //TODO profile button logic
        }

        private void OnSettingsButtonClick()
        {
            //TODO settings button logic
            SettingsPanel.CreatePanel(m_MainMenu);
        }

        private void OnLoginButtonClick()
        {
            m_OnLoginClick?.Invoke();
            LoginPanel.CreatePanel(m_MainMenu);
            //TODO login button logic
        }

        private void OnShopButtonClick()
        {
            GameObject shopPanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "shop_panel");
            RectTransform content = shopPanel.GetComponentItem<RectTransform>("content");
            
            GameObject shopItem = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -60f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 100f)),
                "main_menu",
                "shop_item");
            
            foreach (var shopItemProps in m_ShopItemPropsList)
            {
                if (shopItemProps.Amount == "No Ads" && !SaveUtils.GetValue<bool>(SaveKey.ShowAds))
                    continue;
                var shopItemClone = shopItem.Clone();
                ShopItem si = shopItemClone.GetComponent<ShopItem>();
                si.Init(shopItemProps);
            }
            
            Object.Destroy(shopItem);
            m_DialogViewer.Show(null, shopPanel.RTransform());
        }

        private void OnPlayButtonClick()
        {
            //TODO play button click
        }

        private void OnDailyBonusButtonClick()
        {
            int dailyBonusDay = GetCurrentDailyBonusDay();
            System.DateTime lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.DailyBonusLastDate);
            System.DateTime today = System.DateTime.Now.Date;
            
            if (lastDate.Date != today)
            {
                SaveUtils.PutValue(SaveKey.DailyBonusLastDate, System.DateTime.Today);
                m_DailyBonusAnimator.SetTrigger(AnimKeys.Stop);
            }

            GameObject dailyBonusPanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
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
            m_DialogViewer.Show(null, dailyBonusPanel.RTransform());
        }

        private void OnWheelOfFortuneButtonClick()
        {
            //TODO wheel of fortune button click
        }
        
        #endregion
    }
}