using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI.DialogViewers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI.Game_Logo;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.UI
{
    public enum ETutorialType
    {
        Movement,
        MazeItem
    }

    public interface IViewUITutorial : IOnLevelStageChanged, IInitViewUIItem
    {
        event UnityAction<ETutorialType> TutorialStarted;
        event UnityAction<ETutorialType> TutorialFinished;
    }

    public class ViewUITutorial : IViewUITutorial
    {
        #region nonpublic members

        private bool m_MovementTutorialStarted, m_MovementTutorialFinished;
        private bool m_SettingTutorialStarted,  m_SettingTutorialFinished;

        private bool m_ReadyToSecondMovementStep;
        private bool m_ReadyToThirdMovementStep;
        private bool m_ReadyToFourthMovementStep;
        private bool m_ReadyToFinishMovementTutorial;

        private Vector4           m_Offsets;
        private HandSwipeMovement m_Hsm;
        private HandSwipeRotation m_Hsr;
        private TextMeshPro       m_RotPossText;
        private Animator          m_RotPossTextAnim;
        private bool              m_TutorialPanelLoaded;

        #endregion

        #region inject

        private IModelGame                  Model                   { get; }
        private IPrefabSetManager           PrefabSetManager        { get; }
        private IContainersGetter           ContainersGetter        { get; }
        private ICoordinateConverter        CoordinateConverter     { get; }
        private IViewInputCommandsProceeder CommandsProceeder       { get; }
        private ICameraProvider             CameraProvider          { get; }
        private IColorProvider              ColorProvider           { get; }
        private IViewGameTicker             Ticker                  { get; }
        private IDialogViewersController    DialogViewersController { get; }
        private ITutorialDialogPanel        TutorialDialogPanel     { get; }
        private IViewUIGameLogo             GameLogo                { get; }

        private ViewUITutorial(
            IModelGame                  _Model,
            IPrefabSetManager           _PrefabSetManager,
            IContainersGetter           _ContainersGetter,
            ICoordinateConverter        _CoordinateConverter,
            IViewInputCommandsProceeder _CommandsProceeder,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewGameTicker             _Ticker,
            IDialogViewersController    _DialogViewersController,
            ITutorialDialogPanel        _TutorialDialogPanel,
            IViewUIGameLogo             _GameLogo)
        {
            Model                   = _Model;
            PrefabSetManager        = _PrefabSetManager;
            ContainersGetter        = _ContainersGetter;
            CoordinateConverter     = _CoordinateConverter;
            CommandsProceeder       = _CommandsProceeder;
            CameraProvider          = _CameraProvider;
            ColorProvider           = _ColorProvider;
            Ticker                  = _Ticker;
            DialogViewersController = _DialogViewersController;
            TutorialDialogPanel     = _TutorialDialogPanel;
            GameLogo                = _GameLogo;
        }

        #endregion

        #region api

        public event UnityAction<ETutorialType> TutorialStarted;
        public event UnityAction<ETutorialType> TutorialFinished;

        public void Init(Vector4 _Offsets)
        {
            m_Offsets = _Offsets;
            var movementTutorialFinishedSaveKey = SaveKeysRmazor.IsTutorialFinished("movement");
            m_MovementTutorialFinished = SaveUtils.GetValue(movementTutorialFinishedSaveKey);
            CommandsProceeder.Command += OnCommand;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Loaded:
                    string tutorialName = CurrentLevelTutorialName();
                    if (string.IsNullOrEmpty(tutorialName))
                        return;
                    if (SaveUtils.GetValue(SaveKeysRmazor.IsTutorialFinished(tutorialName)))
                        return;
                    if (tutorialName == "timer")
                        return;
                    Cor.Run(Cor.WaitWhile(() => !GameLogo.WasShown,
                        () =>
                        {
                            if (tutorialName == "movement")
                                StartMovementTutorial();
                            if (!GetValidSystemLanguages().Contains(Application.systemLanguage))
                                return;
                            ShowTutorialPanel(tutorialName);
                        }));
                    break;
            }
        }

        #endregion

        #region nonpublic methods

        private void OnCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            if (m_MovementTutorialStarted && !m_MovementTutorialFinished)
                OnMovementCommand(_Command);
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
        
        private string CurrentLevelTutorialName()
        {
            var args = Model.Data.Info.AdditionalInfo.Arguments.Split(';');
            return (from arg in args where arg.Contains("tutorial")
                    select arg.Split(':')[1]).FirstOrDefault();
        }

        private void StartMovementTutorial()
        {
            if (m_MovementTutorialStarted || m_MovementTutorialFinished)
                return;
            TutorialStarted?.Invoke(ETutorialType.Movement);
            var cont = ContainersGetter.GetContainer(ContainerNamesMazor.Tutorial);
            var goMovePrompt = PrefabSetManager.InitPrefab(
                cont, "tutorials", "hand_swipe_movement");
            goMovePrompt.transform.localScale = Vector3.one * 6f;
            m_Hsm = goMovePrompt.GetCompItem<HandSwipeMovement>("hsm");
            m_Hsm.Init(Ticker, CameraProvider, CoordinateConverter, ColorProvider, m_Offsets);
            Cor.Run(MovementTutorialFirstStepCoroutine());
            m_MovementTutorialStarted = true;
        }

        private void ShowTutorialPanel(string _TutorialName)
        {
            if (SaveUtils.GetValue(SaveKeysRmazor.IsTutorialFinished(_TutorialName)))
                return;
            var info = new TutorialDialogPanelInfo(
                _TutorialName,
                "tut_title_" + _TutorialName,
                "tut_descr_" + _TutorialName,
                "tutorial_clip_" + _TutorialName
            );
            TutorialDialogPanel.SetPanelInfo(info);
            TutorialDialogPanel.PrepareVideo();
            var dv = DialogViewersController.GetViewer(TutorialDialogPanel.DialogViewerId);
            if (!m_TutorialPanelLoaded)
            {
                TutorialDialogPanel.LoadPanel(dv.Container, dv.Back);
                m_TutorialPanelLoaded = true;
            }
            TutorialDialogPanel.PrepareVideo();
            Cor.Run(Cor.WaitWhile(() => !TutorialDialogPanel.IsVideoReady,
                () =>
                {
                    CommandsProceeder.RaiseCommand(
                        EInputCommand.TutorialPanel, 
                        null, 
                        true);
                }));
        }

        private IEnumerator MovementTutorialFirstStepCoroutine()
        {
            m_Hsm.ShowMoveRightPrompt();
            CommandsProceeder.LockCommands(RmazorUtils.MoveCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveRight, GetGroupName());
            while (!m_ReadyToSecondMovementStep)
                yield return null;
            yield return MovementTutorialSecondStepCoroutine();
        }

        private IEnumerator MovementTutorialSecondStepCoroutine()
        {
            m_Hsm.ShowMoveUpPrompt();
            CommandsProceeder.LockCommands(RmazorUtils.MoveCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveUp, GetGroupName());
            while (!m_ReadyToThirdMovementStep)
                yield return null;
            yield return MovementTutorialThirdStepCoroutine();
        }

        private IEnumerator MovementTutorialThirdStepCoroutine()
        {
            m_Hsm.ShowMoveLeftPrompt();
            CommandsProceeder.LockCommands(RmazorUtils.MoveCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveLeft, GetGroupName());
            while (!m_ReadyToFourthMovementStep)
                yield return null;
            yield return MovementTutorialFourthStepCoroutine();
        }

        private IEnumerator MovementTutorialFourthStepCoroutine()
        {
            m_Hsm.ShowMoveDownPrompt();
            CommandsProceeder.LockCommands(RmazorUtils.MoveCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.MoveDown, GetGroupName());
            while (!m_ReadyToFinishMovementTutorial)
                yield return null;
            FinishMovementTutorial();
        }

        private void FinishMovementTutorial()
        {
            m_Hsm.HidePrompt();
            CommandsProceeder.UnlockCommands(RmazorUtils.MoveCommands, GetGroupName());
            m_MovementTutorialFinished = true;
            var saveKeyMovementTutorialFinished = SaveKeysRmazor.IsTutorialFinished("movement");
            SaveUtils.PutValue(saveKeyMovementTutorialFinished, true);
            TutorialFinished?.Invoke(ETutorialType.Movement);
        }

        private static string GetGroupName()
        {
            return nameof(IViewUITutorial);
        }

        private static IEnumerable<SystemLanguage> GetValidSystemLanguages()
        {
            return new[]
            {
                SystemLanguage.English,
                SystemLanguage.Russian,
                SystemLanguage.Belarusian,
                SystemLanguage.Ukrainian,
                SystemLanguage.German,
                SystemLanguage.Spanish,
                SystemLanguage.Portuguese,
                SystemLanguage.Japanese,
                SystemLanguage.Korean,
            };
        }

        #endregion
    }
}