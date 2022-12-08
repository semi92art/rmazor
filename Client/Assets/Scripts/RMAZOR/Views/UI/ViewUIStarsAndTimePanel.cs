using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
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
        
        private float       m_BottomOffset;
        private bool        m_DoUpdateTimeText;

        private float 
            m_TimeThreshold3Stars, 
            m_TimeThreshold2Stars,
            m_TimeThreshold1Star,
            m_CurrentTimeThreshold;

        private bool 
            m_Missed3Stars, 
            m_Missed2Stars,
            m_Missed1Star;
        
        #endregion

        #region inject

        private IModelGame           Model               { get; }
        private IContainersGetter    ContainersGetter    { get; }
        private ICameraProvider      CameraProvider      { get; }
        private IPrefabSetManager    PrefabSetManager    { get; }
        private IViewGameTicker      ViewGameTicker      { get; }
        private IColorProvider       ColorProvider       { get; }
        private IFontProvider        FontProvider        { get; }
        private ILocalizationManager LocalizationManager { get; }

        public ViewUIStarsAndTimePanel(
            IModelGame           _Model,
            IContainersGetter    _ContainersGetter,
            ICameraProvider      _CameraProvider,
            IPrefabSetManager    _PrefabSetManager,
            IViewGameTicker      _ViewGameTicker,
            IColorProvider       _ColorProvider,
            IFontProvider        _FontProvider,
            ILocalizationManager _LocalizationManager)
        {
            Model               = _Model;
            ContainersGetter    = _ContainersGetter;
            CameraProvider      = _CameraProvider;
            PrefabSetManager    = _PrefabSetManager;
            ViewGameTicker      = _ViewGameTicker;
            ColorProvider       = _ColorProvider;
            FontProvider        = _FontProvider;
            LocalizationManager = _LocalizationManager;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_BottomOffset = _Offsets.y;
            InitStarRenderers();
            InitTimeTexts();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            LocalizationManager.LanguageChanged += OnLanguageChanged;
            ViewGameTicker.Register(this);
            base.Init();
        }

        public IEnumerable<Component> GetRenderers()
        {
            return new Component[] {m_TimeText, m_TimeThresholdText}.Concat(m_StarRenderers);
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
            m_TimeText.enabled = _Show;
            m_TimeThresholdText.enabled = _Show;
            foreach (var starRenderer in m_StarRenderers)
                starRenderer.enabled = _Show;
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
            float yPos = screenBounds.max.y - 11f; 
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
                .SetLocalPosY(yPos - 2f)
                .SetLocalPosZ(10f);
            m_TimeText.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(screenBounds.min.x + 4f)
                .SetLocalPosY(yPos - 4f)
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
            OnStarsMissed(4);
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
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            float yPos = screenBounds.max.y - 11f;
            m_StarRenderers[0].enabled = _StarsCount > 1;
            m_StarRenderers[1].enabled = _StarsCount > 2;
            m_StarRenderers[2].enabled = _StarsCount > 3;
            m_TimeText.transform.SetLocalPosY(yPos - 4f);

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
                    m_TimeText.transform.SetLocalPosY(yPos);
                    break;
            }
            if (_StarsCount > 1)
            {
                m_TimeThresholdText.text = LocalizationManager.GetTranslation("goal") + ": " +
                                           m_CurrentTimeThreshold.ToString("F1", m_FloatValueCultureInfo);
            }
        }

        #endregion
    }
}