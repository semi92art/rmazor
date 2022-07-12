using System.Collections;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI;
using TMPro;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewUILevelSkipper : IInitViewUIItem, IOnLevelStageChanged
    {
        bool LevelSkipped { get; }
        void SkipLevel();
    }

    public class ViewUILevelSkipperFake : IViewUILevelSkipper
    {
        public bool LevelSkipped                                 => false;
        public void Init(Vector4                       _Offsets) { }
        public void OnLevelStageChanged(LevelStageArgs _Args)    { }
        public void SkipLevel()                                  { }
    }
    
    public class ViewUILevelSkipperButton : IViewUILevelSkipper
    {
        #region constants

        private const int MinimumLevelToSkip = 7;

        #endregion
        
        #region nonpublic members

        private bool           m_Initialized;
        private float          m_TopOffset;
        private GameObject     m_ButtonObj;
        private TextMeshPro    m_Text;
        private SpriteRenderer m_Border, m_Background;
        private IEnumerator    m_ShowButtonCoroutine;

        #endregion
        
        #region inject

        private ViewSettings                ViewSettings        { get; }
        private IModelGame                  Model               { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private IPrefabSetManager           PrefabSetManager    { get; }
        private IHapticsManager             HapticsManager      { get; }
        private IViewInputTouchProceeder    TouchProceeder      { get; }
        private ICameraProvider             CameraProvider      { get; }
        private ILocalizationManager        LocalizationManager { get; }
        private IAdsManager                 AdsManager          { get; }
        private IColorProvider              ColorProvider       { get; }
        private IViewGameTicker             GameTicker          { get; }

        private ViewUILevelSkipperButton(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            IHapticsManager             _HapticsManager,
            IViewInputTouchProceeder    _TouchProceeder,
            ICameraProvider             _CameraProvider,
            ILocalizationManager        _LocalizationManager,
            IAdsManager                 _AdsManager,
            IColorProvider              _ColorProvider,
            IViewGameTicker             _GameTicker)
        {
            ViewSettings        = _ViewSettings;
            Model               = _Model;
            CommandsProceeder   = _CommandsProceeder;
            ContainersGetter    = _ContainersGetter;
            PrefabSetManager    = _PrefabSetManager;
            HapticsManager      = _HapticsManager;
            TouchProceeder      = _TouchProceeder;
            CameraProvider      = _CameraProvider;
            LocalizationManager = _LocalizationManager;
            AdsManager          = _AdsManager;
            ColorProvider       = _ColorProvider;
            GameTicker          = _GameTicker;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_TopOffset = _Offsets.w;
            ColorProvider.ColorChanged += OnColorChanged;
        }
        
        public bool LevelSkipped { get; private set; }

        public void SkipLevel()
        {
            LevelSkipped = true;
            AdsManager.ShowRewardedAd(null, () =>
            {
                CommandsProceeder.RaiseCommand(
                    EInputCommand.FinishLevel, 
                    new object[] {"skip"}, 
                    true);
            });
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelIndex < MinimumLevelToSkip)
                return;
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    if (!m_Initialized)
                    {
                        InitButton();
                        m_Initialized = true;
                    }
                    LevelSkipped = false;
                    break;
                case ELevelStage.ReadyToStart:
                    ActivateButton(false);
                    break;
                case ELevelStage.StartedOrContinued when 
                    _Args.PreviousStage == ELevelStage.ReadyToStart
                    && _Args.PrePreviousStage == ELevelStage.Loaded:
                    Cor.Stop(m_ShowButtonCoroutine);
                    m_ShowButtonCoroutine = ShowButtonCountdownCoroutine();
                    Cor.Run(m_ShowButtonCoroutine);
                    break;
                case ELevelStage.Finished:
                    if (LevelSkipped)
                        CommandsProceeder.RaiseCommand(EInputCommand.ReadyToUnloadLevel, null, true);
                    Cor.Stop(m_ShowButtonCoroutine);
                    ActivateButton(false);
                    break;
                case ELevelStage.CharacterKilled:
                    Cor.Stop(m_ShowButtonCoroutine);
                    break;
            }
        }

        #endregion

        #region nonpublic methods

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.UiBorder:     m_Border.color = _Color;     break;
                case ColorIds.UiBackground: m_Background.color = _Color; break;
            }
        }
        
        private void OnSkipLevelButtonPressed()
        {
            SkipLevel();
            ActivateButton(false);
        }
        
        private void InitButton()
        {
            var parent = ContainersGetter.GetContainer(ContainerNames.GameUI);
            var go = PrefabSetManager.InitPrefab(
                parent, "ui_game", "skip_level_button");
            var tr = go.transform;
            tr.SetLocalScaleXY(Vector2.one * 0.3f); 
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            float yPos = screenBounds.max.y - m_TopOffset - 8f;
            tr.SetLocalPosXY(screenBounds.center.x, yPos);
            m_Text = go.GetCompItem<TextMeshPro>("text");
            var locInfo = new LocalizableTextObjectInfo(m_Text, ETextType.GameUI, "skip_level");
            LocalizationManager.AddTextObject(locInfo);
            var button = go.GetCompItem<ButtonOnRaycast>("button");
            button.Init(OnSkipLevelButtonPressed,
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                HapticsManager,
                TouchProceeder);
            m_Border = go.GetCompItem<SpriteRenderer>("border");
            m_Background = go.GetCompItem<SpriteRenderer>("background");
            m_ButtonObj = go;
            m_Border.color = ColorProvider.GetColor(ColorIds.UiBorder);
            m_Background.color = ColorProvider.GetColor(ColorIds.UiDialogBackground);
        }
        
        private IEnumerator ShowButtonCountdownCoroutine()
        {
            Dbg.Log(nameof(ShowButtonCountdownCoroutine));
            yield return Cor.Delay(ViewSettings.skipLevelSeconds, 
                GameTicker,
                () =>
            {
                if (AdsManager.RewardedAdReady)
                    ActivateButton(true);
            });
        }

        private void ActivateButton(bool _Active)
        {
            m_ButtonObj.SetActive(_Active);
        }

        #endregion


    }
}