// ReSharper disable ClassNeverInstantiated.Global
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers.Achievements;
using Common.Ticker;
using Common.UI.DialogViewers;
using Common.Utils;
using Lean.Touch;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.UI.Panels;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common.Additional_Background;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.UI.Game_Logo;

namespace RMAZOR.Views.Common
{
    public interface IViewLevelStageController : IOnLevelStageChanged, IInit, IOnPathCompleted
    {
        void RegisterProceeders(List<IOnLevelStageChanged> _Proceeder, int _ExecuteOrder);
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
        private bool                m_ShowRewardedOnUnload;

        #endregion

        #region inject

        private ViewSettings                        ViewSettings                   { get; }
        private IViewGameTicker                     ViewGameTicker                 { get; }
        private IModelGame                          Model                          { get; }
        private IManagersGetter                     Managers                       { get; }
        private IViewCharacter                      Character                      { get; }
        private IViewInputCommandsProceeder         CommandsProceeder              { get; }
        private IContainersGetter                   ContainersGetter               { get; }
        private IMazeShaker                         MazeShaker                     { get; }
        private IDialogPanelsSet                    DialogPanelsSet                { get; }
        private IDialogViewersController            DialogViewersController        { get; }
        private IViewMazeItemsGroupSet              MazeItemsGroupSet              { get; }
        private IViewMazePathItemsGroup             PathItemsGroup                 { get; }
        private CompanyLogo                         CompanyLogo                    { get; }
        private IViewUIGameLogo                     GameLogo                       { get; }
        private IViewFullscreenTransitioner         FullscreenTransitioner         { get; }
        private IViewBetweenLevelAdLoader           BetweenLevelAdLoader           { get; }
        private IViewCameraEffectsCustomAnimator    CameraEffectsCustomAnimator    { get; }
        private IViewMazeAdditionalBackgroundDrawer AdditionalBackgroundDrawer     { get; }
        private IViewInputTouchProceeder            TouchProceeder                 { get; }
        private IMoneyCounter                       MoneyCounter                   { get; }
        private ICameraProvider                     CameraProvider                 { get; }
        private ICoordinateConverter                CoordinateConverter            { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private ILevelsLoader                       LevelsLoader                   { get; }

        private ViewLevelStageController(
            ViewSettings                        _ViewSettings,
            IViewGameTicker                     _ViewGameTicker,
            IModelGame                          _Model,
            IManagersGetter                     _Managers,
            IViewCharacter                      _Character,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IContainersGetter                   _ContainersGetter,
            IMazeShaker                         _MazeShaker,
            IDialogPanelsSet                    _DialogPanelsSet,
            IDialogViewersController            _DialogViewersController,
            IViewMazeItemsGroupSet              _MazeItemsGroupSet,
            IViewMazePathItemsGroup             _PathItemsGroup,
            CompanyLogo                         _CompanyLogo,
            IViewUIGameLogo                     _GameLogo,
            IViewFullscreenTransitioner         _FullscreenTransitioner,
            IViewBetweenLevelAdLoader           _BetweenLevelAdLoader,
            IViewCameraEffectsCustomAnimator    _CameraEffectsCustomAnimator,
            IViewMazeAdditionalBackgroundDrawer _AdditionalBackgroundDrawer, 
            IViewInputTouchProceeder            _TouchProceeder,
            IMoneyCounter                       _MoneyCounter,
            ICameraProvider                     _CameraProvider,
            ICoordinateConverter                _CoordinateConverter,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            ILevelsLoader                       _LevelsLoader)
        {
            ViewSettings                = _ViewSettings;
            ViewGameTicker              = _ViewGameTicker;
            Model                       = _Model;
            Managers                    = _Managers;
            Character                   = _Character;
            CommandsProceeder           = _CommandsProceeder;
            ContainersGetter            = _ContainersGetter;
            MazeShaker                  = _MazeShaker;
            DialogPanelsSet             = _DialogPanelsSet;
            DialogViewersController     = _DialogViewersController;
            MazeItemsGroupSet           = _MazeItemsGroupSet;
            PathItemsGroup              = _PathItemsGroup;
            CompanyLogo                 = _CompanyLogo;
            GameLogo                    = _GameLogo;
            FullscreenTransitioner      = _FullscreenTransitioner;
            BetweenLevelAdLoader        = _BetweenLevelAdLoader;
            CameraEffectsCustomAnimator = _CameraEffectsCustomAnimator;
            AdditionalBackgroundDrawer  = _AdditionalBackgroundDrawer;
            CameraProvider              = _CameraProvider;
            CoordinateConverter         = _CoordinateConverter;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
            LevelsLoader                = _LevelsLoader;
            TouchProceeder              = _TouchProceeder;
            MoneyCounter                = _MoneyCounter;
        }

        #endregion

        #region api

        public override void Init()
        {
            MoneyCounter.Init();
            TouchProceeder.Tap += OnTapScreenAction;
            CameraProvider.Init();
            Cor.Run(Cor.WaitNextFrame(CameraEffectsCustomAnimator.Init));
            CommandsProceeder.Command += OnCommand;
            FullscreenTransitioner.TransitionFinished += OnBetweenLevelTransitionFinished;
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

        public void OnPathCompleted(V2Int _LastPath)
        {
            if (CommonData.Release)
                Model.LevelStaging.FinishLevel(Model.LevelStaging.Arguments);
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            SetCameraProviderProps(_Args);
            ProceedProceedersToExecuteBeforeMazeItemGroups(_Args);
            MazeItemsGroupSet.OnLevelStageChanged(_Args);
            ProceedProceedersToExecuteAfterMazeItemGroups(_Args);
            ProceedMazeItems(_Args);
            ProceedOther(_Args);
            ProceedSounds(_Args);
            SendLevelAnalyticEvent(_Args);
            CameraEffectsCustomAnimator.OnLevelStageChanged(_Args);
        }

        #endregion

        #region nonpublic methods

        private void OnTapScreenAction(LeanFinger _Finger)
        {
            if (_Finger.LastScreenPosition.y / GraphicUtils.ScreenSize.y > 0.9f)
                return;
            if (Model.LevelStaging.LevelStage != ELevelStage.Finished)
                return;
            var dv = DialogViewersController.GetViewer(EDialogViewerType.Medium1);
            var cp = dv.CurrentPanel;
            if (cp is IFinishLevelGroupDialogPanel)
                return;
            if (cp is IPlayBonusLevelDialogPanel)
                return;
            SwitchLevelStageCommandInvoker.SwitchLevelStage(
                EInputCommand.StartUnloadingLevel, false);
        }

        private void OnBetweenLevelTransitionFinished(bool _Appear)
        {
            if (_Appear)
                return;
            SwitchLevelStageCommandInvoker.SwitchLevelStage(
                EInputCommand.ReadyToStartLevel, true);
        }

        private void OnCommand(EInputCommand _Key, Dictionary<string, object> _Args)
        {
            if (_Key != EInputCommand.StartUnloadingLevel)
                return;
            if (_Args == null || !_Args.Any())
                return;
            var doLoadFirstLevelInGroup =
                _Args.GetSafe(CommonInputCommandArg.KeyLoadFirstLevelInGroup, out bool exist);
            if (!exist)
                return;
            if ((bool)doLoadFirstLevelInGroup)
                m_NextLevelMustBeFirstInGroup = true;
        }

        private void SetCameraProviderProps(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            CameraProvider.GetMazeBounds = CoordinateConverter.GetMazeBounds;
            CameraProvider.GetConverterScale = () => CoordinateConverter.Scale;
            CameraProvider.UpdateState();
        }

        private void ProceedProceedersToExecuteBeforeMazeItemGroups(LevelStageArgs _Args)
        {
            foreach (var proceeder in m_ProceedersToExecuteBeforeGroups)
                proceeder.Value.ForEach(_P => _P.OnLevelStageChanged(_Args));
        }
        
        private void ProceedProceedersToExecuteAfterMazeItemGroups(LevelStageArgs _Args)
        {
            foreach (var proceeder in m_ProceedersToExecuteAfterGroups)
                proceeder.Value.ForEach(_P => _P.OnLevelStageChanged(_Args));
        }
        
        private void ProceedMazeItems(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:             OnLevelLoaded(       _Args, GetMazeItems(_Args)); break;
                case ELevelStage.ReadyToUnloadLevel: OnReadyToUnloadLevel(_Args, GetMazeItems(_Args)); break;
                case ELevelStage.CharacterKilled:    OnCharacterKilled(   _Args, GetMazeItems(_Args)); break;
            }
        }

        private void ProceedOther(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Finished: OnLevelFinished(_Args); break;
                case ELevelStage.Unloaded: OnLevelUnloaded(_Args); break;
            }
        }

