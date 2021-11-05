using System.Collections;
using System.Linq;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.InputConfigurators;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIPrompt : IOnLevelStageChanged, IInit
    {
        bool InTutorial { get; }
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

        private const string KeyPromptHowToRotateClockwise = "swipe_to_rotate_clockwise";
        private const string KeyPromptHowToRotateCounter   = "swipe_to_rotate_counter";
        private const string KeyPromptSwipeToStart         = "swipe_to_start";
        private const string KeyPromptTapToNext            = "tap_to_next";

        #endregion

        #region nonpublic members
        
        private bool       m_HowToRotateClockwisePromptHidden;
        private bool       m_HowToRotateCounterClockwisePromptHidden;
        private bool       m_PromptHowToRotateShown;
        private PromptInfo m_CurrentPromptInfo;
        private bool       m_RunShowPromptCoroutine;


        #endregion

        #region inject

        private IModelGame               Model               { get; }
        private IGameTicker              GameTicker          { get; }
        private IContainersGetter        ContainersGetter    { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private ILocalizationManager     LocalizationManager { get; }
        private IViewInput               Input               { get; }
        private ICameraProvider          CameraProvider      { get; }
        private IColorProvider           ColorProvider       { get; }

        public ViewUIPrompt(
            IModelGame _Model,
            IGameTicker _GameTicker,
            IContainersGetter _ContainersGetter,
            IMazeCoordinateConverter _CoordinateConverter,
            ILocalizationManager _LocalizationManager,
            IViewInput _Input,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider)
        {
            Model = _Model;
            GameTicker = _GameTicker;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            LocalizationManager = _LocalizationManager;
            Input = _Input;
            CameraProvider = _CameraProvider;
            ColorProvider = _ColorProvider;
        }

        #endregion

        #region api

        public bool              InTutorial { get; private set; }
        public event UnityAction Initialized;
        
        public void Init()
        {
            GameTicker.Register(this);
            Input.Command += InputConfiguratorOnCommand;
            Initialized?.Invoke();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    m_PromptHowToRotateShown = SaveUtils.GetValue<bool>(SaveKey.PromptHowToRotateShown);
                    break;
                case ELevelStage.ReadyToStart:
                    if (_Args.PreviousStage != ELevelStage.Paused)
                    {
                        if (!m_PromptHowToRotateShown && MazeContainsGravityItems())
                        {
                            InTutorial = true;
                            ShowPromptHowToRotateClockwise();
                        }
                        else
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
                        || _Args.PreviousStage == ELevelStage.Paused 
                        && _Args.PrePreviousStage == ELevelStage.ReadyToUnloadLevel)
                        HidePrompt();
                    break;
            }
        }
        
        public void UpdateTick()
        {
            if (m_RunShowPromptCoroutine)
                Coroutines.Run(ShowPromptCoroutine());
        }

        #endregion

        #region nonpublic methods

        private bool MazeContainsGravityItems()
        {
            return Model.GetAllProceedInfos()
                .Any(_Info => _Info.Type == EMazeItemType.GravityBlock
                              || _Info.Type == EMazeItemType.GravityTrap);
        }
        
        private void InputConfiguratorOnCommand(int _Key, object[] _Args)
        {
            if (m_PromptHowToRotateShown || !MazeContainsGravityItems())
                return;
            if (_Key == InputCommands.RotateClockwise)
            {
                if (m_HowToRotateClockwisePromptHidden)
                    return;
                m_HowToRotateClockwisePromptHidden = true;
                HidePrompt();
                ShowPromptHowToRotateCounterClockwise();
            }
            else if (_Key == InputCommands.RotateCounterClockwise)
            {
                if (m_HowToRotateClockwisePromptHidden)
                {
                    if (m_HowToRotateCounterClockwisePromptHidden)
                        return;
                    m_HowToRotateCounterClockwisePromptHidden = true;
                    HidePrompt();
                    ShowPromptSwipeToStart();
                    Input.UnlockAllCommands();
                    InTutorial = false;
                    m_PromptHowToRotateShown = true;
                    SaveUtils.PutValue(SaveKey.PromptHowToRotateShown, true);
                }
            }
        }

        private void ShowPromptHowToRotateClockwise()
        {
            Input.LockAllCommands();
            Input.UnlockCommand(InputCommands.RotateClockwise);
            var mazeBounds = CoordinateConverter.GetMazeBounds();
            ShowPrompt(KeyPromptHowToRotateClockwise, new Vector3(mazeBounds.center.x, 
                GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y));
        }
        
        private void ShowPromptHowToRotateCounterClockwise()
        {
            Input.LockAllCommands();
            Input.UnlockCommand(InputCommands.RotateCounterClockwise);
            var mazeBounds = CoordinateConverter.GetMazeBounds();
            ShowPrompt(KeyPromptHowToRotateCounter, new Vector3(mazeBounds.center.x, 
                GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y + 1f));
        }

        private void ShowPromptSwipeToStart()
        {
            ShowPrompt(KeyPromptSwipeToStart, new Vector3(
                CoordinateConverter.GetMazeBounds().center.x,
                (GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y 
                 + CoordinateConverter.GetMazeBounds().min.y) * 0.5f));
        }

        private void ShopPromptTapToNext()
        {
            ShowPrompt(KeyPromptTapToNext, new Vector3(
                CoordinateConverter.GetMazeBounds().center.x,
                (GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y 
                 + CoordinateConverter.GetMazeBounds().min.y) * 0.5f));
        }

        private void ShowPrompt(string _Key, Vector3 _Position)
        {
            if (m_CurrentPromptInfo?.Key == _Key)
                return;
            var promptGo = PrefabUtilsEx.InitPrefab(
                null, "ui_game", _Key);
            promptGo.SetParent(ContainersGetter.GetContainer(ContainerNames.GameUI));
            promptGo.transform.position = _Position;
            var text = promptGo.GetCompItem<TextMeshPro>("label");
            text.fontSize = 18f;
            LocalizationManager.AddTextObject(text, _Key);
            text.color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
            var mazeBounds = CoordinateConverter.GetMazeBounds();
            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mazeBounds.size.x);
            if (m_CurrentPromptInfo != null && m_CurrentPromptInfo.PromptGo != null)
                m_CurrentPromptInfo.PromptGo.DestroySafe();
            m_CurrentPromptInfo = new PromptInfo {Key = _Key, PromptGo = promptGo, PromptText = text};
            m_RunShowPromptCoroutine = true;
        }

        private void HidePrompt()
        {
            m_CurrentPromptInfo.NeedToHide = true;
        }

        private IEnumerator ShowPromptCoroutine()
        {
            m_RunShowPromptCoroutine = false;
            const float loopTime = 1f;
            if (m_CurrentPromptInfo != null && m_CurrentPromptInfo.NeedToHide)
            {
                m_CurrentPromptInfo.PromptGo.DestroySafe();
                m_CurrentPromptInfo = null;
                yield break;
            }
            yield return Coroutines.Lerp(
                0f,
                1f,
                loopTime * 2f,
                _Progress =>
                {
                    m_CurrentPromptInfo.PromptText.color = 
                        ColorProvider.GetColor(ColorIds.UI).SetA(_Progress);
                },
                GameTicker,
                (_, __) => m_RunShowPromptCoroutine = true,
                () => m_CurrentPromptInfo == null || m_CurrentPromptInfo.NeedToHide,
                _Progress => _Progress < 0.5f ? 2f * _Progress : 2f * (1f - _Progress));
        }

        #endregion
    }
}