using System.Collections;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.UI
{
    public enum ETutorialType
    {
        Movement,
        Rotation,
        Setting
    }
    
    public interface IViewUITutorial : IOnLevelStageChanged, IInitViewUIItem
    {
        event UnityAction<ETutorialType> TutorialStarted;
        event UnityAction<ETutorialType> TutorialFinished;
        ETutorialType?                   IsCurrentLevelTutorial();
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

        #endregion

        #region inject

        private CommonGameSettings            Settings            { get; }
        private IModelGame                    Model               { get; }
        private IPrefabSetManager             PrefabSetManager    { get; }
        private IContainersGetter             ContainersGetter    { get; }
        private IMazeCoordinateConverter      CoordinateConverter { get; }
        private IViewInputCommandsProceeder   CommandsProceeder   { get; }
        private ICameraProvider               CameraProvider      { get; }
        private IColorProvider                ColorProvider       { get; }
        private ILocalizationManager          LocalizationManager { get; }
        private IViewGameTicker               Ticker              { get; }
        private IRotatingPossibilityIndicator RotationIndicator   { get; }
        private ILevelsLoader                 LevelsLoader        { get; }
        private IViewUISubtitles              Subtitles           { get; }

        public ViewUITutorial(
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
            IViewUISubtitles              _Subtitles)
        {
            Settings            = _Settings;
            Model               = _Model;
            PrefabSetManager    = _PrefabSetManager;
            ContainersGetter    = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            CommandsProceeder   = _CommandsProceeder;
            CameraProvider      = _CameraProvider;
            ColorProvider       = _ColorProvider;
            LocalizationManager = _LocalizationManager;
            Ticker              = _Ticker;
            RotationIndicator   = _RotationIndicator;
            LevelsLoader        = _LevelsLoader;
            Subtitles           = _Subtitles;
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
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded)
                return;
            AdditionalCheckForTutorialFinishing(_Args);
            var tutType = IsCurrentLevelTutorial();
            if (!tutType.HasValue)
                return;
            switch (tutType.Value)
            {
                case ETutorialType.Movement: StartMovementTutorial(); break;
                // case ETutorialType.Rotation: StartRotationTutorial(); break;
                case ETutorialType.Setting:  StartSettingTutorial(); break;
            }
        }
        
        public ETutorialType? IsCurrentLevelTutorial()
        {
            return Model.Data.Info.AdditionalInfo.Comment1 switch
            {
                "movement tutorial" => ETutorialType.Movement,
                "rotation tutorial" => ETutorialType.Rotation,
                "setting tutorial"  => ETutorialType.Setting,
                _ => null
            };
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
                if (info.AdditionalInfo.Comment1 != "rotation tutorial")
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
            if (m_RotationTutorialStarted || m_RotationTutorialFinished)
                return;
            TutorialStarted?.Invoke(ETutorialType.Rotation);
            var cont = ContainersGetter.GetContainer(ContainerNames.Tutorial);
            var goRotPrompt = PrefabSetManager.InitPrefab(
                cont, "tutorials", "hand_swipe_rotation");
            goRotPrompt.transform.localScale = Vector3.one * 6f;
            m_Hsr = goRotPrompt.GetCompItem<HandSwipeRotation>("hsr");
            m_Hsr.Init(Ticker, CameraProvider, CoordinateConverter, ColorProvider, m_Offsets);
            CommandsProceeder.LockCommands(RmazorUtils.MoveCommands, GetGroupName());
            Cor.Run(RotationTutorialFirstStepCoroutine());
            m_RotationTutorialStarted = true;
        }

        private void StartSettingTutorial()
        {
            if (m_SettingTutorialStarted || m_SettingTutorialFinished)
                return;
            TutorialStarted?.Invoke(ETutorialType.Setting);
            Cor.Run(ShowSubtitleCoroutine(1, 6, () =>
            {
                Subtitles.HideSubtitle();
                m_SettingTutorialFinished = true;
            }));
            m_SettingTutorialStarted = true;
        }

        private IEnumerator ShowSubtitleCoroutine(int _ReplicaIndex, int _ReplicasCount, UnityAction _OnFinish)
        {
            while (!Subtitles.CanShowSubtitle)
                yield return null;
            string replic = LocalizationManager.GetTranslation($"char_replica_{_ReplicaIndex}");
            float showDuration = replic.Length * 0.3f;
            Subtitles.ShowSubtitle(replic, showDuration, "character");
            if (_ReplicaIndex >= _ReplicasCount)
            {
                _OnFinish?.Invoke();
                yield break;
            }
            yield return ShowSubtitleCoroutine(_ReplicaIndex + 1, _ReplicasCount, _OnFinish);
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

        #endregion
    }
}