        private IReadOnlyCollection<IViewMazeItem> GetMazeItems(LevelStageArgs _Args)
        {
            var mazeItems = _Args.LevelStage == ELevelStage.Loaded ? 
                m_MazeItemsCached = GetMazeAndPathItems() : m_MazeItemsCached;
            mazeItems.AddRange(PathItemsGroup.PathItems);
            return mazeItems;
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
                    long levelIndex = _Args.LevelIndex;
                    string nextLevelType = (string) _Args.Args.GetSafe(CommonInputCommandArg.KeyNextLevelType, out _);
                    bool nextLevelTypeIsBonus = nextLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
                    if (nextLevelTypeIsBonus)
                    {
                        long bonusLevelIndex = Model.LevelStaging.LevelIndex;
                        levelIndex = RmazorUtils.GetFirstLevelInGroup((int) bonusLevelIndex + 1 + 1);
                    }
                    var newSavedGame = new SavedGame
                    {
                        FileName = CommonData.SavedGameFileName,
                        Money = savedGame.Money,
                        Level = levelIndex,
                        Args = _Args.Args
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
            FullscreenTransitioner.DoTextureTransition(false, ViewSettings.betweenLevelTransitionTime);
            CameraEffectsCustomAnimator.AnimateCameraEffectsOnBetweenLevelTransition(true);
        }

        private void OnLevelFinished(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Paused)
                return;
            
            UnlockAchievementOnLevelFinishedIfKeyExist(_Args);
            CheckForAllLevelsPassed(_Args);
            CheckForLevelGroupOrBonusLevelFinished(_Args);
        }
        
