// ReSharper disable ClassNeverInstantiated.Global
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
using DialogViewers;
using GameHelpers;
using Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Characters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.UI;

namespace RMAZOR.Views.Common
{
    public interface IViewLevelStageController : IOnLevelStageChanged, IInit
    {
        void RegisterProceeders(IEnumerable<IOnLevelStageChanged> _Proceeders);
        void OnAllPathProceed(V2Int _LastPath);
    }

    public class ViewLevelStageController : InitBase, IViewLevelStageController
    {
        #region nonpublic members
        
        private static AudioClipArgs AudioClipArgsLevelStart => 
            new AudioClipArgs("level_start", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsLevelComplete => 
            new AudioClipArgs("level_complete", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsMainTheme =>
            new AudioClipArgs("main_theme", EAudioClipType.Music, _Loop: true);

        private readonly List<IOnLevelStageChanged> m_Proceeders = new List<IOnLevelStageChanged>();

        private bool                m_NextLevelMustBeFirstInGroup;
        private bool                m_FirstTimeLevelLoaded;
        private List<IViewMazeItem> m_MazeItemsCached;

        #endregion

        #region inject

        private CommonGameSettings          GameSettings         { get; }
        private ViewSettings                ViewSettings         { get; }
        private IViewGameTicker             ViewGameTicker       { get; }
        private IModelGameTicker            ModelGameTicker      { get; }
        private IModelGame                  Model                { get; }
        private IManagersGetter             Managers             { get; }
        private IViewCharacter              Character            { get; }
        private IViewInputCommandsProceeder CommandsProceeder    { get; }
        private IContainersGetter           ContainersGetter     { get; }
        private IMazeShaker                 MazeShaker           { get; }
        private IDialogPanels               DialogPanels         { get; }
        private IProposalDialogViewer       ProposalDialogViewer { get; }
        private IViewMazePathItemsGroup     PathItemsGroup       { get; }

        public ViewLevelStageController(
            CommonGameSettings          _GameSettings,
            ViewSettings                _ViewSettings,
            IViewGameTicker             _ViewGameTicker,
            IModelGameTicker            _ModelGameTicker,
            IModelGame                  _Model,
            IManagersGetter             _Managers,
            IViewCharacter              _Character,
            IViewInputCommandsProceeder _CommandsProceeder,
            IContainersGetter           _ContainersGetter,
            IMazeShaker                 _MazeShaker,
            IDialogPanels               _DialogPanels,
            IProposalDialogViewer       _ProposalDialogViewer,
            IViewMazePathItemsGroup     _PathItemsGroup)
        {
            GameSettings         = _GameSettings;
            ViewSettings         = _ViewSettings;
            ViewGameTicker       = _ViewGameTicker;
            ModelGameTicker      = _ModelGameTicker;
            Model                = _Model;
            Managers             = _Managers;
            Character            = _Character;
            CommandsProceeder    = _CommandsProceeder;
            ContainersGetter     = _ContainersGetter;
            MazeShaker           = _MazeShaker;
            DialogPanels         = _DialogPanels;
            ProposalDialogViewer = _ProposalDialogViewer;
            PathItemsGroup       = _PathItemsGroup;
        }

        #endregion

        #region api

        public override void Init()
        {
            CommandsProceeder.Command += OnCommand;
            Managers.AudioManager.InitClip(AudioClipArgsLevelStart);
            Managers.AudioManager.InitClip(AudioClipArgsLevelComplete);
            base.Init();
        }

        public void RegisterProceeders(IEnumerable<IOnLevelStageChanged> _Proceeders)
        {
            m_Proceeders.Clear();
            m_Proceeders.AddRange(_Proceeders);
        }

        public void OnAllPathProceed(V2Int _LastPath)
        {
            if (CommonData.Release)
                Model.LevelStaging.FinishLevel();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            ProceedTime(_Args);
            foreach (var proceeder in m_Proceeders)
                proceeder.OnLevelStageChanged(_Args);
            ProceedMazeItemGroups(_Args);
            ProceedSounds(_Args);
        }

        #endregion

        #region nonpublic methods

        private void OnCommand(EInputCommand _Key, object[] _Args)
        {
            if (_Key != EInputCommand.ReadyToUnloadLevel)
                return;
            if (_Args == null || !_Args.Any())
                return;
            if(_Args[0].ToString() == CommonInputCommandArgs.LoadFirstLevelFromGroupArg)
                m_NextLevelMustBeFirstInGroup = true;
        }
        
        private void ProceedMazeItemGroups(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Paused:
                    return;
            }
            var mazeItems = _Args.Stage == ELevelStage.Loaded ? 
                m_MazeItemsCached = GetMazeAndPathItems(m_Proceeders) : m_MazeItemsCached;
            mazeItems.AddRange(PathItemsGroup.PathItems);
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:             OnLevelLoaded(_Args, mazeItems, PathItemsGroup); break;
                case ELevelStage.Finished:           OnLevelFinished(_Args);                          break;
                case ELevelStage.ReadyToUnloadLevel: OnReadyToUnloadLevel(_Args, mazeItems);          break;
                case ELevelStage.Unloaded:           OnLevelUnloaded(_Args);                          break;
                case ELevelStage.CharacterKilled:    OnCharacterKilled(mazeItems);                    break;
            }
        }

