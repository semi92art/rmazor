using System.Collections;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using TMPro;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public enum EPromptType
    {
        SwipeToStart,
        TapToNext
    }
    
    public interface IViewUIPrompt : IOnLevelStageChanged, IInitViewUIItem
    {
        void ShowPrompt(EPromptType           _PromptType);
        void OnTutorialStarted(ETutorialType  _Type);
        void OnTutorialFinished(ETutorialType _Type);
    }

    public class ViewUIPrompt : IViewUIPrompt, IUpdateTick
    {
        #region types

        private class PromptInfo
        {
            public string      Key        { get; set; }
            public GameObject  PromptGo   { get; set; }
            public TextMeshPro PromptText { get; set; }
            public bool        NeedToHide { get; set; }
        }

        #endregion

        #region constants

        private const string KeyPromptSwipeToStart = "swipe_to_start";
        private const string KeyPromptTapToNext    = "tap_to_next";

        #endregion

        #region nonpublic members

        private float       m_BottomOffset;
        private PromptInfo  m_CurrentPromptInfo;
        private bool        m_RunShowPromptCoroutine;
        private bool        m_InTutorial;
        private GameObject  m_PromptObject;
        private TextMeshPro m_PromptText;
        
        #endregion

        #region inject

        private IModelGame              Model               { get; }
        private IViewGameTicker         GameTicker          { get; }
        private IContainersGetter       ContainersGetter    { get; }
        private ILocalizationManager    LocalizationManager { get; }
        private ICameraProvider         CameraProvider      { get; }
        private IColorProvider          ColorProvider       { get; }
        private IPrefabSetManager       PrefabSetManager    { get; }
        private IViewUIRotationControls RotationControls    { get; }
        private IMoneyCounter           MoneyCounter        { get; }

        private ViewUIPrompt(
            IModelGame              _Model,
            IViewGameTicker         _GameTicker,
            IContainersGetter       _ContainersGetter,
            ILocalizationManager    _LocalizationManager,
            ICameraProvider         _CameraProvider,
            IColorProvider          _ColorProvider,
            IPrefabSetManager       _PrefabSetManager,
            IViewUIRotationControls _RotationControls,
            IMoneyCounter           _MoneyCounter)
        {
            Model               = _Model;
            GameTicker          = _GameTicker;
            ContainersGetter    = _ContainersGetter;
            LocalizationManager = _LocalizationManager;
            CameraProvider      = _CameraProvider;
            ColorProvider       = _ColorProvider;
            PrefabSetManager    = _PrefabSetManager;
            RotationControls    = _RotationControls;
            MoneyCounter        = _MoneyCounter;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            GameTicker.Register(this);
            m_BottomOffset = _Offsets.z;
            m_PromptObject = PrefabSetManager.InitPrefab(
                null, CommonPrefabSetNames.UiGame, "prompt");
            m_PromptObject.SetParent(ContainersGetter.GetContainer(ContainerNames.GameUI));
            m_PromptText = m_PromptObject.GetCompItem<TextMeshPro>("label");
            m_PromptText.fontSize = 18f;
            m_PromptText.enabled = false;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart
                    when _Args.PreviousStage != ELevelStage.Paused && !m_InTutorial:
                {
                    ShowPromptSwipeToStart();
                }
                    break;
                case ELevelStage.StartedOrContinued 
                    when _Args.PreviousStage != ELevelStage.Paused:
                {
                    HidePrompt();
                }        
                    break;
                case ELevelStage.Finished 
                    when _Args.PreviousStage != ELevelStage.Paused:
                {
                    ShopPromptTapToNext();
                }
                    break;
                case ELevelStage.ReadyToUnloadLevel 
                    when _Args.PreviousStage != ELevelStage.Paused 
                         || _Args.PrePreviousStage == ELevelStage.ReadyToUnloadLevel:
                {
                    HidePrompt();
                }    
                    break;
                case ELevelStage.Unloaded:
                    HidePrompt(true);
                    break;
                case ELevelStage.Loaded:
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Finished:
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Paused:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.LevelStage);
            }
        }
        
        public void UpdateTick()
        {
            if (m_RunShowPromptCoroutine)
                Cor.Run(ShowPromptCoroutine());
        }

        public void ShowPrompt(EPromptType _PromptType)
        {
            switch (_PromptType)
            {
                case EPromptType.SwipeToStart: ShowPromptSwipeToStart(); break;
                case EPromptType.TapToNext:    ShopPromptTapToNext();    break;
                default: throw new SwitchCaseNotImplementedException(_PromptType);
            }
        }

        public void OnTutorialStarted(ETutorialType _Type)
        {
            m_InTutorial = true;
        }

        public void OnTutorialFinished(ETutorialType _Type)
        {
            m_InTutorial = false;
        }

        #endregion

        #region nonpublic methods
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            m_PromptObject.SetParent(_Camera.transform);
        }

        private void ShowPromptSwipeToStart()
        {
            var position = GetDefaultPosition();
            ShowPrompt(KeyPromptSwipeToStart, position);
        }

        private void ShopPromptTapToNext()
        {
            var position = GetDefaultPosition();
            ShowPrompt(KeyPromptTapToNext, position);
        }

        private Vector2 GetDefaultPosition()
        {
            bool rotationButtonsShowing = RotationControls.HasButtons
                                          && RmazorUtils.MazeContainsGravityItems(Model.GetAllProceedInfos());
            float addIndentY = m_BottomOffset + 4f + (rotationButtonsShowing ? 7f : 2f);
            var screenBounds = GraphicUtils.GetVisibleBounds();
            float xPos = screenBounds.center.x;
            float yPos = GraphicUtils.GetVisibleBounds(CameraProvider.Camera).min.y + addIndentY;
            return new Vector2(xPos, yPos);
        }

        private void ShowPrompt(string _Key, Vector3 _Position)
        {
            if (m_CurrentPromptInfo != null && m_CurrentPromptInfo.PromptGo.IsNull())
            {
                Dbg.LogError("Prompt was unexpectedly destructed.");
                return;
            }
            if (m_CurrentPromptInfo?.Key == _Key)
                return;
            m_PromptObject.transform.SetLocalPosXY(_Position);
            var info = new LocalizableTextObjectInfo(m_PromptText, ETextType.GameUI, _Key);
            LocalizationManager.AddTextObject(info);
            m_PromptText.enabled = true;
            m_PromptText.color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
            var screenBounds = GraphicUtils.GetVisibleBounds();
            m_PromptText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenBounds.size.x);
            m_CurrentPromptInfo = new PromptInfo
            {
                Key        = _Key, 
                PromptGo   = m_PromptObject,
                PromptText = m_PromptText
            };
            m_RunShowPromptCoroutine = true;
        }

        private void HidePrompt(bool _Instantly = false)
        {
            if (m_CurrentPromptInfo == null)
                return;
            if (_Instantly)
            {
                m_CurrentPromptInfo.PromptText.enabled = false;
                return;
            }
            if (m_CurrentPromptInfo != null)
                m_CurrentPromptInfo.NeedToHide = true;
        }

        private IEnumerator ShowPromptCoroutine()
        {
            m_RunShowPromptCoroutine = false;
            const float loopTime = 1f;
            if (m_CurrentPromptInfo != null 
                && m_CurrentPromptInfo.NeedToHide 
                && m_CurrentPromptInfo.PromptText.IsNotNull())
            {
                m_CurrentPromptInfo.PromptText.enabled = false;
                m_CurrentPromptInfo = null;
                yield break;
            }
            yield return Cor.Lerp(
                GameTicker,
                loopTime * 2f,
                _OnProgress: _P =>
                {
                    var firstColor = Color.black;
                    var secondColor = ColorProvider.GetColor(ColorIds.UI);
                    m_CurrentPromptInfo.PromptText.color = Color.Lerp(firstColor, secondColor, _P);
                },
                _OnFinish:() => m_RunShowPromptCoroutine = true,
                _BreakPredicate: () => m_CurrentPromptInfo == null || m_CurrentPromptInfo.NeedToHide,
                _ProgressFormula: _P => _P < 0.5f ? 2f * _P : 2f * (1f - _P));
        }

        #endregion
    }
}