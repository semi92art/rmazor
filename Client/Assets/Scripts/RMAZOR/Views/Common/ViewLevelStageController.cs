// ReSharper disable ClassNeverInstantiated.Global
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers.Achievements;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.UI.Panels;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.UI.Game_Logo;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewLevelStageController : IOnLevelStageChanged, IInit
    {
        void RegisterProceeders(List<IOnLevelStageChanged> _Proceeder, int _ExecuteOrder);
        void OnAllPathProceed(V2Int                        _LastPath);
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

        private readonly SortedDictionary<int, List<IOnLevelStageChanged>> m_ProceedersToExecuteBeforeGroups      
            = new SortedDictionary<int, List<IOnLevelStageChanged>>();
        private readonly SortedDictionary<int, List<IOnLevelStageChanged>> m_ProceedersToExecuteAfterGroups      
            = new SortedDictionary<int, List<IOnLevelStageChanged>>();

        private bool                m_NextLevelMustBeFirstInGroup;
        private bool                m_FirstTimeLevelLoaded;
        private List<IViewMazeItem> m_MazeItemsCached;
        private bool                m_StartLogoShowing = true;

        #endregion

        #region inject

        private CommonGameSettings            GameSettings             { get; }
        private ViewSettings                  ViewSettings             { get; }
        private IViewGameTicker               ViewGameTicker           { get; }
        private IModelGameTicker              ModelGameTicker          { get; }
        private IModelGame                    Model                    { get; }
        private IManagersGetter               Managers                 { get; }
        private IViewCharacter                Character                { get; }
        private IViewInputCommandsProceeder   CommandsProceeder        { get; }
        private IContainersGetter             ContainersGetter         { get; }
        private IMazeShaker                   MazeShaker               { get; }
        private IDialogPanelsSet              DialogPanelsSet          { get; }
        private IProposalDialogViewer         ProposalDialogViewer     { get; }
        private IViewMazeItemsGroupSet        MazeItemsGroupSet        { get; }
        private IViewMazePathItemsGroup       PathItemsGroup           { get; }
        private CompanyLogo                   CompanyLogo              { get; }
        private IViewUIGameLogo               GameLogo                 { get; }
        private IRateGameDialogPanel          RateGameDialogPanel      { get; }
        private IViewUILevelSkipper           LevelSkipper             { get; }
        private IViewBetweenLevelTransitioner BetweenLevelTransitioner { get; }

        private ViewLevelStageController(
            CommonGameSettings            _GameSettings,
            ViewSettings                  _ViewSettings,
            IViewGameTicker               _ViewGameTicker,
            IModelGameTicker              _ModelGameTicker,
            IModelGame                    _Model,
            IManagersGetter               _Managers,
            IViewCharacter                _Character,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IContainersGetter             _ContainersGetter,
            IMazeShaker                   _MazeShaker,
            IDialogPanelsSet              _DialogPanelsSet,
            IProposalDialogViewer         _ProposalDialogViewer,
            IViewMazeItemsGroupSet        _MazeItemsGroupSet,
            IViewMazePathItemsGroup       _PathItemsGroup,
            CompanyLogo                   _CompanyLogo,
            IViewUIGameLogo               _GameLogo,
            IRateGameDialogPanel          _RateGameDialogPanel,
            IViewUILevelSkipper           _LevelSkipper,
            IViewBetweenLevelTransitioner _BetweenLevelTransitioner)
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
            DialogPanelsSet      = _DialogPanelsSet;
            ProposalDialogViewer = _ProposalDialogViewer;
            MazeItemsGroupSet    = _MazeItemsGroupSet;
            PathItemsGroup       = _PathItemsGroup;
            CompanyLogo          = _CompanyLogo;
            GameLogo             = _GameLogo;
            RateGameDialogPanel  = _RateGameDialogPanel;
            LevelSkipper         = _LevelSkipper;
            BetweenLevelTransitioner = _BetweenLevelTransitioner;
        }

        #endregion

        #region api

        public override void Init()
        {
            CommandsProceeder.Command += OnCommand;
            BetweenLevelTransitioner.TransitionFinished += OnBetweenLevelTransitionFinished;
            Managers.AudioManager.InitClip(AudioClipArgsLevelStart);
            Managers.AudioManager.InitClip(AudioClipArgsLevelComplete);
            base.Init();
        }

        public void RegisterProceeders(List<IOnLevelStageChanged> _Proceeders, int _ExecuteOrder)
        {
            var dict = _ExecuteOrder < 0 ?
                m_ProceedersToExecuteBeforeGroups : m_ProceedersToExecuteAfterGroups;
            if (!dict.ContainsKey(_ExecuteOrder))
                dict.Add(_ExecuteOrder, _Proceeders);
            else 
                dict[_ExecuteOrder] = _Proceeders;
        }

        public void OnAllPathProceed(V2Int _LastPath)
        {
            if (CommonData.Release)
                Model.LevelStaging.FinishLevel();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            PauseTimers(_Args.LevelStage == ELevelStage.Paused);
            foreach (var proceeder in m_ProceedersToExecuteBeforeGroups)
                proceeder.Value.ForEach(_P => _P.OnLevelStageChanged(_Args));
            MazeItemsGroupSet.OnLevelStageChanged(_Args);
            foreach (var proceeder in m_ProceedersToExecuteAfterGroups)
                proceeder.Value.ForEach(_P => _P.OnLevelStageChanged(_Args));
            ProceedMazeItemGroups(_Args);
            ProceedSounds(_Args);
        }

        #endregion

        #region nonpublic methods
        
        private void OnBetweenLevelTransitionFinished(bool _Appear)
        {
            if (!_Appear)
                CommandsProceeder.RaiseCommand(EInputCommand.ReadyToStartLevel, null, true);
        }

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
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Paused:
                    return;
                case ELevelStage.Loaded:
                case ELevelStage.Finished:
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.LevelStage);
            }
            var mazeItems = _Args.LevelStage == ELevelStage.Loaded ? 
                m_MazeItemsCached = GetMazeAndPathItems() : m_MazeItemsCached;
            mazeItems.AddRange(PathItemsGroup.PathItems);
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:             OnLevelLoaded(_Args, mazeItems);        break;
                case ELevelStage.Finished:           OnLevelFinished(_Args);                 break;
                case ELevelStage.ReadyToUnloadLevel: OnReadyToUnloadLevel(_Args, mazeItems); break;
                case ELevelStage.Unloaded:           OnLevelUnloaded(_Args);                 break;
                case ELevelStage.CharacterKilled:    OnCharacterKilled(mazeItems);           break;
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Paused:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.LevelStage);
            }
        }

        private List<IViewMazeItem> GetMazeAndPathItems()
        {
            var mazeItems = new List<IViewMazeItem>();
            var mazeItemGroups = MazeItemsGroupSet.GetGroups()
                .Where(_P => _P != null);
            foreach (var group in mazeItemGroups)
                mazeItems.AddRange(group.GetActiveItems());
            mazeItems.AddRange(PathItemsGroup.PathItems);
            return mazeItems;
        }

        private void OnLevelLoaded(
            LevelStageArgs                     _Args,
            IReadOnlyCollection<IViewMazeItem> _MazeItems)
        {
            var savedGameEntity = Managers.ScoreManager.
                GetSavedGameProgress(CommonData.SavedGameFileName, true);
            Cor.Run(Cor.WaitWhile(
                () => savedGameEntity.Result == EEntityResult.Pending,
                () =>
                {
                    bool castSuccess = savedGameEntity.Value.CastTo(out SavedGame savedGame);
                    if (savedGameEntity.Result == EEntityResult.Fail || !castSuccess)
                    {
                        Dbg.LogWarning("Failed to load saved game: " +
                                       $"_Result: {savedGameEntity.Result}, " +
                                       $"castSuccess: {castSuccess}, " +
                                       $"_Value: {savedGameEntity.Value}");
                        return;
                    }
                    var newSavedGame = new SavedGame
                    {
                        FileName = CommonData.SavedGameFileName,
                        Money = savedGame.Money,
                        Level = _Args.LevelIndex
                    };
                    Managers.ScoreManager.SaveGameProgress(
                        newSavedGame, false);
                }));
            if (m_StartLogoShowing)
            {
                CompanyLogo.HideLogo();
                GameLogo.Show();
                m_StartLogoShowing = false;
            }
            m_NextLevelMustBeFirstInGroup = false;
            Character.Appear(true);
            foreach (var pathItem in PathItemsGroup.PathItems)
            {
                bool collect = Model.PathItemsProceeder.PathProceeds[pathItem.Props.Position];
                pathItem.Collect(collect, true);
            }
            foreach (var mazeItem in _MazeItems)
                mazeItem.Appear(true);
        }

        private void OnLevelFinished(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Paused)
                return;
            bool allLevelsPassed = SaveUtils.GetValue(SaveKeysRmazor.AllLevelsPassed);
            if (!allLevelsPassed && _Args.LevelIndex + 1 >= ViewSettings.levelsCountMain)
                SaveUtils.PutValue(SaveKeysRmazor.AllLevelsPassed, true);
            
            if (PathItemsGroup.MoneyItemsCollectedCount <= 0)
                return;
            var savedGameEntity = Managers.ScoreManager.
                GetSavedGameProgress(CommonData.SavedGameFileName, true);
            Cor.Run(Cor.WaitWhile(
                () => savedGameEntity.Result == EEntityResult.Pending,
                () =>
                {
                    bool castSuccess = savedGameEntity.Value.CastTo(out SavedGame savedGame);
                    if (savedGameEntity.Result == EEntityResult.Fail || !castSuccess)
                    {
                        Dbg.LogWarning("Failed to load saved game: " +
                                       $"_Result: {savedGameEntity.Result}, " +
                                       $"castSuccess: {castSuccess}, " +
                                       $"_Value: {savedGameEntity.Value}");
                        return;
                    }
                    long newMoneyCount = savedGame.Money + PathItemsGroup.MoneyItemsCollectedCount;
                    var newSavedGame = new SavedGame
                    {
                        FileName = CommonData.SavedGameFileName,
                        Money = newMoneyCount,
                        Level = _Args.LevelIndex
                    };
                    SendLevelAnalyticEvent(_Args, newMoneyCount);
                    Managers.ScoreManager.SaveGameProgress(
                        newSavedGame, false);
                }));
            UnlockAchievementOnLevelFinishedIfKeyExist(_Args);
        }

        private void OnReadyToUnloadLevel(LevelStageArgs _Args, IReadOnlyCollection<IViewMazeItem> _MazeItems)
        {
            if (_Args.LevelIndex >= GameSettings.firstLevelToShowAds
                && _Args.LevelIndex % GameSettings.showAdsEveryLevel == 0
                && !LevelSkipper.LevelSkipped)
            {
                UnityAction<UnityAction, UnityAction, string, bool> act =  Managers.AdsManager.ShowInterstitialAd;
                if (GameSettings.showRewardedOnLevelPass)
                    act = Managers.AdsManager.ShowRewardedAd;
                act(null, null, null, false);
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
                            Dbg.Log("Level score from server leaderboard: " + score.Value);
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
                CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null, true);
                string panelText = Managers.LocalizationManager.GetTranslation("rate_game_text_all_levels_passed");
                RateGameDialogPanel.SetDialogTitle(panelText);
                RateGameDialogPanel.CanBeClosed = false;
            }
            else if (m_NextLevelMustBeFirstInGroup)
                CommandsProceeder.RaiseCommand(EInputCommand.LoadFirstLevelFromCurrentGroup, null, true);
            else if (CommonData.LoadNextLevelAutomatically)
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
                        var panel = DialogPanelsSet.CharacterDiedDialogPanel;
                        panel.LoadPanel();
                        ProposalDialogViewer.Show(panel, 3f);
                    }
                });
        }
        
        private void PauseTimers(bool _Pause)
        {
            ViewGameTicker.Pause = _Pause;
            ModelGameTicker.Pause = _Pause;
        }

        private void ProceedSounds(LevelStageArgs _Args)
        {
            var audioManager = Managers.AudioManager;
            switch (_Args.LevelStage)
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
                    Cor.Run(Cor.WaitWhile(
                        () => !GameLogo.WasShown,
                        () =>
                        {
                            audioManager.PlayClip(AudioClipArgsLevelStart);
                            if (!m_FirstTimeLevelLoaded)
                            {
                                audioManager.PlayClip(AudioClipArgsMainTheme);
                                m_FirstTimeLevelLoaded = true;
                            }
                            audioManager.UnmuteAudio(EAudioClipType.GameSound);
                        }));
                    break;
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Finished:
                    audioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.LevelStage);
            }
        }

        private void SendLevelAnalyticEvent(LevelStageArgs _Args, long _MoneyCount)
        {
            string analyticId = _Args.LevelStage switch
            {
                ELevelStage.ReadyToStart    => AnalyticIds.LevelReadyToStart,
                ELevelStage.StartedOrContinued when _Args.PreviousStage == ELevelStage.ReadyToStart &&
                                                    _Args.PrePreviousStage == ELevelStage.Loaded
                                            => AnalyticIds.LevelStarted,
                ELevelStage.CharacterKilled => AnalyticIds.CharacterDied,
                ELevelStage.Finished when _Args.PreviousStage == ELevelStage.StartedOrContinued 
                                            => AnalyticIds.LevelFinished,
                _                           => null
            };
            if (string.IsNullOrEmpty(analyticId))
                return;
            Managers.AnalyticsManager.SendAnalytic(analyticId, 
                new Dictionary<string, object>
                {
                    {"index", _Args.LevelIndex},
                    {"spent_time", Model.LevelStaging.LevelTime},
                    {"dies_count", Model.LevelStaging.DiesCount},
                    {"money_count", _MoneyCount}
                });
        }

        private void UnlockAchievementOnLevelFinishedIfKeyExist(LevelStageArgs _Args)
        {
            var dict = new Dictionary<long, ushort>
            {
                {10, AchievementKeys.Level10Finished},
                {50, AchievementKeys.Level50Finished},
                {100, AchievementKeys.Level100Finished},
                {200, AchievementKeys.Level200Finished},
                {300, AchievementKeys.Level300Finished},
                {400, AchievementKeys.Level400Finished},
                {500, AchievementKeys.Level500Finished},
                {600, AchievementKeys.Level600Finished},
                {700, AchievementKeys.Level700Finished},
                {800, AchievementKeys.Level800Finished},
                {900, AchievementKeys.Level900Finished},
                {1000, AchievementKeys.Level1000Finished},
            };
            ushort achievementKey = dict.GetSafe(_Args.LevelIndex + 1, out bool containsKey);
            if (!containsKey)
                return;
            Managers.ScoreManager.UnlockAchievement(achievementKey);
            
            var args = Model.Data.Info.AdditionalInfo.Arguments.Split(';');
            foreach (string arg in args)
            {
                if (!arg.Contains("achievement"))
                    continue;
                ushort id = ushort.Parse(arg.Split(':')[1]);
                Managers.ScoreManager.UnlockAchievement(id);
            }
        }
        
        #endregion
    }
}