        private List<IViewMazeItem> GetMazeAndPathItems(IEnumerable<object> _Proceeders)
        {
            var mazeItems = new List<IViewMazeItem>();
            var mazeItemGroups = _Proceeders
                .Select(_P => _P as IViewMazeItemGroup)
                .Where(_P => _P != null);
            foreach (var group in mazeItemGroups)
                mazeItems.AddRange(group.GetActiveItems());
            mazeItems.AddRange(PathItemsGroup.PathItems);
            return mazeItems;
        }

        private void OnLevelLoaded(
            LevelStageArgs                     _Args,
            IReadOnlyCollection<IViewMazeItem> _MazeItems,
            IViewMazePathItemsGroup            _PathItemsGroup)
        {
            var levelArgs = new LevelArgs
            {
                FileName = nameof(DataFieldIds.Level),
                Level = _Args.LevelIndex
            };
            Managers.ScoreManager.SaveGameProgress(levelArgs, true);
            m_NextLevelMustBeFirstInGroup = false;
            Character.Appear(true);
            foreach (var pathItem in _PathItemsGroup.PathItems)
            {
                bool collect = Model.PathItemsProceeder.PathProceeds[pathItem.Props.Position];
                pathItem.Collected = collect;
            }
            foreach (var mazeItem in _MazeItems)
                mazeItem.Appear(true);
            Cor.Run(Cor.WaitWhile(() => 
            {
                return Character.AppearingState != EAppearingState.Appeared
                       || _MazeItems.Any(_Item => _Item.AppearingState != EAppearingState.Appeared);
            },
            () => Model.LevelStaging.ReadyToStartLevel()));
        }

        private void OnLevelFinished(LevelStageArgs _Args)
        {
            bool allLevelsPassed = SaveUtils.GetValue(SaveKeysRmazor.AllLevelsPassed);
            if (!allLevelsPassed && _Args.LevelIndex + 1 >= ViewSettings.levelsCountMain)
                SaveUtils.PutValue(SaveKeysRmazor.AllLevelsPassed, true);
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.LevelFinished, 
           new Dictionary<string, object>
           {
               {"level_index", _Args.LevelIndex},
               {"level_time", Model.LevelStaging.LevelTime},
               {"dies_count", Model.LevelStaging.DiesCount}
           });
        }

        private void OnReadyToUnloadLevel(LevelStageArgs _Args, IReadOnlyCollection<IViewMazeItem> _MazeItems)
        {
            if (_Args.LevelIndex >= GameSettings.firstLevelToShowAds
                && _Args.LevelIndex % GameSettings.showAdsEveryLevel == 0)
            {
                Managers.AdsManager.ShowInterstitialAd(null);
            }
            foreach (var mazeItem in _MazeItems)
                mazeItem.Appear(false);
            Cor.Run(Cor.WaitWhile(() =>
            {
                return _MazeItems.Any(_Item => _Item.AppearingState != EAppearingState.Dissapeared);
            },
            () =>
            {
                CommandsProceeder.RaiseCommand(EInputCommand.UnloadLevel, null, true);
            }));
        }

