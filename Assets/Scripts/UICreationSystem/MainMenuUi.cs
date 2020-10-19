using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UICreationSystem
{
    public class MainMenuUi
    {
        private RectTransform m_MainMenu;
        private RectTransform m_GameTitleContainer;
        private RectTransform m_DialogContainer;
        private GameObject m_GameTitle;
        private System.Action m_OnLoginClick;

        public MainMenuUi(RectTransform _Parent,
            RectTransform _DialogContainer,
            System.Action _OnLoginClick
            )
        {
            InitContainers(_Parent);
            m_DialogContainer = _DialogContainer;
            m_OnLoginClick = _OnLoginClick;
            
            SetGameTitle(SaveUtils.GetValue<int>(SaveKey.GameId));
            InitButtonsScrollView();
            InitSmallButtons();
            InitPlayButton();
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
                UiAnchor.Create(new Vector2(0.5f, 1.0f), new Vector2(0.5f, 1.0f)),
                new Vector2(0, -93f),
                Vector2.one * 0.5f,
                new Vector2(270f, 185f));
        }

        private void SetGameTitle(int _GameId)
        {
            switch (_GameId)
            {
                case 1:
                    m_GameTitle = PrefabInitializer.InitUiPrefab(
                        UiFactory.UiRectTransform(
                            m_GameTitleContainer,
                            "Pimple Killer Title",
                            UiAnchor.Create(Vector2.zero, Vector2.one), 
                            Vector2.zero, 
                            Vector2.one * 0.5f,
                            Vector2.zero),
                        "pimple_killer_main_menu", "game_title");
                    break;
                default:
                    SaveUtils.PutValue(SaveKey.GameId, 1); //TODO put id of default game (depends from build)
                    SetGameTitle(1);
                    break;
            }
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