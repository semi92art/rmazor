using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Ticker;
using Common.Utils;
using DialogViewers;
using GameHelpers;
using Managers;
using Managers.Audio;
using Mono_Installers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Characters;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.UI;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewLevelStageController : IOnLevelStageChanged, IInit
    {
        void RegisterProceeders(IEnumerable<IOnLevelStageChanged> _Proceeders);
        void OnAllPathProceed(V2Int _LastPath);
    }

    public class ViewLevelStageController : IViewLevelStageController
    {
        #region nonpublic members
        
        private static AudioClipArgs AudioClipArgsLevelStart => 
            new AudioClipArgs("level_start", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsLevelComplete => 
            new AudioClipArgs("level_complete", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsMainTheme =>
            new AudioClipArgs("main_theme", EAudioClipType.Music, _Loop: true);

        private readonly List<IOnLevelStageChanged> m_Proceeders = new List<IOnLevelStageChanged>();

        private bool m_NextLevelMustBeFirstInGroup;
        private bool m_FirstTimeLevelLoaded;

        #endregion

        #region inject

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
        private ILevelsLoader               LevelsLoader         { get; }

        public ViewLevelStageController(
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
            ILevelsLoader               _LevelsLoader)
        {
            ViewGameTicker = _ViewGameTicker;
            ModelGameTicker = _ModelGameTicker;
            Model = _Model;
            Managers = _Managers;
            Character = _Character;
            CommandsProceeder = _CommandsProceeder;
            ContainersGetter = _ContainersGetter;
            MazeShaker = _MazeShaker;
            DialogPanels = _DialogPanels;
            ProposalDialogViewer = _ProposalDialogViewer;
            LevelsLoader = _LevelsLoader;
        }

        #endregion

        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        
        public void Init()
        {
            CommandsProceeder.Command += OnCommand;
            Managers.AudioManager.InitClip(AudioClipArgsLevelStart);
            Managers.AudioManager.InitClip(AudioClipArgsLevelComplete);
            Initialize?.Invoke();
            Initialized = true;
        }

        public void RegisterProceeders(IEnumerable<IOnLevelStageChanged> _Proceeders)
        {
            m_Proceeders.Clear();
            m_Proceeders.AddRange(_Proceeders);
        }

        public void OnAllPathProceed(V2Int _LastPath)
        {
            if (LevelMonoInstaller.Release)
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
            var mazeItems = GetMazeItems(m_Proceeders);
            var pathItemsGroup = GetPathItemsGroup(m_Proceeders);
            if (pathItemsGroup != null)
                mazeItems.AddRange(pathItemsGroup.PathItems);
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:             OnLevelLoaded(_Args, mazeItems, pathItemsGroup); break;
                case ELevelStage.ReadyToStart:                                                        break; 
                case ELevelStage.Finished:           OnLevelFinished(_Args);                          break;
                case ELevelStage.ReadyToUnloadLevel: OnReadyToUnloadLevel(_Args, mazeItems);          break;
                case ELevelStage.Unloaded:           OnLevelUnloaded(_Args);                          break;
                case ELevelStage.CharacterKilled:    OnCharacterKilled(mazeItems);                    break;
            }
        }

        private static List<IViewMazeItem> GetMazeItems(IEnumerable<object> _Proceeders)
        {
            var mazeItems = new List<IViewMazeItem>();
            var mazeItemGroups = _Proceeders
                .Select(_P => _P as IViewMazeItemGroup)
                .Where(_P => _P != null);
            foreach (var group in mazeItemGroups)
                mazeItems.AddRange(group.GetActiveItems());
            return mazeItems;
        }
        
        private static IViewMazePathItemsGroup GetPathItemsGroup(IEnumerable<object> _Proceeders)
        {
            return _Proceeders?
                .Select(_P => _P as IViewMazePathItemsGroup)
                .First(_P => _P != null);
        }

        private void OnLevelLoaded(
            LevelStageArgs                     _Args,
            IReadOnlyCollection<IViewMazeItem> _MazeItems,
            IViewMazePathItemsGroup            _PathItemsGroup)
        {
            Managers.ScoreManager.SetScore(DataFieldIds.Level, _Args.LevelIndex, true);
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
            bool allLevelsPassed = SaveUtils.GetValue(SaveKeys.AllLevelsPassed);
            if (!allLevelsPassed)
            {
                Dbg.Log($"{_Args.LevelIndex + 1} {LevelsLoader.GetLevelsCount(GameClientUtils.GameId)}");
                if (_Args.LevelIndex + 1 >= LevelsLoader.GetLevelsCount(GameClientUtils.GameId))
                    SaveUtils.PutValue(SaveKeys.AllLevelsPassed, true);
            }
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
            if ((_Args.LevelIndex + 1) % 3 == 0)
                Managers.AdsManager.ShowInterstitialAd(null);
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
            Managers.ScoreManager.SetScore(DataFieldIds.Level, _Args.LevelIndex + 1, true);
            if (SaveUtils.GetValue(SaveKeys.AllLevelsPassed ) && (_Args.LevelIndex - 2) % 3 == 0)
                CommandsProceeder.RaiseCommand(EInputCommand.LoadFirstLevelFromRandomGroup, null, true);
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
                    if (!LevelMonoInstaller.Release)
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
                    
                    if (!m_FirstTimeLevelLoaded)
                    {
                        audioManager.PlayClip(AudioClipArgsMainTheme);
                        m_FirstTimeLevelLoaded = true;
                    }
                    break;
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused:
                    audioManager.PlayClip(AudioClipArgsLevelComplete);
                    break;
                case ELevelStage.Paused:
                    audioManager.MuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Loaded:
                    audioManager.PlayClip(AudioClipArgsLevelStart);
                    audioManager.UnmuteAudio(EAudioClipType.GameSound);
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
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        #endregion
    }
}