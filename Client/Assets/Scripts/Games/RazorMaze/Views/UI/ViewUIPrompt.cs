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
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIPrompt : IOnLevelStageChanged, IInitViewUIItem
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

        private float      m_BottomOffset;
        private bool       m_HowToRotateClockwisePromptHidden;
        private bool       m_HowToRotateCounterClockwisePromptHidden;
        private bool       m_PromptHowToRotateShown;
        private PromptInfo m_CurrentPromptInfo;
        private bool       m_RunShowPromptCoroutine;


        #endregion

        #region inject

        private IModelGame                  Model               { get; }
        private IViewGameTicker             GameTicker          { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private ILocalizationManager        LocalizationManager { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }
        private ICameraProvider             CameraProvider      { get; }
        private IColorProvider              ColorProvider       { get; }

        public ViewUIPrompt(
            IModelGame _Model,
            IViewGameTicker _GameTicker,
            IContainersGetter _ContainersGetter,
            ILocalizationManager _LocalizationManager,
            IViewInputCommandsProceeder _CommandsProceeder,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider)
        {
            Model = _Model;
            GameTicker = _GameTicker;
            ContainersGetter = _ContainersGetter;
            LocalizationManager = _LocalizationManager;
            CommandsProceeder = _CommandsProceeder;
            CameraProvider = _CameraProvider;
            ColorProvider = _ColorProvider;
        }

        #endregion

        #region api

        public bool InTutorial { get; private set; }

        public void Init(Vector4 _Offsets)
        {
            m_BottomOffset = _Offsets.z;
            GameTicker.Register(this);
            CommandsProceeder.Command += InputConfiguratorOnCommand;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    m_PromptHowToRotateShown = SaveUtils.GetValue(SaveKeys.PromptHowToRotateShown);
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
                .Any(_Info => RazorMazeUtils.GravityItemTypes().Contains(_Info.Type));
        }
        
        private void InputConfiguratorOnCommand(EInputCommand _Key, object[] _Args)
        {
            if (_Key != EInputCommand.RotateClockwise && _Key != EInputCommand.RotateCounterClockwise)
                return;
            if (m_PromptHowToRotateShown || !MazeContainsGravityItems())
                return;
            if (_Key == EInputCommand.RotateClockwise)
            {
                if (m_HowToRotateClockwisePromptHidden)
                    return;
                m_HowToRotateClockwisePromptHidden = true;
                HidePrompt();
                ShowPromptHowToRotateCounterClockwise();
            }
            else if (_Key == EInputCommand.RotateCounterClockwise)
            {
                if (m_HowToRotateClockwisePromptHidden)
                {
                    if (m_HowToRotateCounterClockwisePromptHidden)
                        return;
                    m_HowToRotateCounterClockwisePromptHidden = true;
                    HidePrompt();
                    ShowPromptSwipeToStart();
                    CommandsProceeder.UnlockAllCommands();
                    InTutorial = false;
                    m_PromptHowToRotateShown = true;
                    SaveUtils.PutValue(SaveKeys.PromptHowToRotateShown, true);
                }
            }
        }

        private void ShowPromptHowToRotateClockwise()
        {
            CommandsProceeder.LockAllCommands();
            CommandsProceeder.UnlockCommand(EInputCommand.RotateClockwise);
            var screenBounds = GraphicUtils.GetVisibleBounds();
            var position = new Vector3(
                screenBounds.center.x,
                GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y + m_BottomOffset + 10f);
            ShowPrompt(KeyPromptHowToRotateClockwise, position);
        }
        
        private void ShowPromptHowToRotateCounterClockwise()
        {
            CommandsProceeder.LockAllCommands();
            CommandsProceeder.UnlockCommand(EInputCommand.RotateCounterClockwise);
            var screenBounds = GraphicUtils.GetVisibleBounds();
            var position = new Vector3(
                screenBounds.center.x,
                GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y + m_BottomOffset + 10f);
            ShowPrompt(KeyPromptHowToRotateCounter, position);
        }

        private void ShowPromptSwipeToStart()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds();
            var position = new Vector3(
                screenBounds.center.x,
                GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y + m_BottomOffset + 5f);
            ShowPrompt(KeyPromptSwipeToStart, position);
        }

        private void ShopPromptTapToNext()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds();
            var position = new Vector3(
                screenBounds.center.x,
                GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera).min.y + m_BottomOffset + 5f);
            ShowPrompt(KeyPromptTapToNext, position);
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
            var screenBounds = GraphicUtils.GetVisibleBounds();
            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenBounds.size.x);
            if (m_CurrentPromptInfo != null && m_CurrentPromptInfo.PromptGo != null)
                m_CurrentPromptInfo.PromptGo.DestroySafe();
            m_CurrentPromptInfo = new PromptInfo {Key = _Key, PromptGo = promptGo, PromptText = text};
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
            var secondColor = Color.red;
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
                    var firstColor = ColorProvider.GetColor(ColorIds.UI);
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