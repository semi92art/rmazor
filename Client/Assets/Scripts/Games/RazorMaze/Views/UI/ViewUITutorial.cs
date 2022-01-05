using System.Collections;
using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Factories;
using Games.RazorMaze.Views.InputConfigurators;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public enum ETutorialType
    {
        Movement,
        Rotation
    }
    
    public interface IViewUITutorial : IOnLevelStageChanged, IInitViewUIItem
    {
        event UnityAction<ETutorialType> TutorialStarted;
        event UnityAction<ETutorialType> TutorialFinished;
    }
    
    public class ViewUITutorial : IViewUITutorial
    {
        #region constants

        #endregion

        #region nonpublic members

        private bool                          m_MovementTutorialStarted;
        private bool                          m_RotationTutorialStarted;
        private bool                          m_MovementTutorialFinished;
        private bool                          m_RotationTutorialFinished;
        private bool                          m_ReadyToSecondMovementStep;
        private bool                          m_ReadyToThirdMovementStep;
        private bool                          m_ReadyToFourthMovementStep;
        private bool                          m_ReadyToFinishMovementTutorial;
        private bool                          m_ReadyToSecondRotationStep;
        private bool                          m_ReadyToFinishRotationTutorial;
        private Vector4                       m_Offsets;
        private HandSwipeMovement             m_Hsm;
        private HandSwipeRotation             m_Hsr;
        private IRotatingPossibilityIndicator m_RotPossIndicator;
        private TextMeshPro                   m_RotPossText;
        private Animator                      m_RotPossTextAnim;
        private AnimationTriggerer            m_RotPossTextTriggerer;

        #endregion

        #region inject

        private IModelGame                           Model               { get; }
        private IPrefabSetManager                    PrefabSetManager    { get; }
        private IContainersGetter                    ContainersGetter    { get; }
        private IMazeCoordinateConverter             CoordinateConverter { get; }
        private IViewInputCommandsProceeder          CommandsProceeder   { get; }
        private ICameraProvider                      CameraProvider      { get; }
        private IColorProvider                       ColorProvider       { get; }
        private IRotatingPossibilityIndicatorFactory RotPossIndFactory   { get; }
        private ILocalizationManager                 LocalizationManager { get; }
        private IViewGameTicker                      Ticker              { get; }

        public ViewUITutorial(
            IModelGame _Model,
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            IMazeCoordinateConverter _CoordinateConverter,
            IViewInputCommandsProceeder _CommandsProceeder,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider,
            IRotatingPossibilityIndicatorFactory _RotPossIndFactory,
            ILocalizationManager _LocalizationManager,
            IViewGameTicker _Ticker)
        {
            Model = _Model;
            PrefabSetManager = _PrefabSetManager;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            CommandsProceeder = _CommandsProceeder;
            CameraProvider = _CameraProvider;
            ColorProvider = _ColorProvider;
            RotPossIndFactory = _RotPossIndFactory;
            LocalizationManager = _LocalizationManager;
            Ticker = _Ticker;
        }

        #endregion

        #region api
        
        public event UnityAction<ETutorialType> TutorialStarted;
        public event UnityAction<ETutorialType> TutorialFinished;
        
        public void Init(Vector4 _Offsets)
        {
            m_Offsets = _Offsets;
            m_MovementTutorialFinished = SaveUtils.GetValue(SaveKeys.MovementTutorialFinished);
            m_RotationTutorialFinished = SaveUtils.GetValue(SaveKeys.RotationTutorialFinished);
            CommandsProceeder.Command += OnCommand;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    switch (Model.Data.Info.Comment)
                    {
                        case "movement tutorial": StartMovementTutorial(); break;
                        case "rotation tutorial": StartRotationTutorial(); break;
                    }
                    break;
            }
        }

        #endregion

        #region nonpublic methods
        
        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (m_MovementTutorialStarted && !m_MovementTutorialFinished)
                OnMovementCommand(_Command);
            if (m_RotationTutorialStarted && !m_RotationTutorialFinished)
                OnRotationCommand(_Command);
        }

        private void OnMovementCommand(EInputCommand _Command)
        {
            switch (_Command)
            {
                case EInputCommand.MoveRight:
                    m_ReadyToSecondMovementStep = true;
                    break;
                case EInputCommand.MoveUp:
                    m_ReadyToThirdMovementStep = true;
                    break;
                case EInputCommand.MoveLeft:
                    m_ReadyToFourthMovementStep = true;
                    break;
                case EInputCommand.MoveDown:
                    m_ReadyToFinishMovementTutorial = true;
                    break;
            }
        }

        private void OnRotationCommand(EInputCommand _Command)
        {
            switch (_Command)
            {
                case EInputCommand.RotateCounterClockwise:
                    m_ReadyToSecondRotationStep = true;
                    break;
                case EInputCommand.RotateClockwise:
                    m_ReadyToFinishRotationTutorial = true;
                    break;
            }   
        }

        private void StartMovementTutorial()
        {
            if (m_MovementTutorialStarted || m_MovementTutorialFinished)
                return;
            TutorialStarted?.Invoke(ETutorialType.Movement);
            var cont = ContainersGetter.GetContainer(ContainerNames.Tutorial);
            var goMovePrompt = PrefabSetManager.InitPrefab(
                cont, "tutorials", "hand_swipe_movement");
            goMovePrompt.transform.localScale = Vector3.one * 6f;
            m_Hsm = goMovePrompt.GetCompItem<HandSwipeMovement>("hsm");
            m_Hsm.Init(Ticker, CameraProvider, CoordinateConverter, ColorProvider, m_Offsets);
            Coroutines.Run(MovementTutorialFirstStepCoroutine());
            m_MovementTutorialStarted = true;
        }

        private void StartRotationTutorial()
        {
            if (m_RotationTutorialStarted || m_RotationTutorialFinished)
                return;
            SaveUtils.PutValue(SaveKeys.EnableRotation, true);
            TutorialStarted?.Invoke(ETutorialType.Rotation);
            var cont = ContainersGetter.GetContainer(ContainerNames.Tutorial);
            var goRotPrompt = PrefabSetManager.InitPrefab(
                cont, "tutorials", "hand_swipe_rotation");
            goRotPrompt.transform.localScale = Vector3.one * 6f;
            m_Hsr = goRotPrompt.GetCompItem<HandSwipeRotation>("hsr");
            m_Hsr.Init(Ticker, CameraProvider, CoordinateConverter, ColorProvider, m_Offsets);
            CommandsProceeder.LockCommands(RazorMazeUtils.GetMoveCommands(), GetGroupName());
            Coroutines.Run(RotationTutorialFirstStepCoroutine());
            m_RotationTutorialStarted = true;
        }

        
        private IEnumerator MovementTutorialFirstStepCoroutine()
        {
            m_Hsm.ShowMoveRightPrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.GetMoveCommands(), GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveRight, GetGroupName());
            while (!m_ReadyToSecondMovementStep)
                yield return null;
            yield return MovementTutorialSecondStepCoroutine();
        }
        
        private IEnumerator MovementTutorialSecondStepCoroutine()
        {
            m_Hsm.ShowMoveUpPrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.GetMoveCommands(), GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveUp, GetGroupName());
            while (!m_ReadyToThirdMovementStep)
                yield return null;
            yield return MovementTutorialThirdStepCoroutine();
        }

        private IEnumerator MovementTutorialThirdStepCoroutine()
        {
            m_Hsm.ShowMoveLeftPrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.GetMoveCommands(), GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveLeft, GetGroupName());
            while (!m_ReadyToFourthMovementStep)
                yield return null;
            yield return MovementTutorialFourthStepCoroutine();
        }

        private IEnumerator MovementTutorialFourthStepCoroutine()
        {
            m_Hsm.ShowMoveDownPrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.GetMoveCommands(), GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveDown, GetGroupName());
            while (!m_ReadyToFinishMovementTutorial)
                yield return null;
            FinishMovementTutorial();
        }

        private void FinishMovementTutorial()
        {
            m_Hsm.HidePrompt();
            CommandsProceeder.UnlockCommands(RazorMazeUtils.GetMoveCommands(), GetGroupName());
            m_MovementTutorialFinished = true;
            SaveUtils.PutValue(SaveKeys.MovementTutorialFinished, true);
            TutorialFinished?.Invoke(ETutorialType.Movement);
        }
        
        private IEnumerator RotationTutorialFirstStepCoroutine()
        {
            m_Hsr.ShowRotateCounterClockwisePrompt();
            CommandsProceeder.UnlockCommands(RazorMazeUtils.GetRotateCommands(), "all");
            CommandsProceeder.LockCommands(RazorMazeUtils.GetRotateCommands(), GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.RotateCounterClockwise, GetGroupName());
            while (!m_ReadyToSecondRotationStep)
                yield return null;
            yield return RotationTutorialSecondStepCoroutine();
        }

        private IEnumerator RotationTutorialSecondStepCoroutine()
        {
            m_Hsr.ShowRotateClockwisePrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.GetRotateCommands(), GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.RotateClockwise, GetGroupName());
            while (!m_ReadyToFinishRotationTutorial)
                yield return null;
            yield return RotationTutorialThirdStepCoroutine();
        }

        private IEnumerator RotationTutorialThirdStepCoroutine()
        {
            m_Hsr.HidePrompt();
            m_RotPossIndicator = RotPossIndFactory.Create();
            m_RotPossIndicator.Shape.Color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
            m_RotPossIndicator.Animator.SetTrigger(AnimKeys.Anim);
            var cont = ContainersGetter.GetContainer(ContainerNames.Tutorial);
            var goRotPossText = PrefabSetManager.InitPrefab(
                cont, "tutorials", "rotation_possibility_text");
            var screenBounds = GraphicUtils.GetVisibleBounds();
            goRotPossText.transform.SetPosXY(
                screenBounds.center.x,
                screenBounds.min.y + 5f);
            m_RotPossText = goRotPossText.GetCompItem<TextMeshPro>("text");
            m_RotPossText.rectTransform.sizeDelta = m_RotPossText.rectTransform.sizeDelta.SetX(
                screenBounds.max.x - screenBounds.min.x - 3f);
            m_RotPossText.color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
            m_RotPossTextAnim = goRotPossText.GetCompItem<Animator>("animator");
            LocalizationManager.AddTextObject(m_RotPossText, "rotation_possibility_text");
            bool readyToNextStage = false;
            m_RotPossIndicator.Triggerer.Trigger1 = () => readyToNextStage = true;
            m_RotPossTextAnim.SetTrigger(AnimKeys.Anim);
            m_RotPossIndicator.Animator.SetTrigger(AnimKeys.Anim2);
            while (!readyToNextStage)
                yield return null;
            m_RotPossTextAnim.SetTrigger(AnimKeys.Stop);
            m_RotPossIndicator.Animator.SetTrigger(AnimKeys.Stop);
            FinishRotationTutorial();
        }

        private void FinishRotationTutorial()
        {
            CommandsProceeder.UnlockCommands(RazorMazeUtils.GetMoveCommands(), GetGroupName());
            CommandsProceeder.UnlockCommands(RazorMazeUtils.GetRotateCommands(), GetGroupName());
            
            m_RotationTutorialFinished = true;
            SaveUtils.PutValue(SaveKeys.RotationTutorialFinished, true);
            TutorialFinished?.Invoke(ETutorialType.Rotation);
        }

        private Bounds GetScreenBounds()
        {
            return GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
        }

        private static string GetGroupName()
        {
            return nameof(IViewUITutorial);
        }

        #endregion
    }
}