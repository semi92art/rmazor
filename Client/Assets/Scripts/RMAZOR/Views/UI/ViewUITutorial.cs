using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
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
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.UI.Panels;
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
        Rotation,
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
        private bool m_RotationTutorialStarted, m_RotationTutorialFinished;
        private bool m_SettingTutorialStarted,  m_SettingTutorialFinished;

        private bool m_ReadyToSecondMovementStep;
        private bool m_ReadyToThirdMovementStep;
        private bool m_ReadyToFourthMovementStep;
        private bool m_ReadyToFinishMovementTutorial;
        private bool m_ReadyToSecondRotationStep;
        private bool m_ReadyToFinishRotationTutorial;

        private Vector4           m_Offsets;
        private HandSwipeMovement m_Hsm;
        private HandSwipeRotation m_Hsr;
        private TextMeshPro       m_RotPossText;
        private Animator          m_RotPossTextAnim;
        private int               m_LastTutorialLevelIndex = -1;
        private bool              m_GameLogoWasShown;

        #endregion

        #region inject

        private CommonGameSettings            Settings             { get; }
        private IModelGame                    Model                { get; }
        private IPrefabSetManager             PrefabSetManager     { get; }
        private IContainersGetter             ContainersGetter     { get; }
        private IMazeCoordinateConverter      CoordinateConverter  { get; }
        private IViewInputCommandsProceeder   CommandsProceeder    { get; }
        private ICameraProvider               CameraProvider       { get; }
        private IColorProvider                ColorProvider        { get; }
        private ILocalizationManager          LocalizationManager  { get; }
        private IViewGameTicker               Ticker               { get; }
        private IRotatingPossibilityIndicator RotationIndicator    { get; }
        private ILevelsLoader                 LevelsLoader         { get; }
        private IProposalDialogViewer         ProposalDialogViewer { get; }
        private ITutorialDialogPanel          TutorialDialogPanel  { get; }
        private IViewUIGameLogo               GameLogo             { get; }

        private ViewUITutorial(
            CommonGameSettings            _Settings,
            IModelGame                    _Model,
            IPrefabSetManager             _PrefabSetManager,
            IContainersGetter             _ContainersGetter,
            IMazeCoordinateConverter      _CoordinateConverter,
            IViewInputCommandsProceeder   _CommandsProceeder,
            ICameraProvider               _CameraProvider,
            IColorProvider                _ColorProvider,
            ILocalizationManager          _LocalizationManager,
            IViewGameTicker               _Ticker,
            IRotatingPossibilityIndicator _RotationIndicator,
            ILevelsLoader                 _LevelsLoader,
            IProposalDialogViewer         _ProposalDialogViewer,
            ITutorialDialogPanel          _TutorialDialogPanel,
            IViewUIGameLogo               _GameLogo)
        {
            Settings = _Settings;
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
            LevelsLoader = _LevelsLoader;
            ProposalDialogViewer = _ProposalDialogViewer;
            TutorialDialogPanel = _TutorialDialogPanel;
            GameLogo = _GameLogo;
        }

        #endregion

        #region api

        public event UnityAction<ETutorialType> TutorialStarted;
        public event UnityAction<ETutorialType> TutorialFinished;

        public void Init(Vector4 _Offsets)
        {
            GetLastTutorialLevelIndex();
            m_Offsets = _Offsets;
            m_MovementTutorialFinished = SaveUtils.GetValue(SaveKeysRmazor.MovementTutorialFinished);
            m_RotationTutorialFinished = SaveUtils.GetValue(SaveKeysRmazor.RotationTutorialFinished);
            CommandsProceeder.Command += OnCommand;
            GameLogo.Shown += OnGameLogoShown;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            AdditionalCheckForTutorialFinishing(_Args);
            var tutType = IsCurrentLevelTutorial(out var mazeItemType);
            if (!tutType.HasValue)
                return;
            switch (tutType.Value)
            {
                case ETutorialType.Movement:
                    StartMovementTutorial();
                    break;
                case ETutorialType.Rotation:
                    StartRotationTutorial();
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
                    "rotation" => ETutorialType.Rotation,
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
        
        private void OnGameLogoShown()
        {
            m_GameLogoWasShown = true;
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

        private void GetLastTutorialLevelIndex()
        {
            int? lastIdx = SaveUtils.GetValue(SaveKeysRmazor.LastTutorialLevelIndex);
            if (lastIdx.HasValue)
            {
                m_LastTutorialLevelIndex = lastIdx.Value;
                return;
            }

            int levelsCount = LevelsLoader.GetLevelsCount(Settings.gameId);
            for (int i = 0; i < levelsCount; i++)
            {
                var info = LevelsLoader.LoadLevel(Settings.gameId, i);
                if (info.AdditionalInfo.Arguments != "rotation tutorial")
                    continue;
                lastIdx = i;
                break;
            }

            if (lastIdx.HasValue)
                m_LastTutorialLevelIndex = lastIdx.Value;
        }

        private void AdditionalCheckForTutorialFinishing(LevelStageArgs _Args)
        {
            if (m_LastTutorialLevelIndex == -1 || _Args.LevelIndex <= m_LastTutorialLevelIndex)
                return;
            Dbg.Log("Finish all tutorials");
            SaveUtils.PutValue(SaveKeysRmazor.MovementTutorialFinished, true);
            SaveUtils.PutValue(SaveKeysRmazor.RotationTutorialFinished, true);
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
            // TODO
            // if (m_RotationTutorialStarted || m_RotationTutorialFinished)
            //     return;
            // TutorialStarted?.Invoke(ETutorialType.Rotation);
            // var cont = ContainersGetter.GetContainer(ContainerNames.Tutorial);
            // var goRotPrompt = PrefabSetManager.InitPrefab(
            //     cont, "tutorials", "hand_swipe_rotation");
            // goRotPrompt.transform.localScale = Vector3.one * 6f;
            // m_Hsr = goRotPrompt.GetCompItem<HandSwipeRotation>("hsr");
            // m_Hsr.Init(Ticker, CameraProvider, CoordinateConverter, ColorProvider, m_Offsets);
            // CommandsProceeder.LockCommands(RmazorUtils.MoveCommands, GetGroupName());
            // Cor.Run(RotationTutorialFirstStepCoroutine());
            // m_RotationTutorialStarted = true;
        }

        private void StartMazeItemTutorial(EMazeItemType _MazeItemType)
        {
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

        private IEnumerator RotationTutorialFirstStepCoroutine()
        {
            m_Hsr.ShowRotateCounterClockwisePrompt();
            CommandsProceeder.UnlockCommands(RmazorUtils.RotateCommands, "all");
            CommandsProceeder.LockCommands(RmazorUtils.MoveAndRotateCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.RotateCounterClockwise, GetGroupName());
            while (!m_ReadyToSecondRotationStep)
                yield return null;
            yield return RotationTutorialSecondStepCoroutine();
        }

        private IEnumerator RotationTutorialSecondStepCoroutine()
        {
            m_Hsr.ShowRotateClockwisePrompt();
            CommandsProceeder.LockCommands(RmazorUtils.RotateCommands, GetGroupName());
            CommandsProceeder.UnlockCommand(EInputCommand.RotateClockwise, GetGroupName());
            while (!m_ReadyToFinishRotationTutorial)
                yield return null;
            yield return RotationTutorialThirdStepCoroutine();
        }

        private IEnumerator RotationTutorialThirdStepCoroutine()
        {
            CommandsProceeder.UnlockCommands(RmazorUtils.MoveCommands, GetGroupName());
            m_Hsr.HidePrompt();
            RotationIndicator.Name = "Rotating Indicator Tutorial";
            var screenBounds = GraphicUtils.GetVisibleBounds();
            RotationIndicator.Init(m_Offsets);
            RotationIndicator.SetPosition(new Vector2(screenBounds.center.x, screenBounds.min.y + 10f));
            RotationIndicator.Shape.Color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
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
            m_RotPossText.color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
            m_RotPossTextAnim = goRotPossText.GetCompItem<Animator>("animator");
            LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(
                m_RotPossText, ETextType.GameUI, "rotation_possibility_text"));
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
            CommandsProceeder.UnlockCommands(RmazorUtils.MoveAndRotateCommands, GetGroupName());
            m_RotationTutorialFinished = true;
            SaveUtils.PutValue(SaveKeysRmazor.RotationTutorialFinished, true);
            TutorialFinished?.Invoke(ETutorialType.Rotation);
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