        private void OnReadyToUnloadLevel(
            LevelStageArgs                     _Args,
            IReadOnlyCollection<IViewMazeItem> _MazeItems)
        {
            void UnloadLevel()
            {
                CameraEffectsCustomAnimator.AnimateCameraEffectsOnBetweenLevelTransition(false);
                FullscreenTransitioner.DoTextureTransition(true, ViewSettings.betweenLevelTransitionTime);
                foreach (var mazeItem in _MazeItems)
                    mazeItem.Appear(false);
                AdditionalBackgroundDrawer.Appear(false);
                Cor.Run(Cor.WaitWhile(() =>
                {
                    return _MazeItems.Any(_Item => _Item.AppearingState != EAppearingState.Dissapeared);
                },
                () =>
                {
                    SwitchLevelStageCommandInvoker.SwitchLevelStage(
                        EInputCommand.UnloadLevel, true);
                }));
            }
            BetweenLevelAdLoader.TryShowAd(_Args.LevelIndex, UnloadLevel);
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
                // TODO сделать отдельное сообщение на окончание всех уровней
                var args = new Dictionary<string, object>
                {
                    {CommonInputCommandArg.KeyLevelIndex, 0}
                };
                SwitchLevelStageCommandInvoker.SwitchLevelStage(
                    EInputCommand.LoadLevelByIndex, 
                    false,
                    args);
            }
            else if (m_NextLevelMustBeFirstInGroup)
            {
                SwitchLevelStageCommandInvoker.SwitchLevelStage(
                    EInputCommand.LoadFirstLevelFromCurrentGroup,
                    true);
            }
            else
            {
                string nextLevelType = (string)_Args.Args.GetSafe(
                    CommonInputCommandArg.KeyNextLevelType, out _);
                if (nextLevelType == CommonInputCommandArg.ParameterLevelTypeBonus
                || nextLevelType == CommonInputCommandArg.ParameterLevelTypeMain)
                {
                    SwitchLevelStageCommandInvoker.SwitchLevelStage(
                        EInputCommand.LoadLevelByIndex,
                        true);
                }
                else if (CommonData.LoadNextLevelAutomatically)
                {
                    CommandsProceeder.RaiseCommand(
                        EInputCommand.LoadNextLevel,
                        _Args.Args, 
                        true);
                }
            }
        }

