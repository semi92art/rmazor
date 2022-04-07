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
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public interface IViewUIPrompt : IOnLevelStageChanged, IInitViewUIItem
    {
        void OnTutorialStarted(ETutorialType _Type);
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
        private bool        m_IsPromptObjectNull = true;
        private TextMeshPro m_PromptText;


        #endregion

        #region inject

        private IModelGame                  Model               { get; }
        private IViewGameTicker             GameTicker          { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private ILocalizationManager        LocalizationManager { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }
        private ICameraProvider             CameraProvider      { get; }
        private IColorProvider              ColorProvider       { get; }
        private IPrefabSetManager           PrefabSetManager    { get; }

        public ViewUIPrompt(
            IModelGame                  _Model,
            IViewGameTicker             _GameTicker,
            IContainersGetter           _ContainersGetter,
            ILocalizationManager        _LocalizationManager,
            IViewInputCommandsProceeder _CommandsProceeder,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IPrefabSetManager           _PrefabSetManager)
        {
            Model = _Model;
            GameTicker = _GameTicker;
            ContainersGetter = _ContainersGetter;
            LocalizationManager = _LocalizationManager;
            CommandsProceeder = _CommandsProceeder;
            CameraProvider = _CameraProvider;
            ColorProvider = _ColorProvider;
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_BottomOffset = _Offsets.z;
            GameTicker.Register(this);
            CommandsProceeder.Command += OnCommand;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.ReadyToStart:
                    if (_Args.PreviousStage != ELevelStage.Paused)
                    {
                        if (!m_InTutorial)
                            ShowPromptSwipeToStart();
                    }
                    break;
                case ELevelStage.StartedOrContinued:
                    if (_Args.PreviousStage != ELevelStage.Paused)
                        HidePrompt();
                    break;
                case ELevelStage.Finished:
                    if (_Args.PreviousStage != ELevelStage.Paused)
                        ShopPromptTapToNext();
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    if (_Args.PreviousStage != ELevelStage.Paused 
                        || _Args.PrePreviousStage == ELevelStage.ReadyToUnloadLevel)
                        HidePrompt();
                    break;
                case ELevelStage.Loaded:
                case ELevelStage.Paused:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        public void UpdateTick()
        {
            if (m_RunShowPromptCoroutine)
                Cor.Run(ShowPromptCoroutine());
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
        
        private void OnCommand(EInputCommand _Key, object[] _Args)
        {

        }

        private void ShowPromptSwipeToStart()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds();
            var position = new Vector3(
                screenBounds.center.x,
                GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y + m_BottomOffset + 4f);
            ShowPrompt(KeyPromptSwipeToStart, position);
        }

        private void ShopPromptTapToNext()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds();
            var position = new Vector3(
                screenBounds.center.x,
                GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y + m_BottomOffset + 4f);
            ShowPrompt(KeyPromptTapToNext, position);
        }

        private void ShowPrompt(string _Key, Vector3 _Position)
        {
            if (m_CurrentPromptInfo?.Key == _Key)
                return;
            if (m_IsPromptObjectNull)
            {
                m_PromptObject = PrefabSetManager.InitPrefab(
                    null, "ui_game", "prompt");
                m_PromptObject.SetParent(ContainersGetter.GetContainer(ContainerNames.GameUI));
                m_PromptText = m_PromptObject.GetCompItem<TextMeshPro>("label");
                m_PromptText.fontSize = 18f;
                m_IsPromptObjectNull = false;
            }
            m_PromptObject.transform.position = _Position;
            LocalizationManager.AddTextObject(m_PromptText, _Key);
            m_PromptText.enabled = true;
            m_PromptText.color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
            var screenBounds = GraphicUtils.GetVisibleBounds();
            m_PromptText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenBounds.size.x);
            if (m_CurrentPromptInfo != null && m_CurrentPromptInfo.PromptGo != null)
                m_CurrentPromptInfo.PromptGo.DestroySafe();
            m_CurrentPromptInfo = new PromptInfo {Key = _Key, PromptGo = m_PromptObject, PromptText = m_PromptText};
            m_RunShowPromptCoroutine = true;
        }

        private void HidePrompt()
        {
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
                0f,
                1f,
                loopTime * 2f,
                _Progress =>
                {
                    var firstColor = ColorProvider.GetColor(ColorIds.UI);
                    var secondColor = new Color(1f, 0.54f, 0.55f);
                    m_CurrentPromptInfo.PromptText.color = Color.Lerp(firstColor, secondColor, _Progress);
                },
                GameTicker,
                (_, __) => m_RunShowPromptCoroutine = true,
                () => m_CurrentPromptInfo == null || m_CurrentPromptInfo.NeedToHide,
                _Progress => _Progress < 0.5f ? 2f * _Progress : 2f * (1f - _Progress));
        }

        #endregion
    }
}