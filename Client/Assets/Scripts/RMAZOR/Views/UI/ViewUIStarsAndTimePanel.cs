using System.Collections.Generic;
using System.Globalization;
using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.Utils;
using TMPro;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public interface IViewUIStarsAndTimePanel :
        IInitViewUIItem,
        IOnLevelStageChanged,
        IViewUIGetRenderers
    {
        void ShowControls(bool _Show, bool _Instantly);
    }

    public class ViewUIStarsAndTimePanel :
        InitBase,
        IViewUIStarsAndTimePanel,
        IUpdateTick
    {
        #region nonpublic members

        private readonly List<SpriteRenderer> m_StarRenderers = new List<SpriteRenderer>();
        private readonly CultureInfo m_FloatValueCultureInfo = new CultureInfo("en-US");

        private TextMeshPro m_TimeText;
        private TextMeshPro m_TimeThresholdText;
        
        private bool        m_DoUpdateTimeText;

        private bool CanShowPanel => false;
        // private bool CanShowPanel => Model.LevelStaging.LevelIndex >= m_TimerTutorialIndexCached
        //                              || (string) Model.LevelStaging.Arguments.GetSafe(
        //                                  CommonInputCommandArg.KeyNextLevelType, out _) ==
        //                              CommonInputCommandArg.ParameterLevelTypeBonus; 

        private float 
            m_TimeThreshold3Stars, 
            m_TimeThreshold2Stars,
            m_TimeThreshold1Star,
            m_CurrentTimeThreshold;

        private bool 
            m_Missed3Stars, 
            m_Missed2Stars,
            m_Missed1Star;

        private long m_TimerTutorialIndexCached;
        
        #endregion

        #region inject

        private IModelGame           Model               { get; }
        private ICameraProvider      CameraProvider      { get; }
        private IPrefabSetManager    PrefabSetManager    { get; }
        private IViewGameTicker      ViewGameTicker      { get; }
        private IColorProvider       ColorProvider       { get; }
        private IFontProvider        FontProvider        { get; }
        private ILocalizationManager LocalizationManager { get; }
        private ILevelsLoader        LevelsLoader        { get; }

        public ViewUIStarsAndTimePanel(
            IModelGame           _Model,
            ICameraProvider      _CameraProvider,
            IPrefabSetManager    _PrefabSetManager,
            IViewGameTicker      _ViewGameTicker,
            IColorProvider       _ColorProvider,
            IFontProvider        _FontProvider,
            ILocalizationManager _LocalizationManager,
            ILevelsLoader        _LevelsLoader)
        {
            Model               = _Model;
            CameraProvider      = _CameraProvider;
            PrefabSetManager    = _PrefabSetManager;
            ViewGameTicker      = _ViewGameTicker;
            ColorProvider       = _ColorProvider;
            FontProvider        = _FontProvider;
            LocalizationManager = _LocalizationManager;
            LevelsLoader        = _LevelsLoader;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            CacheTimerTutorialLevelIndex();
            InitStarRenderers();
            InitTimeTexts();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            LocalizationManager.LanguageChanged += OnLanguageChanged;
            ViewGameTicker.Register(this);
            base.Init();
        }

        public IEnumerable<Component> GetRenderers()
        {
            return new Component[0];
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    RefreshState();
                    break;
                case ELevelStage.StartedOrContinued:
                    m_DoUpdateTimeText = true;
                    break;
                case ELevelStage.Paused:
                case ELevelStage.Finished:
                case ELevelStage.CharacterKilled:
                    m_DoUpdateTimeText = false;
                    break;
            }
        }

        public void ShowControls(bool _Show, bool _Instantly)
        {
            m_TimeText.enabled = _Show && CanShowPanel;
            m_TimeThresholdText.enabled = _Show && CanShowPanel;
            foreach (var starRenderer in m_StarRenderers)
                starRenderer.enabled = _Show && CanShowPanel;
        }
        
        public void UpdateTick()
        {
            if (!m_DoUpdateTimeText)
                return;
            m_TimeText.text = Model.LevelStaging.LevelTime.ToString("F3", m_FloatValueCultureInfo);
            CheckForStarMissOnUpdateTick();
        }

        #endregion

        #region nonpublic methods
        
        private void OnLanguageChanged(ELanguage _Language)
        {
            var font = FontProvider.GetFont(ETextType.GameUI, _Language);
            m_TimeText.font = font;
            m_TimeThresholdText.font = font;
            m_TimeThresholdText.text = LocalizationManager.GetTranslation("goal") + ": " +
                                       m_CurrentTimeThreshold.ToString("F3", m_FloatValueCultureInfo);
        }
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            var parent = _Camera.transform;
            var screenBounds = GraphicUtils.GetVisibleBounds(_Camera);
            var scaleVec = Vector2.one * 1.5f;
            float yPos = screenBounds.min.y + 4f; 
            for (int i = 0; i < 3; i++)
            {
                var iStarRend = m_StarRenderers[i];
                iStarRend.transform
                    .SetParentEx(parent)
                    .SetLocalScaleXY(scaleVec)
                    .SetLocalPosX(screenBounds.min.x + 1.5f + i * 2.5f)
                    .SetLocalPosY(yPos)
                    .SetLocalPosZ(10f);
            }
            m_TimeThresholdText.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(screenBounds.min.x + 4f)
                .SetLocalPosY(yPos + 2.5f)
                .SetLocalPosZ(10f);
            m_TimeText.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(screenBounds.min.x + 4f)
                .SetLocalPosY(yPos + 4.5f)
                .SetLocalPosZ(10f);
        }
        
        private void InitStarRenderers()
        {
            var starSprite = PrefabSetManager.GetObject<Sprite>("icons", "star");
            for (int i = 1; i <= 3; i++)
            {
                var go = new GameObject($"Star {i}");
                var spriteRenderer = go.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = starSprite;
                spriteRenderer.sortingOrder = SortingOrders.GameUI;
                m_StarRenderers.Add(spriteRenderer);
            }
        }

        private void InitTimeTexts()
        {
            var colUi = ColorProvider.GetColor(ColorIds.UI);
            var go1 = new GameObject("Current Level Time Text");
            var font =  FontProvider.GetFont(ETextType.GameUI, LocalizationManager.GetCurrentLanguage());
            m_TimeText = go1.AddComponent<TextMeshPro>();
            m_TimeText.color = colUi;
            m_TimeText.sortingOrder = SortingOrders.GameUI;
            m_TimeText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            m_TimeText.verticalAlignment = VerticalAlignmentOptions.Bottom;
            m_TimeText.enableWordWrapping = false;
            m_TimeText.font = font;
            m_TimeText.fontSize = 13f;
            m_TimeText.rectTransform.sizeDelta = new Vector2(5f, 1.5f);
            var go2 = new GameObject("Time Threshold Text");
            m_TimeThresholdText = go2.AddComponent<TextMeshPro>();
            m_TimeThresholdText.color = colUi;
            m_TimeThresholdText.sortingOrder = SortingOrders.GameUI;
            m_TimeThresholdText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            m_TimeThresholdText.verticalAlignment = VerticalAlignmentOptions.Bottom;
            m_TimeThresholdText.enableWordWrapping = false;
            m_TimeThresholdText.font = font;
            m_TimeThresholdText.fontSize = 13f;
            m_TimeThresholdText.rectTransform.sizeDelta = new Vector2(5f, 1.5f);
        }
        
        private void RefreshState()
        {
            var mazeAdditionalInfo = Model.Data.Info.AdditionalInfo;
            m_TimeThreshold3Stars = mazeAdditionalInfo.Time3Stars;
            m_TimeThreshold2Stars = mazeAdditionalInfo.Time2Stars;
            m_TimeThreshold1Star = mazeAdditionalInfo.Time1Star;
            m_Missed3Stars = false;
            m_Missed2Stars = false;
            m_Missed1Star  = false;
            m_CurrentTimeThreshold = m_TimeThreshold3Stars;
            m_TimeText.text = 0.ToString("F2", m_FloatValueCultureInfo);
            OnStarsMissed(4);
            ShowControls(true, true);
        }

        private void CheckForStarMissOnUpdateTick()
        {
            float levelTime = Model.LevelStaging.LevelTime;
            if (levelTime > m_TimeThreshold3Stars && !m_Missed3Stars)
                OnStarsMissed(3);
            if (levelTime > m_TimeThreshold2Stars && !m_Missed2Stars)
                OnStarsMissed(2);
            if (levelTime > m_TimeThreshold1Star && !m_Missed1Star)
                OnStarsMissed(1);
        }
        
        private void OnStarsMissed(int _StarsCount)
        {
            m_StarRenderers[0].enabled = _StarsCount > 1 && CanShowPanel;
            m_StarRenderers[1].enabled = _StarsCount > 2 && CanShowPanel;
            m_StarRenderers[2].enabled = _StarsCount > 3 && CanShowPanel;
            switch (_StarsCount)
            {
                case 3:
                    m_Missed3Stars = true;
                    m_CurrentTimeThreshold = m_TimeThreshold2Stars;
                    break;
                case 2:
                    m_Missed2Stars = true;
                    m_CurrentTimeThreshold = m_TimeThreshold1Star;
                    break;
                case 1:
                    m_TimeThresholdText.text = string.Empty;
                    m_Missed1Star  = true;
                    break;
            }
            if (_StarsCount > 1)
            {
                m_TimeThresholdText.text = LocalizationManager.GetTranslation("goal") + ": " +
                                           m_CurrentTimeThreshold.ToString("F1", m_FloatValueCultureInfo);
            }
        }

        private void CacheTimerTutorialLevelIndex()
        {
            for (int i = 0; i < 50; i++)
            {
                var levelInfo = LevelsLoader.GetLevelInfo(CommonData.GameId, i, false);
                string args = levelInfo.AdditionalInfo.Arguments;
                if (string.IsNullOrEmpty(args))
                    continue;
                if (!args.Contains("tutorial") || !args.Contains(":"))
                    continue;
                string tutorialType = args.Split(':')[1];
                if (tutorialType != "timer")
                    continue;
                m_TimerTutorialIndexCached = i;
                return;
            }
            m_TimerTutorialIndexCached = long.MaxValue;
        }

        #endregion
    }
}