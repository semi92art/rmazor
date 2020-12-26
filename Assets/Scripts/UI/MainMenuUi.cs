using System.Collections.Generic;
using Constants;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using Lean.Localization;
using Managers;
using Network;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class MainMenuUi : GameObservable
    {
        #region notify messages

        public const string NotifyMessageMainMenuLoaded = nameof(NotifyMessageMainMenuLoaded);
        public const string NotifyMessageSelectGamePanelButtonClick = nameof(NotifyMessageSelectGamePanelButtonClick);
        public const string NotifyMessageProfileButtonClick = nameof(NotifyMessageProfileButtonClick);
        public const string NotifyMessageSettingsButtonClick = nameof(NotifyMessageSettingsButtonClick);
        public const string NotifyMessageLoginButtonClick = nameof(NotifyMessageLoginButtonClick);
        public const string NotifyMessageShopButtonClick = nameof(NotifyMessageShopButtonClick);
        public const string NotifyMessagePlayButtonClick = nameof(NotifyMessagePlayButtonClick);
        public const string NotifyMessageRatingsButtonClick  = nameof(NotifyMessageRatingsButtonClick);
        public const string NotifyMessageDailyBonusButtonClick = nameof(NotifyMessageDailyBonusButtonClick);
        public const string NotifyMessageWheelOfFortuneButtonClick = nameof(NotifyMessageWheelOfFortuneButtonClick);
        
        #endregion
        
        #region nonpublic members
        
        private static bool _isDailyBonusClicked;
        private readonly IMenuDialogViewer m_MenuDialogViewer;
        private readonly INotificationViewer m_NotificationViewer;
        private readonly RectTransform m_Parent;
        
        private IMiniPanel m_BankMiniPanel;
        private RectTransform m_MainMenu;
        private RectTransform m_GameLogoContainer;
        private Animator m_DailyBonusAnimator;
        private MenuUiCategory m_CurrentCategory;
        private Button m_SelectGameButton;
        private Image m_GameLogo;

        #endregion

        #region constructor
        
        public MainMenuUi(
            RectTransform _Parent,
            IMenuDialogViewer _MenuDialogViewer,
            INotificationViewer _NotificationViewer)
        {
            m_Parent = _Parent;
            m_MenuDialogViewer = _MenuDialogViewer;
            m_NotificationViewer = _NotificationViewer;
            UiManager.Instance.CurrentMenuCategory = MenuUiCategory.MainMenu;
        }

        #endregion
        
        #region api

        public void Init()
        {
            InitContainers(m_Parent);
            InitSelectGameButton();
            SetGameLogo(GameClient.Instance.GameId);
            InitCenterButtonsGroup();
            InitBottomButtonsGroups();
            InitSmallButtons();
            InitBankMiniPanel();
            CheckIfDailyBonusNotChosenToday();
            m_MenuDialogViewer.AddNotDialogItem(m_MainMenu, MenuUiCategory.MainMenu);
            m_MainMenu.SetGoActive(false);
        }
        
        public void Show()
        {
            Notify(this, NotifyMessageMainMenuLoaded);
            m_MainMenu.SetGoActive(true);
        }
        
        #endregion
        
        #region nonpublic methods

        private void InitBankMiniPanel()
        {
            var bmp = new BankMiniPanel(m_Parent, m_MenuDialogViewer, m_NotificationViewer);
            bmp.AddObservers(GetObservers());
            bmp.Init();
            bmp.Show();
            m_BankMiniPanel = bmp;
        }

        private void InitContainers(RectTransform _Parent)
        {
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
            m_SelectGameButton.SetOnClick(OnSelectGamePanelButtonClick);
        }

        private void SetGameLogo(int _GameId)
        {
            m_GameLogo.sprite = PrefabInitializer.GetObject<Sprite>(
                    "game_logos", $"game_logo_{GameClient.Instance.GameId}");
        }

        private void InitCenterButtonsGroup()
        {
            GameObject centerButtonsScrollView = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(0.5f, 0.5f, 0.5f, 0.5f),
                    Vector2.up * -65f,
                    Vector2.one * 0.5f,
                    new Vector2(280, 360)),
                CommonStyleNames.MainMenu, "center_buttons_group");
            
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
            var bestScoreText = playButton.GetCompItem<TextMeshProUGUI>("best_score_text");
            var scores = ScoreManager.Instance.GetScores();
            Coroutines.Run(Coroutines.WaitWhile(
                () =>
                {
                    long maxScore = 0;
                    if (scores.Scores.ContainsKey(ScoreTypes.MaxScore))
                        maxScore = scores.Scores[ScoreTypes.MaxScore];
                    bestScoreText.text = $" {maxScore.ToNumeric()}";
                },
                () => !scores.Loaded));
            
            
            var ratingsButton = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    rtrLite),
                "main_menu_buttons",
                "ratings_button");
            ratingsButton.GetComponent<Button>().SetOnClick(OnRatingsButtonClick);
            
        }

        private void InitBottomButtonsGroups()
        {
            string paletteName = "Main Menu Bottom Group Buttons";
            var buttonRtrLite = new RectTransformLite
            {
                Anchor = UiAnchor.Create(1, 0, 1, 0),
                AnchoredPosition = Vector2.zero,
                Pivot = Vector2.one * 0.5f,
                SizeDelta = Vector2.one * 100f
            };
            
            var firstGroupObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(0, 0, 1, 0),
                    Vector2.up * 123f,
                    Vector2.one * 0.5f,
                    Vector2.up * 100f),
                CommonStyleNames.MainMenu,
                "bottom_buttons_group");
            var firstGroupContent = firstGroupObj.GetCompItem<RectTransform>("content");
            
            var secondGroupObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MainMenu,
                    UiAnchor.Create(0, 0, 1, 0),
                    Vector2.up * 247,
                    Vector2.one * 0.5f,
                    Vector2.up * 100f),
                CommonStyleNames.MainMenu,
                "bottom_buttons_group");
            var secondGroupContent = secondGroupObj.GetCompItem<RectTransform>("content");
            
            var temp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    firstGroupContent,
                    buttonRtrLite),
                "main_menu_buttons",
                "bottom_group_button_template");

            // Wheel of fortune button
            InitBottomGroupButton(
                "Wheel Of Fortune Button",
                firstGroupContent,
                temp,
                PrefabInitializer.GetObject<Sprite>(
                    "button_sprites", "wof_button_background"),
                PrefabInitializer.GetObject<Sprite>(
                    "icons", "icon_wof"),
                ColorUtils.GetColorFromPalette(paletteName, "Wheel Of Fortune"),
                "Wheel",
                OnWheelOfFortuneButtonClick);

            // Daily bonus button
            var buttonGo = InitBottomGroupButton(
                "Daily Bonus Button",
                firstGroupContent,
                temp,
                PrefabInitializer.GetObject<Sprite>(
                    "button_sprites", "daily_bonus_button_background"),
                PrefabInitializer.GetObject<Sprite>(
                    "icons", "icon_daily_bonus"),
                ColorUtils.GetColorFromPalette(paletteName, "Daily Bonus"),
                "Bonus",
                OnDailyBonusButtonClick);
            m_DailyBonusAnimator = buttonGo.GetCompItem<Animator>("animator");

            // Shop button
            InitBottomGroupButton(
                "Shop Button",
                firstGroupContent,
                temp,
                PrefabInitializer.GetObject<Sprite>(
                    "button_sprites", "shop_button_background"),
                PrefabInitializer.GetObject<Sprite>(
                    "icons", "icon_shop"),
                ColorUtils.GetColorFromPalette(paletteName, "Shop"),
                "Shop",
                OnShopButtonClick);

            // Login button
            InitBottomGroupButton(
                "Login Button",
                secondGroupContent,
                temp,
                PrefabInitializer.GetObject<Sprite>(
                    "button_sprites", "login_button_background"),
                PrefabInitializer.GetObject<Sprite>(
                    "icons", "icon_login"),
                ColorUtils.GetColorFromPalette(paletteName, "Login"),
                "Login",
                OnLoginButtonClick);

            Object.Destroy(temp);
        }

        private GameObject InitBottomGroupButton(
            string _Name,
            RectTransform _Parent,
            GameObject _ButtonObjectTemplate,
            Sprite _Background,
            Sprite _Icon,
            Color _BorderColor,
            string _TitleLocalizationKey,
            UnityAction _Action)
        {
            var go = Object.Instantiate(_ButtonObjectTemplate);
            go.name = _Name;
            go.SetParent(_Parent);
            go.RTransform().localScale = Vector3.one;
            go.GetCompItem<Image>("background").sprite = _Background;
            go.GetCompItem<Image>("icon").sprite = _Icon;
            go.GetCompItem<Image>("border").color = _BorderColor;
            var localized = go.GetCompItem<TextMeshProUGUI>("title").gameObject
                .AddComponent<LeanLocalizedTextMeshProUGUI>();
            localized.TranslationName = _TitleLocalizationKey;
            go.GetCompItem<Button>("button").SetOnClick(_Action);
            return go;
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
            if (!m_DailyBonusAnimator.IsNull())
                m_DailyBonusAnimator.SetTrigger(
                    lastDate.Date == System.DateTime.Now.Date ?
                    AnimKeys.Stop : AnimKeys.Anim);
        }
        
        #endregion

        #region event methods

        private void OnSelectGamePanelButtonClick()
        {
            Notify(this, NotifyMessageSelectGamePanelButtonClick);
            var selectGamePanel = new SelectGamePanel(m_MenuDialogViewer, SetGameLogo);
            selectGamePanel.AddObservers(GetObservers());
            selectGamePanel.Init();
            m_MenuDialogViewer.Show(selectGamePanel);
        }
        
        private void OnProfileButtonClick()
        {
            Notify(this, NotifyMessageProfileButtonClick);
            var profilePanel = new ProfilePanel(m_MenuDialogViewer);
            profilePanel.AddObservers(GetObservers());
            profilePanel.Init();
            m_MenuDialogViewer.Show(profilePanel);
        }

        private void OnSettingsButtonClick()
        {
            Notify(this, NotifyMessageSettingsButtonClick);
            var settingsPanel = new SettingsPanel(m_MenuDialogViewer);
            settingsPanel.AddObservers(GetObservers());
            settingsPanel.Init();
            m_MenuDialogViewer.Show(settingsPanel);
        }

        private void OnLoginButtonClick()
        {
            Notify(this, NotifyMessageLoginButtonClick);
            var loginPanel = new LoginPanel(m_MenuDialogViewer);
            loginPanel.AddObservers(GetObservers());
            loginPanel.Init();
            m_MenuDialogViewer.Show(loginPanel);
        }

        private void OnShopButtonClick()
        {
            Notify(this, NotifyMessageShopButtonClick);
            var shop = new ShopPanel(m_MenuDialogViewer.Container);
            shop.AddObservers(GetObservers());
            shop.Init();
            m_MenuDialogViewer.Show(shop);
        }

        private void OnPlayButtonClick()
        {
            Notify(this, NotifyMessagePlayButtonClick);
            (m_BankMiniPanel as BankMiniPanel)?.UnregisterFromEvents();
            LevelLoader.LoadLevel(1);
        }

        private void OnRatingsButtonClick()
        {
            Notify(this, NotifyMessageRatingsButtonClick);
            // TODO
        }

        private void OnDailyBonusButtonClick()
        {
            Notify(this, NotifyMessageDailyBonusButtonClick);
            var dailyBonusPanel = new DailyBonusPanel(
                m_MenuDialogViewer, (IActionExecutor)m_BankMiniPanel);
            dailyBonusPanel.AddObservers(GetObservers());
            dailyBonusPanel.Init();
            m_MenuDialogViewer.Show(dailyBonusPanel);
        }

        private void OnWheelOfFortuneButtonClick()
        {
            Notify(this, NotifyMessageWheelOfFortuneButtonClick);
            var wofPanel = new WheelOfFortunePanel(m_MenuDialogViewer, m_NotificationViewer);
            wofPanel.AddObservers(GetObservers());
            wofPanel.Init();
            m_MenuDialogViewer.Show(wofPanel);
        }
        
        #endregion
    }
}