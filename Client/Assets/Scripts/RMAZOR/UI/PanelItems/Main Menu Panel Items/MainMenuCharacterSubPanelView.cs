using System;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using RMAZOR.UI.Panels;
using RMAZOR.UI.Panels.ShopPanels;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Coordinate_Converters;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public class MainMenuCharacterSubPanelView : SimpleUiItem
    {
        #region serialized fields

        [SerializeField] private RawImage                         characterIcon;
        [SerializeField] private Button                           buttonCharacterIcon;
        [SerializeField] private MainMenuCustomizeCharacterButton buttonCustomizeCharacter;
        [SerializeField] private MainMenuShopButton               buttonShop;
        [SerializeField] private TextMeshProUGUI                  moneyText;
        [SerializeField] private Button                           buyMoneyButton;
        [SerializeField] private Slider                           xpSlider;
        [SerializeField] private TextMeshProUGUI                  xpSliderTitleText, xpSliderBodyText;
        
        #endregion

        #region nonpublic members
        
        private UnityAction 
            m_OnAddMoneyButtonClick,
            m_OnCustomizeCharacterButtonClick,
            m_OnShopButtonClick;

        private LocTextInfo MoneyTextLocTextInfo => new LocTextInfo(
            moneyText, ETextType.MenuUI_H1, "empty_key",
            _T => GetBankMoneyCount().ToString("N0"));

        private IContainersGetter        ContainersGetter        { get; set; }
        private IPrefabSetManager        PrefabSetManager        { get; set; }
        private IScoreManager            ScoreManager            { get; set; }
        private IDailyGiftPanel          DailyGiftPanel          { get; set; }
        private ICoordinateConverter     CoordinateConverter     { get; set; }
        private ICustomizeCharacterPanel CustomizeCharacterPanel { get; set; }
        private IShopDialogPanel         ShopDialogPanel         { get; set; }

        #endregion

        #region api

        public void Init(
            IUITicker                _UITicker,
            IAudioManager            _AudioManager,
            ILocalizationManager     _LocalizationManager,
            IContainersGetter        _ContainersGetter,
            IPrefabSetManager        _PrefabSetManager,
            IScoreManager            _ScoreManager,
            IDailyGiftPanel          _DailyGiftPanel,
            ICoordinateConverter     _CoordinateConverter,
            ICustomizeCharacterPanel _CustomizeCharacterPanel,
            IShopDialogPanel         _ShopDialogPanel,
            UnityAction              _OnAddMoneyButtonClick,
            UnityAction              _OnCustomizeCharacterButtonClick,
            UnityAction              _OnShopButtonClick)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            ContainersGetter                  = _ContainersGetter;
            PrefabSetManager                  = _PrefabSetManager;
            ScoreManager                      = _ScoreManager;
            DailyGiftPanel                    = _DailyGiftPanel;
            CoordinateConverter               = _CoordinateConverter;
            CustomizeCharacterPanel           = _CustomizeCharacterPanel;
            ShopDialogPanel                   = _ShopDialogPanel;
            m_OnAddMoneyButtonClick           = _OnAddMoneyButtonClick;
            m_OnCustomizeCharacterButtonClick = _OnCustomizeCharacterButtonClick;
            m_OnShopButtonClick               = _OnShopButtonClick;
            buttonCharacterIcon.SetOnClick(m_OnCustomizeCharacterButtonClick);
            InitRenderCameraAndRawTexture();
            LocalizeTextObjects();
            SubscribeButtonEvents();
            DailyGiftPanel.OnPanelCloseAction += RecalculateBankMoneyCount;
            ScoreManager.GameSaved += OnGameSaved;
            InitButtonCustomizeCharacter();
            InitButtonShop();
            CustomizeCharacterPanel.BadgesNumberChanged += OnCustomizeCharacterPanelBadgesNumberChanged;
            ShopDialogPanel        .BadgesNumberChanged += OnShopPanelBadgesNumberChanged;
        }
        
        public void UpdateState()
        {
            LocalizeTextObjects();
            xpSlider.value = (float) GetXpGotOnThisLevel() / GetXpTotalToNextLevel();
            buttonCustomizeCharacter.UpdateState();
            buttonShop.UpdateState();
        }

        #endregion

        #region nonpublic methods
        
        private void OnGameSaved(SavedGameEventArgs _Args)
        {
            var savedGame = (SavedGameV2) _Args.SavedGame;
            int moneyCount = Convert.ToInt32(savedGame.Arguments[KeyMoneyCount]);
            moneyText.text = moneyCount.ToString("N0");
        }
        
        private void OnCustomizeCharacterPanelBadgesNumberChanged(int _BadgesNum)
        {
            buttonCustomizeCharacter.UpdateState();
        }
        
        private void OnShopPanelBadgesNumberChanged(int _BadgesNum)
        {
            buttonShop.UpdateState();
        }

        private void InitButtonCustomizeCharacter()
        {
            buttonCustomizeCharacter.Init(
                Ticker, 
                AudioManager,
                LocalizationManager,
                m_OnCustomizeCharacterButtonClick,
                GetCustomizeCharacterBadgeNum, 
                "choose");
        }

        private void InitButtonShop()
        {
            buttonShop.Init(
                Ticker, 
                AudioManager,
                LocalizationManager,
                m_OnShopButtonClick,
                GetShopBadgeNum, 
                "shop");
        }
        
        private int GetCustomizeCharacterBadgeNum()
        {
            return CustomizeCharacterPanel.GetBadgesCount();
        }

        private int GetShopBadgeNum()
        {
            return ShopDialogPanel.GetBadgesNumber();
        }

        private void RecalculateBankMoneyCount()
        {
            LocalizationManager.AddLocalization(MoneyTextLocTextInfo);
        }

        private void InitRenderCameraAndRawTexture()
        {
            var camGo = new GameObject("Main Menu Character Camera");
            var container = ContainersGetter.GetContainer(ContainerNamesMazor.Character);
            camGo.SetParent(container);
            var cam                = camGo.AddComponent<Camera>();
            cam.orthographic       = true;
            cam.depth              = 2f;
            cam.orthographicSize   = CoordinateConverter.Scale * 0.6f;
            cam.cullingMask = 1 << LayerMask.NameToLayer("ο Omikron");
            cam.transform.SetLocalPosXY(Vector2.zero).SetLocalPosZ(-1f);
            var previewCharacterTexture = PrefabSetManager.GetObject<RenderTexture>(
                CommonPrefabSetNames.Views, "preview_character_camera_texture");
            cam.targetTexture      = previewCharacterTexture;
            characterIcon.texture  = previewCharacterTexture;
        }
        
        private void LocalizeTextObjects()
        {
            const string emptyLocKey = "empty_key";
            var locTextInfos = new[]
            {
                MoneyTextLocTextInfo,
                new LocTextInfo(xpSliderTitleText, ETextType.MenuUI_H1, emptyLocKey,
                    _T => GetCharacterLevel().ToString(), ETextLocalizationType.OnlyText),
                new LocTextInfo(xpSliderBodyText, ETextType.MenuUI_H1, emptyLocKey, 
                    _T => GetXpGotOnThisLevel() + "/" + GetXpTotalToNextLevel() + "XP", 
                    ETextLocalizationType.OnlyText),
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
        }

        private void SubscribeButtonEvents()
        {
            buyMoneyButton.SetOnClick(m_OnAddMoneyButtonClick);
        }

        private int GetBankMoneyCount()
        {
            var savedGame = ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
            object bankMoneyArg = savedGame.Arguments.GetSafe(KeyMoneyCount, out bool keyExist);
            if (keyExist)
                return Convert.ToInt32(bankMoneyArg);
            bankMoneyArg = 0L;
            savedGame.Arguments.SetSafe(KeyMoneyCount, bankMoneyArg);
            ScoreManager.SaveGame(savedGame);
            return Convert.ToInt32(bankMoneyArg);
        }

        private int GetCharacterLevel()
        {
            int totalXpGot = MainMenuUtils.GetTotalXpGot(ScoreManager);
            return RmazorUtils.GetCharacterLevel(totalXpGot, out _, out _);
        }

        private int GetXpGotOnThisLevel()
        {
            int totalXpGot = MainMenuUtils.GetTotalXpGot(ScoreManager);
            RmazorUtils.GetCharacterLevel(totalXpGot, out _, out int xpGotOnThisLevel);
            return xpGotOnThisLevel;
        }

        private int GetXpTotalToNextLevel()
        {
            int totalXpGot = MainMenuUtils.GetTotalXpGot(ScoreManager);
            RmazorUtils.GetCharacterLevel(totalXpGot, out int xpToNextLevelTotal, out _);
            return xpToNextLevelTotal;
        }

        #endregion
    }
}