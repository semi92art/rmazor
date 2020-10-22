using System.Collections.Generic;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UICreationSystem
{
    public class MainMenuUi
    {
        private List<ChooseGameItemProps> m_cgiPropsList = new List<ChooseGameItemProps>
        {
            new ChooseGameItemProps(null, "POINT CLICKER", false, true, null),
            new ChooseGameItemProps(null, "FIGURE DRAWER", false, true, null),
            new ChooseGameItemProps(null, "MATH TRAIN", true, true, null),
            new ChooseGameItemProps(null, "TILE PATHER", true, false, null),
            new ChooseGameItemProps(null, "BALANCE DRAWER", true, false, null)
        };

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
                    new Vector2(-56f, -56f),
                    Vector2.one * 0.5f,
                    new Vector2(113f, 113f)),
                "main_menu_buttons",
                "select_game_button");
            selectGameButon.GetContentItemButton("button").SetOnClick(OnSelectGameClick);

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
            
            RectTransform content = centerButtonsScrollView.GetContentItemRTransform("content");
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
            dailyBonusButton.GetComponent<Button>().SetOnClick(OnDailyBonusButtonClick);
            
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
            
            RectTransform contentRtr = bottomButtonsScrollView.GetContentItemRTransform("content");
            
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

        #region event methods

        private void OnSelectGameClick()
        {
            GameObject selectGamePanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "select_game_panel");
            RectTransform content = selectGamePanel.GetContentItemRTransform("content");
            Button backButton = selectGamePanel.GetContentItemButton("back_button");
            backButton.SetOnClick(() =>
            {
                m_DialogViewer.Show(selectGamePanel.RTransform(), null, 0.2f, true);
            });

            GameObject cgiObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -43f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 66f)),
                "main_menu",
                "select_game_item");
            
            foreach (var cgiProps in m_cgiPropsList)
            {
                var cgiObjClone = cgiObj.Clone();
                ChooseGameItem cgi = cgiObjClone.GetComponent<ChooseGameItem>();
                cgi.Init(cgiProps);
            }
            
            Object.Destroy(cgiObj);
            m_DialogViewer.Show(null, selectGamePanel.RTransform(), 0.2f);
        }
        
        private void OnProfileButtonClick()
        {
            //TODO profile button logic
        }

        private void OnSettingsButtonClick()
        {
            //TODO settings button logic
        }

        private void OnLoginButtonClick()
        {
            m_OnLoginClick?.Invoke();
            //TODO login button logic
        }

        private void OnShopButtonClick()
        {
            //TODO shop button logic
        }

        private void OnPlayButtonClick()
        {
            //TODO play button click
        }

        private void OnDailyBonusButtonClick()
        {
            //TODO daily bonus button click
        }

        private void OnWheelOfFortuneButtonClick()
        {
            //TODO wheel of fortune button click
        }
        
        #endregion
    }
}