        private void OnLevelUnloaded(LevelStageArgs _Args)
        {
            var scoreEntity = Managers.ScoreManager.GetScoreFromLeaderboard(DataFieldIds.Level, false);
            Cor.Run(Cor.WaitWhile(
                () => scoreEntity.Result == EEntityResult.Pending,
                () =>
                {
                    switch (scoreEntity.Result)
                    {
                        case EEntityResult.Pending:
                            Dbg.LogWarning("Timeout when getting score from leaderboard");
                            return;
                        case EEntityResult.Fail:
                            Dbg.LogError("Failed to get score from leaderboard");
                            return;
                        case EEntityResult.Success:
                        {
                            var score = scoreEntity.GetFirstScore();
                            if (!score.HasValue)
                            {
                                Dbg.LogError("Failed to get score from leaderboard");
                                return;
                            }
                            Managers.ScoreManager.SetScoreToLeaderboard(
                                DataFieldIds.Level, 
                                score.Value + 1, 
                                false);
                            break;
                        }
                        default:
                            throw new SwitchCaseNotImplementedException(scoreEntity.Result);
                    }
                },
                _Seconds: 3f,
                _Ticker: ViewGameTicker));
            if (SaveUtils.GetValue(SaveKeysRmazor.AllLevelsPassed))
            {
                int group = RazorMazeUtils.GetGroupIndex(_Args.LevelIndex);
                int firstLevelInGroup = RazorMazeUtils.GetFirstLevelInGroup(group);
                int levelsInGroup = RazorMazeUtils.GetLevelsInGroup(group);
                bool isLastLevelInGroup = _Args.LevelIndex == firstLevelInGroup + levelsInGroup - 1;
                if (isLastLevelInGroup)
                    CommandsProceeder.RaiseCommand(EInputCommand.LoadFirstLevelFromRandomGroup, null, true);
            }
            else if (m_NextLevelMustBeFirstInGroup)
                CommandsProceeder.RaiseCommand(EInputCommand.LoadFirstLevelFromCurrentGroup, null, true);
            else if (RazorMazeUtils.LoadNextLevelAutomatically)
                CommandsProceeder.RaiseCommand(EInputCommand.LoadNextLevel, null, true);
        }

        private void OnCharacterKilled(List<IViewMazeItem> _MazeItems)
        {
            MazeShaker.OnCharacterDeathAnimation(
                ContainersGetter.GetContainer(ContainerNames.Character).transform.position,
                _MazeItems,
                () =>
                {
                    if (!CommonData.Release)
                        return;
                    var gravityTraps = Model.GetAllProceedInfos()
                        .Where(_Info => _Info.Type == EMazeItemType.GravityTrap);
                    if (gravityTraps.Any(
                        _Info =>
                            _Info.CurrentPosition == Model.PathItemsProceeder.PathProceeds.First().Key))
                    {
                        CommandsProceeder.RaiseCommand(
                            EInputCommand.ReadyToUnloadLevel,
                            new object[] { CommonInputCommandArgs.LoadFirstLevelFromGroupArg }, 
                            true);
                    }
                    else
                    {
                        var panel = DialogPanels.CharacterDiedDialogPanel;
                        panel.LoadPanel();
                        ProposalDialogViewer.Show(panel);
                    }
                });
        }
        
        private void ProceedTime(LevelStageArgs _Args)
        {
            bool pause = _Args.Stage == ELevelStage.Paused;
            ViewGameTicker.Pause = pause;
            ModelGameTicker.Pause = pause;
        }

        private void ProceedSounds(LevelStageArgs _Args)
        {
            var audioManager = Managers.AudioManager;
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    break;
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused:
                    audioManager.PlayClip(AudioClipArgsLevelComplete);
                    break;
                case ELevelStage.Paused:
                    audioManager.MuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Loaded:
                    audioManager.PlayClip(AudioClipArgsLevelStart);
                    if (!m_FirstTimeLevelLoaded)
                    {
                        audioManager.PlayClip(AudioClipArgsMainTheme);
                        m_FirstTimeLevelLoaded = true;
                    }
                    audioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.ReadyToStart:
                    audioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Finished:
                    audioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        #endregion
    }
}