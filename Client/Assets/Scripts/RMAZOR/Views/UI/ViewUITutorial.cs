using System.Collections;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using GameHelpers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.UI
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
        private TextMeshPro                   m_RotPossText;
        private Animator                      m_RotPossTextAnim;

        #endregion

        #region inject

        private IModelGame                           Model               { get; }
        private IPrefabSetManager                    PrefabSetManager    { get; }
        private IContainersGetter                    ContainersGetter    { get; }
        private IMazeCoordinateConverter             CoordinateConverter { get; }
        private IViewInputCommandsProceeder          CommandsProceeder   { get; }
        private ICameraProvider                      CameraProvider      { get; }
        private IColorProvider                       ColorProvider       { get; }
        private ILocalizationManager                 LocalizationManager { get; }
        private IViewGameTicker                      Ticker              { get; }
        private IRotatingPossibilityIndicator        RotationIndicator   { get; }

        public ViewUITutorial(
            IModelGame                           _Model,
            IPrefabSetManager                    _PrefabSetManager,
            IContainersGetter                    _ContainersGetter,
            IMazeCoordinateConverter             _CoordinateConverter,
            IViewInputCommandsProceeder          _CommandsProceeder,
            ICameraProvider                      _CameraProvider,
            IColorProvider                       _ColorProvider,
            ILocalizationManager                 _LocalizationManager,
            IViewGameTicker                      _Ticker, 
            IRotatingPossibilityIndicator        _RotationIndicator)
        {
            Model = _Model;
            PrefabSetManager = _PrefabSetManager;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            CommandsProceeder = _CommandsProceeder;
            CameraProvider = _CameraProvider;
            ColorProvider = _ColorProvider;
            LocalizationManager = _LocalizationManager;
            Ticker = _Ticker;
            RotationIndicator = _RotationIndicator;
        }

        #endregion

        #region api
        
        public event UnityAction<ETutorialType> TutorialStarted;
        public event UnityAction<ETutorialType> TutorialFinished;
        
        public void Init(Vector4 _Offsets)
        {
            m_Offsets = _Offsets;
            m_MovementTutorialFinished = SaveUtils.GetValue(SaveKeysRmazor.MovementTutorialFinished);
            m_RotationTutorialFinished = SaveUtils.GetValue(SaveKeysRmazor.RotationTutorialFinished);
            CommandsProceeder.Command += OnCommand;
            ColorProvider.ColorChanged += OnColorChanged;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded)
                return;
            switch (Model.Data.Info.AdditionalInfo.Comment1)
            {
                case "movement tutorial": StartMovementTutorial(); break;
                case "rotation tutorial": StartRotationTutorial(); break;
            }
        }

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            
        }
        
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
                case EInputCommand.MoveRight: m_ReadyToSecondMovementStep     = true; break;
                case EInputCommand.MoveUp:    m_ReadyToThirdMovementStep      = true; break;
                case EInputCommand.MoveLeft:  m_ReadyToFourthMovementStep     = true; break;
                case EInputCommand.MoveDown:  m_ReadyToFinishMovementTutorial = true; break;
            }
        }

        private void OnRotationCommand(EInputCommand _Command)
        {
            switch (_Command)
            {
                case EInputCommand.RotateCounterClockwise: m_ReadyToSecondRotationStep     = true; break;
                case EInputCommand.RotateClockwise:        m_ReadyToFinishRotationTutorial = true; break;
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
            Cor.Run(MovementTutorialFirstStepCoroutine());
            m_MovementTutorialStarted = true;
        }

        private void StartRotationTutorial()
        {
            if (m_RotationTutorialStarted || m_RotationTutorialFinished)
                return;
            SaveUtils.PutValue(SaveKeysRmazor.EnableRotation, true);
            TutorialStarted?.Invoke(ETutorialType.Rotation);
            var cont = ContainersGetter.GetContainer(ContainerNames.Tutorial);
            var goRotPrompt = PrefabSetManager.InitPrefab(
                cont, "tutorials", "hand_swipe_rotation");
            goRotPrompt.transform.localScale = Vector3.one * 6f;
            m_Hsr = goRotPrompt.GetCompItem<HandSwipeRotation>("hsr");
            m_Hsr.Init(Ticker, CameraProvider, CoordinateConverter, ColorProvider, m_Offsets);
            CommandsProceeder.LockCommands(RazorMazeUtils.MoveCommands, GetGroupName());
            Cor.Run(RotationTutorialFirstStepCoroutine());
            m_RotationTutorialStarted = true;
        }

        
        private IEnumerator MovementTutorialFirstStepCoroutine()
        {
            m_Hsm.ShowMoveRightPrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.MoveCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveRight, GetGroupName());
            while (!m_ReadyToSecondMovementStep)
                yield return null;
            yield return MovementTutorialSecondStepCoroutine();
        }
        
        private IEnumerator MovementTutorialSecondStepCoroutine()
        {
            m_Hsm.ShowMoveUpPrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.MoveCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveUp, GetGroupName());
            while (!m_ReadyToThirdMovementStep)
                yield return null;
            yield return MovementTutorialThirdStepCoroutine();
        }

        private IEnumerator MovementTutorialThirdStepCoroutine()
        {
            m_Hsm.ShowMoveLeftPrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.MoveCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveLeft, GetGroupName());
            while (!m_ReadyToFourthMovementStep)
                yield return null;
            yield return MovementTutorialFourthStepCoroutine();
        }

        private IEnumerator MovementTutorialFourthStepCoroutine()
        {
            m_Hsm.ShowMoveDownPrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.MoveCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveDown, GetGroupName());
            while (!m_ReadyToFinishMovementTutorial)
                yield return null;
            FinishMovementTutorial();
        }

        private void FinishMovementTutorial()
        {
            m_Hsm.HidePrompt();
            CommandsProceeder.UnlockCommands(RazorMazeUtils.MoveCommands, GetGroupName());
            m_MovementTutorialFinished = true;
            SaveUtils.PutValue(SaveKeysRmazor.MovementTutorialFinished, true);
            TutorialFinished?.Invoke(ETutorialType.Movement);
        }
        
        private IEnumerator RotationTutorialFirstStepCoroutine()
        {
            m_Hsr.ShowRotateCounterClockwisePrompt();
            CommandsProceeder.UnlockCommands(RazorMazeUtils.RotateCommands, "all");
            CommandsProceeder.LockCommands(RazorMazeUtils.MoveAndRotateCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.RotateCounterClockwise, GetGroupName());
            while (!m_ReadyToSecondRotationStep)
                yield return null;
            yield return RotationTutorialSecondStepCoroutine();
        }

        private IEnumerator RotationTutorialSecondStepCoroutine()
        {
            m_Hsr.ShowRotateClockwisePrompt();
            CommandsProceeder.LockCommands(RazorMazeUtils.RotateCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.RotateClockwise, GetGroupName());
            while (!m_ReadyToFinishRotationTutorial)
                yield return null;
            yield return RotationTutorialThirdStepCoroutine();
        }

        private IEnumerator RotationTutorialThirdStepCoroutine()
        {
            CommandsProceeder.UnlockCommands(RazorMazeUtils.MoveCommands, GetGroupName());
            m_Hsr.HidePrompt();
            RotationIndicator.Name = "Rotating Indicator Tutorial";
            var screenBounds = GraphicUtils.GetVisibleBounds();
            RotationIndicator.Init(m_Offsets);
            RotationIndicator.SetPosition(new Vector2(screenBounds.center.x, screenBounds.min.y + 10f));
            RotationIndicator.Shape.Color = ColorProvider.GetColor(ColorIdsCommon.UI).SetA(0f);
            RotationIndicator.Animator.SetTrigger(AnimKeys.Anim);
            var cont = ContainersGetter.GetContainer(ContainerNames.Tutorial);
            var goRotPossText = PrefabSetManager.InitPrefab(
                cont, "tutorials", "rotation_possibility_text");
            goRotPossText.transform.SetPosXY(
                screenBounds.center.x,
                screenBounds.min.y + 5f);
            m_RotPossText = goRotPossText.GetCompItem<TextMeshPro>("text");
            m_RotPossText.rectTransform.sizeDelta = m_RotPossText.rectTransform.sizeDelta.SetX(
                screenBounds.max.x - screenBounds.min.x - 3f);
            m_RotPossText.color = ColorProvider.GetColor(ColorIdsCommon.UI).SetA(0f);
            m_RotPossTextAnim = goRotPossText.GetCompItem<Animator>("animator");
            LocalizationManager.AddTextObject(m_RotPossText, "rotation_possibility_text");
            bool readyToNextStage = false;
            RotationIndicator.Triggerer.Trigger1 = () => readyToNextStage = true;
            m_RotPossTextAnim.SetTrigger(AnimKeys.Anim);
            RotationIndicator.Animator.SetTrigger(AnimKeys.Anim2);
            while (!readyToNextStage)
                yield return null;
            m_RotPossTextAnim.SetTrigger(AnimKeys.Stop);
            RotationIndicator.Animator.SetTrigger(AnimKeys.Stop);
            FinishRotationTutorial();
        }

        private void FinishRotationTutorial()
        {
            CommandsProceeder.UnlockCommands(RazorMazeUtils.MoveAndRotateCommands, GetGroupName());
            m_RotationTutorialFinished = true;
            SaveUtils.PutValue(SaveKeysRmazor.RotationTutorialFinished, true);
            TutorialFinished?.Invoke(ETutorialType.Rotation);
        }

        private static string GetGroupName()
        {
            return nameof(IViewUITutorial);
        }

        #endregion
    }
}