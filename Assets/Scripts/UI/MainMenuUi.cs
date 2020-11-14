using Constants;
using Entities;
using Extensions;
using Helpers;
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
    public class MainMenuUi
    {
        private static bool _isDailyBonusClicked;
        
        private IDialogViewer m_DialogViewer;
        private IMiniPanel m_BankMiniPanel;
        
        private RectTransform m_Parent;
        private RectTransform m_MainMenu;
        private RectTransform m_GameLogoContainer;
        private Animator m_DailyBonusAnimator;
        private UiCategory m_CurrentCategory;
        private Image m_WofBackground;
        private Image m_WofBorder;
        private Button m_WofButton;
        private Button m_WofAdsButton;

        private Button m_SelectGameButton;
        private Image m_GameLogo;

        public static MainMenuUi Create(
            RectTransform _Parent,
            IDialogViewer _DialogViewer)
        {
            return new MainMenuUi(_Parent, _DialogViewer);
        }

        private MainMenuUi(
            RectTransform _Parent,
            IDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
            UiManager.Instance.CurrentCategory = UiCategory.MainMenu;
            InitContainers(_Parent);
            InitSelectGameButton();
            SetGameLogo(GameClient.Instance.GameId);
            InitCenterButtonsScrollView();
            InitBottomButtonsScrollView();
            InitSmallButtons();
            InitBankMiniPanel();
            CheckIfDailyBonusNotChosenToday();
            m_DialogViewer.AddNotDialogItem(m_MainMenu, UiCategory.MainMenu);
            UiManager.Instance.OnCurrentCategoryChanged += (_Prev, _New) =>
            {
                if (_Prev == UiCategory.WheelOfFortune && m_WofButton != null)
                    CheckIfWofSpinedToday();
            };
            SoundManager.Instance.PlayClip("main_menu_theme", true, 0f);
            LocalizationManager a = LocalizationManager.Instance;
        }

        private void InitBankMiniPanel()
        {
            m_BankMiniPanel = new BankMiniPanel(m_Parent, m_DialogViewer);
            m_BankMiniPanel.Show();
        }

        private void InitContainers(RectTransform _Parent)
        {
            m_Parent = _Parent;
            m_MainMenu = UiFactory.UiRectTransform(
                _Parent,
                "Main Menu",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero);

            var activeStateWatcher = m_MainMenu.gameObject.AddComponent<ActiveStateWatcher>();
            activeStateWatcher.ActiveStateChanged += _Args =>
            {
                if (_Args.Value)
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
            ratingsButton.GetComponent<Button>().SetOnClick(OnRationgsClick);
            
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
            m_WofButton = wofButton.GetComponent<Button>();
            m_WofButton.SetOnClick(OnWheelOfFortuneButtonClick);
            m_WofBackground = wofButton.GetCompItem<Image>("background");
            m_WofBorder = wofButton.GetCompItem<Image>("border");
            m_WofAdsButton = wofButton.GetCompItem<Button>("watch_ad_button");
            m_WofAdsButton.SetOnClick(() =>
            {
                SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, System.DateTime.Now.Date.AddDays(-1));
                CheckIfWofSpinedToday();
            });
            CheckIfWofSpinedToday();
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
            m_DailyBonusAnimator.SetTrigger(lastDate.Date == System.DateTime.Now.Date ?
                AnimKeys.Stop : AnimKeys.Anim);
        }

        private void CheckIfWofSpinedToday()
        {
            System.DateTime lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.WheelOfFortuneLastDate);
            bool done = lastDate == System.DateTime.Now.Date;
            m_WofButton.interactable = !done;
            string color = done ? "gray" : "purple";
            m_WofBackground.sprite = PrefabInitializer.GetObject<Sprite>(
                "button_sprites", $"rect_background_{color}");
            if (ColorUtility.TryParseHtmlString($"#{(!done ? "894089" : "8A8A8A")}", out Color borderColor))
                m_WofBorder.color = borderColor;
            
            m_WofAdsButton.gameObject.SetActive(done);
        }
        
        #region event methods

        private void OnOpenSelectGamePanel()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IDialogPanel selectGame = new SelectGamePanel(m_DialogViewer, SetGameLogo);
            selectGame.Show();
        }
        
        private void OnProfileButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IDialogPanel profile = new ProfilePanel(m_DialogViewer);
            profile.Show();
        }

        private void OnSettingsButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IDialogPanel settings = new SettingsPanel(m_DialogViewer);
            settings.Show();
        }

        private void OnLoginButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IDialogPanel login = new LoginPanel(m_DialogViewer);
            login.Show();
        }

        private void OnShopButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IDialogPanel shop = new ShopPanel(m_DialogViewer);
            shop.Show();
        }

        private void OnPlayButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            new LevelLoader().LoadLevel();
        }

        private void OnRationgsClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            // TODO
        }

        private void OnDailyBonusButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IDialogPanel dailyBonus = new DailyBonusPanel(
                m_DialogViewer, (IActionExecuter)m_BankMiniPanel);
            dailyBonus.Show();
        }

        private void OnWheelOfFortuneButtonClick()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IDialogPanel wheelOfFortune = new WheelOfFortunePanel(m_DialogViewer);
            wheelOfFortune.Show();
        }
        
        #endregion
    }
}