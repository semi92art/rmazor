using System.Collections;
using System.Collections.Generic;
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
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIPrompts : IOnLevelStageChanged
    {
        bool InTutorial { get; }
    }

    public class ViewUIPrompts : IViewUIPrompts
    {
        #region types

        private class PromptArgs
        {
            public GameObject Prompt { get; set; }
            public TextMeshPro PromptText { get; set; }
            public bool NeedToHide { get; set; }
        }

        #endregion

        #region constants

        private const string KeyPromptHowToRotateClockwise = "swipe_to_rotate_clockwise";
        private const string KeyPromptHowToRotateCounter   = "swipe_to_rotate_counter";
        private const string KeyPromptSwipeToStart         = "swipe_to_start";
        private const string KeyPromptTapToNext            = "tap_to_next";

        #endregion

        #region nonpublic members
        
        private bool m_HowToRotateClockwisePromptHidden;
        private bool m_HowToRotateCounterClockwisePromptHidden;
        private bool m_PromptHowToRotateShown;

        private readonly Dictionary<string, PromptArgs> m_Prompts = new Dictionary<string, PromptArgs>();

        #endregion

        #region inject

        private IModelGame               Model               { get; }
        private IGameTicker              GameTicker          { get; }
        private IContainersGetter        ContainersGetter    { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private ILocalizationManager     LocalizationManager { get; }
        private IViewInput   Input   { get; }
        private ICameraProvider          CameraProvider      { get; }
        private IColorProvider           ColorProvider       { get; }

        public ViewUIPrompts(
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
            Input.Command += InputConfiguratorOnCommand;
        }

        #endregion

        #region api

        public bool InTutorial { get; private set; }
        
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
                        HidePrompt(KeyPromptSwipeToStart);
                    break;
                case ELevelStage.Finished:
                    if (_Args.PreviousStage != ELevelStage.Paused)
                        ShopPromptTapToNext();
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    if (_Args.PreviousStage != ELevelStage.Paused 
                        || _Args.PreviousStage == ELevelStage.Paused 
                        && _Args.PrePreviousStage == ELevelStage.ReadyToUnloadLevel)
                        HidePrompt(KeyPromptTapToNext);
                    break;
            }
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
                HidePrompt(KeyPromptHowToRotateClockwise);
                ShowPromptHowToRotateCounterClockwise();
            }
            else if (_Key == InputCommands.RotateCounterClockwise)
            {
                if (m_HowToRotateClockwisePromptHidden)
                {
                    if (m_HowToRotateCounterClockwisePromptHidden)
                        return;
                    m_HowToRotateCounterClockwisePromptHidden = true;
                    HidePrompt(KeyPromptHowToRotateCounter);
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
            if (m_Prompts.ContainsKey(_Key))
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
            
            if (!m_Prompts.ContainsKey(_Key))
                m_Prompts.Add(_Key, new PromptArgs { Prompt = promptGo, PromptText = text });
            Coroutines.Run(ShowPromptCoroutine(_Key, text));
        }

        private void HidePrompt(string _Key)
        {
            if (m_Prompts.ContainsKey(_Key))
                m_Prompts[_Key].NeedToHide = true;
        }

        private IEnumerator ShowPromptCoroutine(string _Key, TextMeshPro _Text)
        {
            const float loopTime = 1f;
            if (!m_Prompts.ContainsKey(_Key)) 
                yield break;
            if (!m_Prompts[_Key].NeedToHide)
            {
                yield return Coroutines.Lerp(
                    0f,
                    1f,
                    loopTime * 2f,
                    _Progress => _Text.color = ColorProvider.GetColor(ColorIds.UI).SetA(_Progress),
                    GameTicker,
                    (_, __) => Coroutines.Run(ShowPromptCoroutine(_Key, _Text)),
                    () => m_Prompts.ContainsKey(_Key) && m_Prompts[_Key].NeedToHide,
                    _Progress => _Progress < 0.5f ? 2f * _Progress : 2f * (1f - _Progress));
                yield break;
            }
            if (!m_Prompts.ContainsKey(_Key)) 
                yield break;
            m_Prompts[_Key].Prompt.DestroySafe();
            m_Prompts.Remove(_Key);
        }

        #endregion
    }
}