        private void OnCharacterKilled(
            LevelStageArgs                     _Args,
            IReadOnlyCollection<IViewMazeItem> _MazeItems)
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
                        var args = new Dictionary<string, object>
                        {
                            {CommonInputCommandArg.KeyLoadFirstLevelInGroup, true}
                        };
                        SwitchLevelStageCommandInvoker.SwitchLevelStage(
                            EInputCommand.StartUnloadingLevel, 
                            true,
                            args);
                    }
                    else
                    {
                        var panel = DialogPanelsSet.GetPanel<ICharacterDiedDialogPanel>();
                        var dv = DialogViewersController.GetViewer(panel.DialogViewerType);
                        SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.PauseLevel, true);
                        dv.Show(panel, 3f);
                    }
                });
        }
        
        private void CheckForAllLevelsPassed(LevelStageArgs _Args)
        {
            bool allLevelsPassed = SaveUtils.GetValue(SaveKeysRmazor.AllLevelsPassed);
            if (!allLevelsPassed && _Args.LevelIndex + 1 >= ViewSettings.levelsCountMain)
                SaveUtils.PutValue(SaveKeysRmazor.AllLevelsPassed, true);
        }

        private void CheckForLevelGroupOrBonusLevelFinished(LevelStageArgs _Args)
        {
            string currentLevelType = (string) _Args.Args.GetSafe(
                CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool currentLevelIsBonus = currentLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            if (_Args.Args != null && currentLevelIsBonus)
            {
                CommandsProceeder.RaiseCommand(
                    EInputCommand.FinishLevelGroupPanel,
                    _Args.Args, 
                    true);
            }
            else if (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex) 
                     && _Args.Args != null 
                     && !currentLevelIsBonus)
            {
                int nextBonusLevelIndexToLoad = RmazorUtils.GetLevelsGroupIndex(_Args.LevelIndex) - 1;
                var args = new Dictionary<string, object>
                {
                    {CommonInputCommandArg.KeyNextLevelType, CommonInputCommandArg.ParameterLevelTypeBonus}
                };
                int bonusLevelsCount = LevelsLoader.GetLevelsCount(CommonData.GameId, args);
                var inputCommand = nextBonusLevelIndexToLoad < bonusLevelsCount
                    ? EInputCommand.PlayBonusLevelPanel
                    : EInputCommand.FinishLevelGroupPanel;
                CommandsProceeder.RaiseCommand(
                    inputCommand,
                    _Args.Args, 
                    true);
            }
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

        private void SendLevelAnalyticEvent(LevelStageArgs _Args)
        {
            string analyticId = _Args.LevelStage switch
            {
                ELevelStage.CharacterKilled => AnalyticIds.CharacterDied,
                ELevelStage.Finished when _Args.PreviousStage == ELevelStage.StartedOrContinued 
                                            => AnalyticIds.LevelFinished,
                _                           => null
            };
            if (string.IsNullOrEmpty(analyticId))
                return;
            if (analyticId == AnalyticIds.LevelFinished 
                && CheckIfLevelWasFinishedAtLeastOnce(_Args.LevelIndex))
            {
                return;
            }
            Managers.AnalyticsManager.SendAnalytic(analyticId, 
                new Dictionary<string, object>
                {
                    {AnalyticIds.ParameterLevelIndex, _Args.LevelIndex},
                });
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.GetLevelFinishedAnalyticId(_Args.LevelIndex));
            if (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex))
                Managers.AnalyticsManager.SendAnalytic(AnalyticIds.LevelStageFinished);
        }

        private static bool CheckIfLevelWasFinishedAtLeastOnce(long _LevelIndex)
        {
            bool wasFinishedAtLeastOnce = false;
            var finishedOnceDict = SaveUtils.GetValue(SaveKeysRmazor.LevelsFinishedOnce);
            if (finishedOnceDict != null && finishedOnceDict.Contains(_LevelIndex))
                wasFinishedAtLeastOnce = true;
            if (wasFinishedAtLeastOnce) 
                return true;
            finishedOnceDict ??= new List<long>();
            finishedOnceDict.Add(_LevelIndex);
            finishedOnceDict = finishedOnceDict.Distinct().ToList();
            SaveUtils.PutValue(SaveKeysRmazor.LevelsFinishedOnce, finishedOnceDict);
            return false;
        }

        private void UnlockAchievementOnLevelFinishedIfKeyExist(LevelStageArgs _Args)
        {
            UnlockAchievementLevelFinishedByIndex(_Args.LevelIndex);
            UnlockSpecificAchievementOnLevelFinished();
        }

        private void UnlockAchievementLevelFinishedByIndex(long _LevelIndex)
        {
            var achievementKey = AchievementKeys.GetLevelFinishedAchievementKey(_LevelIndex + 1);
            if (!achievementKey.HasValue)
                return;
            Managers.ScoreManager.UnlockAchievement(achievementKey.Value);
        }

        private void UnlockSpecificAchievementOnLevelFinished()
        {
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