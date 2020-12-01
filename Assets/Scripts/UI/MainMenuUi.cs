using Constants;
using DialogViewers;
using Entities;
using Extensions;
using Helpers;
using Lean.Touch;
using Managers;
using Network;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class MainMenuUi : DI.DiObject
    {
        #region nonpublic members
        
        private static bool _isDailyBonusClicked;
        private readonly IMenuDialogViewer m_MenuDialogViewer;
        private IMiniPanel m_BankMiniPanel;
        
        private RectTransform m_Parent;
        private RectTransform m_MainMenu;
        private RectTransform m_GameLogoContainer;
        private Animator m_DailyBonusAnimator;
        private MenuUiCategory m_CurrentCategory;
        private Button m_SelectGameButton;
        private Image m_GameLogo;
        
        #endregion

        #region factory method and constructor

        public static MainMenuUi Create(
            RectTransform _Parent,
            IMenuDialogViewer _MenuDialogViewer)
        {
            return new MainMenuUi(_Parent, _MenuDialogViewer);
        }
        
        private MainMenuUi(
            RectTransform _Parent,
            IMenuDialogViewer _MenuDialogViewer)
        {
            m_MenuDialogViewer = _MenuDialogViewer;
            UiManager.Instance.CurrentMenuCategory = MenuUiCategory.MainMenu;
            InitContainers(_Parent);
            InitSelectGameButton();
            SetGameLogo(GameClient.Instance.GameId);
            InitCenterButtonsScrollView();
            InitBottomButtonsScrollView();
            InitSmallButtons();
            InitBankMiniPanel();
            CheckIfDailyBonusNotChosenToday();
            m_MenuDialogViewer.AddNotDialogItem(m_MainMenu, MenuUiCategory.MainMenu);
            SoundManager.Instance.PlayClip("main_menu_theme", true, 0f);
        }

        #endregion
        
        #region nonpublic methods

        private void InitBankMiniPanel()
        {
            m_BankMiniPanel = new BankMiniPanel(m_Parent, m_MenuDialogViewer);
            m_BankMiniPanel.Show();
        }

        private void InitContainers(RectTransform _Parent)
        {
            m_Parent = _Parent;
            m_MainMenu = UiFactory.UiRectTransform(
                _Parent,
                "Main Menu",
                UiAnchor.Create(0, 0, 1, 1),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero);

            var activeStateWatcher = m_MainMenu.gameObject.AddComponent<ActiveStateWatcher>();
            activeStateWatcher.OnActiveStateChanged += _Value =>
            {
                if (_Value)
                    CheckIfDailyBonusNotChosenToday();
            };

            m_GameLogoContainer = UiFactory.UiRectTransform(
                m_MainMenu,
                "Game Logo Container",
                UiAnchor.Create(0.5f, 1f, 0.5f, 1f), 
                new Vector2(0, -209f),
                Vector2.one * 0.5f,
                new Vector2(486f, 198.4f));

            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_GameLogoContainer,
                    UiAnchor.Create(0, 0, 1, 1),
                    Vector2.zero,
                    Vector2.one * 0.5f,
                    Vector2.zero),
                "game_logos", "game_logo_container");
            m_GameLogo = go.GetCompItem<Image>("logo");
        }

        private void InitSelectGameButton()
        {
            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(0.5f, 1, 0.5f, 1),
                    new Vector2(0, -57f),
                    Vector2.one * 0.5f,
                    new Vector2(113f, 113f)),
                "main_menu_buttons",
                "select_game_button");
            m_SelectGameButton = go.GetCompItem<Button>("button");
            m_SelectGameButton.SetOnClick(OnOpenSelectGamePanel);
        }

        private void SetGameLogo(int _GameId)
        {
            m_GameLogo.sprite = PrefabInitializer.GetObject<Sprite>(
                    "game_logos", $"game_logo_{GameClient.Instance.GameId}");
        }

        private void InitCenterButtonsScrollView()
        {
            GameObject centerButtonsScrollView = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(0.5f, 0.5f, 0.5f, 0.5f),
                    Vector2.up * -65f,
                    Vector2.one * 0.5f,
                    new Vector2(280, 360)),
                "main_menu", "center_buttons_scroll_view");
            
            RectTransform content = centerButtonsScrollView.GetCompItem<RectTransform>("content");
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
            
            var ratingsButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    rtrLite),
                "main_menu_buttons",
                "ratings_button");
            ratingsButton.GetComponent<Button>().SetOnClick(OnRatingsClick);
            
        }

        private void InitBottomButtonsScrollView()
        {
            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(0, 0, 1, 0),
                    Vector2.up * 123f,
                    Vector2.one * 0.5f,
                    Vector2.up * 100f),
                "main_menu",
                "bottom_buttons_scroll_view");
            RectTransform contentRtr = go.GetCompItem<RectTransform>("content");
            
            var rTrLite = new RectTransformLite
            {
                Anchor = UiAnchor.Create(1, 0, 1, 0),
                AnchoredPosition = Vector2.zero,
                Pivot = Vector2.one * 0.5f,
                SizeDelta = Vector2.one * 100f
            };

            var loginButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(contentRtr, rTrLite),
                "main_menu_buttons",
                "login_button");
            loginButton.GetComponent<Button>().SetOnClick(OnLoginButtonClick);

            var dailyBonusButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(contentRtr, rTrLite),
                "main_menu_buttons",
                "daily_bonus_button_2");
            dailyBonusButton.GetCompItem<Button>("button").SetOnClick(OnDailyBonusButtonClick);
            m_DailyBonusAnimator = dailyBonusButton.GetCompItem<Animator>("animator");

            var shopButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(contentRtr, rTrLite),
                "main_menu_buttons",
                "shop_button");
            shopButton.GetComponent<Button>().SetOnClick(OnShopButtonClick);
            
            go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(0, 0, 1, 0),
                    Vector2.up * 247,
                    Vector2.one * 0.5f,
                    Vector2.up * 100f),
                "main_menu",
                "bottom_buttons_scroll_view");
            contentRtr = go.GetCompItem<RectTransform>("content");
            
            var wofButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    contentRtr,
                    rTrLite),
                "main_menu_buttons",
                "wheel_of_fortune_button_2");
            wofButton.GetComponent<Button>().SetOnClick(OnWheelOfFortuneButtonClick);
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
        }

        private void CheckIfDailyBonusNotChosenToday()
        {
            System.DateTime lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.DailyBonusLastDate);
            m_DailyBonusAnimator.SetTrigger(lastDate.Date == System.DateTime.Now.Date ?
                AnimKeys.Stop : AnimKeys.Anim);
        }
        
        #endregion

        #region event methods

        private void OnOpenSelectGamePanel()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IMenuDialogPanel selectGame = new SelectGamePanel(m_MenuDialogViewer, SetGameLogo);
            selectGame.Show();
        }
        
        private void OnProfileButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IMenuDialogPanel profile = new ProfilePanel(m_MenuDialogViewer);
            profile.Show();
        }

        private void OnSettingsButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IMenuDialogPanel settings = new SettingsPanel(m_MenuDialogViewer);
            settings.Show();
        }

        private void OnLoginButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IMenuDialogPanel login = new LoginPanel(m_MenuDialogViewer);
            login.Show();
        }

        private void OnShopButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IMenuDialogPanel shop = new ShopPanel(m_MenuDialogViewer);
            shop.Show();
        }

        private void OnPlayButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            (m_BankMiniPanel as BankMiniPanel)?.UnregisterFromEvents();
            LevelLoader.LoadLevel(1);
        }

        private void OnRatingsClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            // TODO
        }

        private void OnDailyBonusButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IMenuDialogPanel dailyBonus = new DailyBonusPanel(
                m_MenuDialogViewer, (IActionExecuter)m_BankMiniPanel);
            dailyBonus.Show();
        }

        private void OnWheelOfFortuneButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IMenuDialogPanel wheelOfFortune = new WheelOfFortunePanel(m_MenuDialogViewer);
            wheelOfFortune.Show();
        }
        
        #endregion
    }
}