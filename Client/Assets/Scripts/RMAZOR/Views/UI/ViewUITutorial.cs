using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.UI.Panels;
using RMAZOR.Views.CoordinateConverters;
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
        ETutorialType?                   IsCurrentLevelTutorial(out EMazeItemType? _MazeItemType);
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

        #endregion

        #region inject

        private IModelGame                  Model                { get; }
        private IPrefabSetManager           PrefabSetManager     { get; }
        private IContainersGetter           ContainersGetter     { get; }
        private ICoordinateConverterRmazor  CoordinateConverter  { get; }
        private IViewInputCommandsProceeder CommandsProceeder    { get; }
        private ICameraProvider             CameraProvider       { get; }
        private IColorProvider              ColorProvider        { get; }
        private IViewGameTicker             Ticker               { get; }
        private IProposalDialogViewer       ProposalDialogViewer { get; }
        private ITutorialDialogPanel        TutorialDialogPanel  { get; }
        private IViewUIGameLogo             GameLogo             { get; }

        private ViewUITutorial(
            IModelGame                  _Model,
            IPrefabSetManager           _PrefabSetManager,
            IContainersGetter           _ContainersGetter,
            ICoordinateConverterRmazor  _CoordinateConverter,
            IViewInputCommandsProceeder _CommandsProceeder,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewGameTicker             _Ticker,
            IProposalDialogViewer       _ProposalDialogViewer,
            ITutorialDialogPanel        _TutorialDialogPanel,
            IViewUIGameLogo             _GameLogo)
        {
            Model                = _Model;
            PrefabSetManager     = _PrefabSetManager;
            ContainersGetter     = _ContainersGetter;
            CoordinateConverter  = _CoordinateConverter;
            CommandsProceeder    = _CommandsProceeder;
            CameraProvider       = _CameraProvider;
            ColorProvider        = _ColorProvider;
            Ticker               = _Ticker;
            ProposalDialogViewer = _ProposalDialogViewer;
            TutorialDialogPanel  = _TutorialDialogPanel;
            GameLogo             = _GameLogo;
        }

        #endregion

        #region api

        public event UnityAction<ETutorialType> TutorialStarted;
        public event UnityAction<ETutorialType> TutorialFinished;

        public void Init(Vector4 _Offsets)
        {
            m_Offsets = _Offsets;
            m_MovementTutorialFinished = SaveUtils.GetValue(SaveKeysRmazor.MovementTutorialFinished);
            CommandsProceeder.Command += OnCommand;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            var tutType = IsCurrentLevelTutorial(out var mazeItemType);
            if (!tutType.HasValue)
                return;
            switch (tutType.Value)
            {
                case ETutorialType.Movement:
                    StartMovementTutorial();
                    break;
                case ETutorialType.MazeItem:
                    if (GameLogo.WasShown)
                    {
                        Cor.Run(Cor.WaitWhile(
                            () => Model.LevelStaging.LevelStage != ELevelStage.ReadyToStart,
                            () => StartMazeItemTutorial(mazeItemType!.Value)));
                    }
                    break;
                default: throw new SwitchCaseNotImplementedException(tutType.Value);
            }
        }

        public ETutorialType? IsCurrentLevelTutorial(out EMazeItemType? _MazeItemType)
        {
            _MazeItemType = null;
            var args = Model.Data.Info.AdditionalInfo.Arguments.Split(';');
            foreach (string arg in args)
            {
                if (!arg.Contains("tutorial"))
                    continue;
                string tutorialTypeRaw = arg.Split(':')[1];
                ETutorialType? tutorialType = tutorialTypeRaw switch
                {
                    "movement" => ETutorialType.Movement,
                    _          => ETutorialType.MazeItem
                };
                if (tutorialType != ETutorialType.MazeItem)
                    return tutorialType;
                var dict = GetMazeItemPrefabSubstringsDict();
                _MazeItemType = dict.First(
                    _Kvp => _Kvp.Value == tutorialTypeRaw).Key;
                return tutorialType;
            }

            return null;
        }

        #endregion

        #region nonpublic methods

        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (m_MovementTutorialStarted && !m_MovementTutorialFinished)
                OnMovementCommand(_Command);
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

        private void StartMazeItemTutorial(EMazeItemType _MazeItemType)
        {
            if (SaveUtils.GetValue(SaveKeysRmazor.GetMazeItemTutorialFinished(_MazeItemType)))
                return;
            var dict = GetMazeItemPrefabSubstringsDict();
            string mazeItemAssetSubstring = dict[_MazeItemType];
            var info = new TutorialDialogPanelInfo(
                "tut_descr_" + mazeItemAssetSubstring,
                "tutorial_clip_" + mazeItemAssetSubstring
            );
            TutorialDialogPanel.SetPanelInfo(info);
            TutorialDialogPanel.PrepareVideo();
            Cor.Run(Cor.WaitWhile(() => TutorialDialogPanel.IsVideoReady,
                () =>
                {
                    TutorialDialogPanel.LoadPanel();
                    ProposalDialogViewer.Show(TutorialDialogPanel, 3f);
                    SaveUtils.PutValue(SaveKeysRmazor.GetMazeItemTutorialFinished(_MazeItemType), true);
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
            SaveUtils.PutValue(SaveKeysRmazor.MovementTutorialFinished, true);
            TutorialFinished?.Invoke(ETutorialType.Movement);
        }

        private static string GetGroupName()
        {
            return nameof(IViewUITutorial);
        }

        private static Dictionary<EMazeItemType, string> GetMazeItemPrefabSubstringsDict()
        {
            return new Dictionary<EMazeItemType, string>
            {
                {EMazeItemType.Block,            null},
                {EMazeItemType.GravityBlock,     "gravity_block"},
                {EMazeItemType.ShredingerBlock,  "shredinger"},
                {EMazeItemType.Portal,           "portal"},
                {EMazeItemType.TrapReact,        "trap_react"},
                {EMazeItemType.TrapIncreasing,   "trap_increasing"},
                {EMazeItemType.TrapMoving,       "trap_moving"},
                {EMazeItemType.GravityTrap,      "gravity_trap"},
                {EMazeItemType.Turret,           "turret"},
                {EMazeItemType.GravityBlockFree, "gravity_block_free"},
                {EMazeItemType.Springboard,      "springboard"},
                {EMazeItemType.Hammer,           "hammer"},
                {EMazeItemType.Spear,            "spear"},
                {EMazeItemType.Diode,            "diode"},
            };
        }

        #endregion
    }
}