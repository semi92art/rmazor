using Extentions;
using UICreationSystem.Factories;
using UICreationSystem.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UICreationSystem
{
    public class MainMenuUi
    {
        private static bool _isDailyBonusClicked;
        
        private IDialogViewer m_DialogViewer;
        private IMiniPanel m_BankMiniPanel;

        private GameObject m_GameTitle;
        private RectTransform m_Parent;
        private RectTransform m_MainMenu;
        private RectTransform m_GameTitleContainer;
        private Animator m_DailyBonusAnimator;

        public static MainMenuUi Create(RectTransform _Parent,
            IDialogViewer _DialogViewer)
        {
            return new MainMenuUi(_Parent, _DialogViewer);
        }

        private MainMenuUi(RectTransform _Parent,
            IDialogViewer _DialogViewer)
        {
            UiManager.Instance.CurrentCategory = UiCategory.MainMenu;
            InitContainers(_Parent);
            m_DialogViewer = _DialogViewer;
            
            SetGameTitle(SaveUtils.GetValue<int>(SaveKey.GameId));
            InitCenterButtonsScrollView();
            InitBottomButtonsScrollView();
            InitSmallButtons();
            InitBankMiniPanel();
            
            m_DialogViewer.SetNotDialogItems(new [] {m_MainMenu});

            CheckIfDailyBonusNotChosenToday();
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
                if (_Args.IsActive)
                    CheckIfDailyBonusNotChosenToday();
            };

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
            m_DailyBonusAnimator.SetTrigger(lastDate.Date == System.DateTime.Now.Date ?
                AnimKeys.Stop : AnimKeys.Anim);
        }
        
        #region event methods

        private void OnOpenSelectGamePanel()
        {
            IDialogPanel selectGamePanel = new SelectGamePanel(m_DialogViewer);
            selectGamePanel.Show();
        }
        
        private void OnProfileButtonClick()
        {
            //TODO profile button logic
        }

        private void OnSettingsButtonClick()
        {
            IDialogPanel settingsPanel = new SettingsPanel(m_DialogViewer);
            settingsPanel.Show();
        }

        private void OnLoginButtonClick()
        {
            IDialogPanel loginPanel = new LoginPanel(m_DialogViewer);
            loginPanel.Show();
        }

        private void OnShopButtonClick()
        {
            IDialogPanel shopPanel = new ShopPanel(m_DialogViewer);
            shopPanel.Show();
        }

        private void OnPlayButtonClick()
        {
            //TODO play button click
        }

        private void OnDailyBonusButtonClick()
        {
            IDialogPanel dailyBonusPanel = new DailyBonusPanel(m_DialogViewer, (IActionExecuter)m_BankMiniPanel);
            dailyBonusPanel.Show();
        }

        private void OnWheelOfFortuneButtonClick()
        {
            //TODO wheel of fortune button click
        }
        
        #endregion
    }
}