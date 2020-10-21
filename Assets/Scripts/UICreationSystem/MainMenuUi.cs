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
            new ChooseGameItemProps
            {
                Icon = null,
                Title = "POINT CLICKER",
                IsComingSoon = false,
                IsVisible = true,
                OnClick = null
            },
            new ChooseGameItemProps
            {
                Icon = null,
                Title = "FIGURE DRAWER",
                IsComingSoon = false,
                IsVisible = true,
                OnClick = null
            },
            new ChooseGameItemProps
            {
                Icon = null,
                Title = "MATH TRAIN",
                IsComingSoon = true,
                IsVisible = true,
                OnClick = null
            },
            new ChooseGameItemProps
            {
                Icon = null,
                Title = "TILE PATHER",
                IsComingSoon = true,
                IsVisible = false,
                OnClick = null
            },
            new ChooseGameItemProps
            {
                Icon = null,
                Title = "BALANCE DRAWER",
                IsComingSoon = true,
                IsVisible = false,
                OnClick = null
            }
        };

        private IDialogViewer m_DialogViewer;
        private RectTransform m_MainMenu;
        private RectTransform m_GameTitleContainer;
        private GameObject m_GameTitle;
        private System.Action m_OnLoginClick;

        public MainMenuUi(RectTransform _Parent,
            IDialogViewer _DialogViewer,
            System.Action _OnLoginClick
            )
        {
            InitContainers(_Parent);
            m_DialogViewer = _DialogViewer;
            m_OnLoginClick = _OnLoginClick;
            
            SetGameTitle(SaveUtils.GetValue<int>(SaveKey.GameId));
            InitButtonsScrollView();
            InitSmallButtons();
            InitPlayButton();
            
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
                "Game Title",
                UiAnchor.Create(0.5f, 1f, 0.5f, 1f), 
                new Vector2(0, -47f),
                Vector2.one * 0.5f,
                new Vector2(511f, 92f));
        }

        private void SetGameTitle(int _GameId)
        {
            GameObject selectGameButon = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_GameTitleContainer,
                    "Select Game Button",
                    UiAnchor.Create(1, 1, 1, 1),
                    new Vector2(-56f, -56f),
                    Vector2.one * 0.5f,
                    new Vector2(113f, 113f)),
                "main_menu",
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
                UiFactory.UiRectTransform(
                    m_GameTitleContainer,
                    "Point Clicker Title",
                    commonPos),
                "main_menu", $"game_{_GameId}_title");
            
            SaveUtils.PutValue(SaveKey.GameId, _GameId);
        }

        private void InitButtonsScrollView()
        {
            GameObject buttonsScrollView = PrefabInitializer.InitUiPrefab(UiFactory.UiRectTransform(
                    m_MainMenu,
                    "Bottom Buttons Scroll View",
                    UiAnchor.Create(0, 0, 1, 0),
                    Vector2.up * 120f,
                    Vector2.one * 0.5f,
                    Vector2.up * 240f),
                "main_menu",
                "buttons_scroll_view");
            
            RectTransform contentRtr = buttonsScrollView.GetContentItemRTransform("content");
            
            var sizeDelta = Vector2.one * 150;
            
            var profileButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    "Profile Button",
                    UiAnchor.Create(0, 0, 0, 0),
                    new Vector2(150, 130),
                    Vector2.one * 0.5f,
                    sizeDelta),
                "main_menu",
                "profile_button");
            
            profileButton.GetComponent<Button>().SetOnClick(OnProfileButtonClick);
            profileButton.SetParent(contentRtr);

            var loginButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    "Settings Button",
                    UiAnchor.Create(1, 0, 1, 0),
                    new Vector2(-150, 130),
                    Vector2.one * 0.5f,
                    sizeDelta),
                "main_menu",
                "login_button");
            
            loginButton.GetComponent<Button>().SetOnClick(OnLoginButtonClick);
            loginButton.SetParent(contentRtr);
        }

        private void InitSmallButtons()
        {
            var settingsButtonSmall = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    "Settings Button",
                    UiAnchor.Create(1, 0, 1, 0),
                    new Vector2(-43, 35),
                    Vector2.one * 0.5f,
                    new Vector2(63, 54)),
                "main_menu",
                "settings_button_small");
            
            settingsButtonSmall.GetComponent<Button>().SetOnClick(OnSettingsButtonClick);
            settingsButtonSmall.SetParent(m_MainMenu);
        }

        private void InitPlayButton()
        {
            var playButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    "Play Button",
                    UiAnchor.Create(0.5f, 0.5f, 0.5f, 0.5f),
                    Vector2.down * 12.6f,
                    Vector2.one * 0.5f,
                    new Vector2(280f, 96f)),
                "main_menu",
                "play_button");
            
            playButton.GetComponent<Button>().SetOnClick(OnPlayButtonClick);
            playButton.SetParent(m_MainMenu);
        }
        
        #region event methods

        private void OnSelectGameClick()
        {
            GameObject selectGamePanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    "Select Game Panel",
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
                    "Choose Game Item",
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

        private void OnLotteryButtonClick()
        {
            //TODO lottery button logic
        }

        private void OnPlayButtonClick()
        {
            //TODO play button click
        }
        
        #endregion
    }
}