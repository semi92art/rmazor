using System;
using System.Globalization;
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
using mazing.common.Runtime.Utils;
using RMAZOR.UI.Panels;
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

        [SerializeField] private RawImage        characterIcon;
        [SerializeField] private Button          customizeCharacterButton;
        [SerializeField] private TextMeshProUGUI customizeCharacterButtonText;
        
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private Button          buyMoneyButton;
        
        [SerializeField] private Slider          xpSlider;
        [SerializeField] private TextMeshProUGUI xpSliderTitleText, xpSliderBodyText;

        [SerializeField] private Button         dailyGiftButton;
        [SerializeField] private Image          dailyGiftIcon;
        [SerializeField] private ParticleSystem dailyGiftParticleSystem;
        [SerializeField] private Image          dailyGiftStamp;

        #endregion

        #region nonpublic members
        
        private UnityAction              m_OnDailyGiftButtonClick;
        private UnityAction              m_OnAddMoneyButtonClick;
        private UnityAction              m_OnCustomizeCharacterButtonClick;
        private Sprite                   m_DailyGiftDisabledSprite;
        
        private LocTextInfo MoneyTextLocTextInfo => new LocTextInfo(
            moneyText, ETextType.MenuUI, "empty_key",
            _T => GetBankMoneyCount().ToString("N0"));

        private IContainersGetter       ContainersGetter    { get; set; }
        private IPrefabSetManager       PrefabSetManager    { get; set; }
        private IScoreManager           ScoreManager        { get; set; }
        private IDailyGiftPanel         DailyGiftPanel      { get; set; }
        private ICoordinateConverter    CoordinateConverter { get; set; }

        #endregion

        #region api

        public void Init(
            IUITicker                   _UITicker,
            IAudioManager               _AudioManager,
            ILocalizationManager        _LocalizationManager,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            IScoreManager               _ScoreManager,
            IDailyGiftPanel             _DailyGiftPanel,
            ICoordinateConverter        _CoordinateConverter,
            UnityAction                 _OnDailyGiftButtonClick,
            UnityAction                 _OnAddMoneyButtonClick,
            UnityAction                 _OnCustomizeCharacterButtonClick)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            ContainersGetter    = _ContainersGetter;
            PrefabSetManager    = _PrefabSetManager;
            ScoreManager        = _ScoreManager;
            DailyGiftPanel      = _DailyGiftPanel;
            CoordinateConverter = _CoordinateConverter;
            
            m_OnDailyGiftButtonClick          = _OnDailyGiftButtonClick;
            m_OnAddMoneyButtonClick           = _OnAddMoneyButtonClick;
            m_OnCustomizeCharacterButtonClick = _OnCustomizeCharacterButtonClick;
            InitRenderCameraAndRawTexture();
            LocalizeTextObjects();
            SubscribeButtonEvents();
            m_DailyGiftDisabledSprite = PrefabSetManager.GetObject<Sprite>(
                CommonPrefabSetNames.Views, "daily_gift_icon_disabled");
            if (!DailyGiftPanel.IsDailyGiftAvailableToday)
                Cor.Run(Cor.WaitNextFrame(DisableDailyGiftButton));
            DailyGiftPanel.OnClose += DisableDailyGiftButton;
            DailyGiftPanel.OnClose += RecalculateBankMoneyCount;
            ScoreManager.GameSaved += OnGameSaved;
        }

        public void UpdateState()
        {
            LocalizeTextObjects();
            xpSlider.value = (float) GetXpGotOnThisLevel() / GetXpTotalToNextLevel();
            dailyGiftStamp.enabled = false;
            if (!DailyGiftPanel.IsDailyGiftAvailableToday)
                Cor.Run(Cor.WaitNextFrame(DisableDailyGiftButton));
        }

        #endregion

        #region nonpublic methods
        
        private void OnGameSaved(SavedGameEventArgs _Args)
        {
            var savedGame = (SavedGameV2) _Args.SavedGame;
            int moneyCount = Convert.ToInt32(savedGame.Arguments[KeyMoneyCount]);
            moneyText.text = moneyCount.ToString("N0");
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
            var colorGrading = cam.gameObject.AddComponent<ColorGrading>();
            colorGrading.material = PrefabSetManager.GetObject<Material>(
                CommonPrefabSetNames.Views, "color_grading_material");
            colorGrading.Contrast       = 0.35f;
            colorGrading.Blur           = 0.2f;
            colorGrading.Saturation     = 0f;
            colorGrading.VignetteAmount = 0f;
        }
        
        private void LocalizeTextObjects()
        {
            const string emptyLocKey = "empty_key";
            var locTextInfos = new[]
            {
                MoneyTextLocTextInfo,
                new LocTextInfo(xpSliderTitleText, ETextType.MenuUI, emptyLocKey,
                    _T => GetCharacterLevel().ToString(), ETextLocalizationType.OnlyText),
                new LocTextInfo(xpSliderBodyText, ETextType.MenuUI, emptyLocKey, 
                    _T => GetXpGotOnThisLevel() + "/" + GetXpTotalToNextLevel() + "XP", ETextLocalizationType.OnlyText),
                new LocTextInfo(customizeCharacterButtonText, ETextType.MenuUI, "choose",
                    _T => _T.ToUpper(CultureInfo.CurrentUICulture)), 
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
        }

        private void SubscribeButtonEvents()
        {
            customizeCharacterButton.onClick.AddListener(m_OnCustomizeCharacterButtonClick);
            buyMoneyButton          .onClick.AddListener(m_OnAddMoneyButtonClick);
            dailyGiftButton         .onClick.AddListener(m_OnDailyGiftButtonClick);
        }

        private int GetBankMoneyCount()
        {
            var savedGame = ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
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

        private void DisableDailyGiftButton()
        {
            dailyGiftIcon.sprite = m_DailyGiftDisabledSprite;
            dailyGiftButton.interactable = false;
            dailyGiftParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dailyGiftStamp.enabled = true;
        }

        #endregion
    }
}