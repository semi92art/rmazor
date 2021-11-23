using System.Collections.Generic;
using System.Linq;
using Constants;
using Controllers;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.UI;
using Mono_Installers;
using Ticker;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.Common
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
            new AudioClipArgs("level_start", EAudioClipType.Sound);
        private static AudioClipArgs AudioClipArgsLevelComplete => 
            new AudioClipArgs("level_complete", EAudioClipType.Sound);
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
            IViewGameTicker _ViewGameTicker,
            IModelGameTicker _ModelGameTicker,
            IModelGame _Model,
            IManagersGetter _Managers,
            IViewCharacter _Character,
            IViewInputCommandsProceeder _CommandsProceeder,
            IContainersGetter _ContainersGetter,
            IMazeShaker _MazeShaker,
            IDialogPanels _DialogPanels,
            IProposalDialogViewer _ProposalDialogViewer,
            ILevelsLoader _LevelsLoader)
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
        
        public event UnityAction Initialized;
        
        public void Init()
        {
            CommandsProceeder.Command += OnCommand;
            Initialized?.Invoke();
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
            foreach (var proceeder in m_Proceeders)
                proceeder.OnLevelStageChanged(_Args);
            ProceedMazeItemGroups(_Args);
            ProceedSounds(_Args);
            ProceedTime(_Args);
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
            var mazeItems = new List<IViewMazeItem>();
            var mazeItemGroups = m_Proceeders
                .Select(_P => _P as IViewMazeItemGroup)
                .Where(_P => _P != null);
            foreach (var group in mazeItemGroups)
                mazeItems.AddRange(group.GetActiveItems());
            IViewMazePathItemsGroup pathItemsGroup = null;
            if (m_Proceeders
                    .FirstOrDefault(_P => _P is IViewMazePathItemsGroup)
                is IViewMazePathItemsGroup g)
            {
                pathItemsGroup = g;
                mazeItems.AddRange(g.PathItems);
            }
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    SaveUtils.PutValue(SaveKeys.CurrentLevelIndex, _Args.LevelIndex);
                    m_NextLevelMustBeFirstInGroup = false;
                    Character.Appear(true);
                    if (pathItemsGroup != null)
                        foreach (var pathItem in pathItemsGroup.PathItems)
                        {
                            bool collect = Model.PathItemsProceeder.PathProceeds[pathItem.Props.Position];
                            pathItem.Collected = collect;
                        }

                    foreach (var mazeItem in mazeItems)
                        mazeItem.Appear(true);
                    Coroutines.Run(Coroutines.WaitWhile(() =>
                        {
                            return Character.AppearingState != EAppearingState.Appeared
                                   || mazeItems.Any(_Item => _Item.AppearingState != EAppearingState.Appeared);
                        },
                        () => Model.LevelStaging.ReadyToStartLevel()));
                    break;
                case ELevelStage.ReadyToStart:
                    break;
                case ELevelStage.Finished:
                    if (_Args.LevelIndex % 3 == 0)
                    {
                        Managers.AdsManager.ShowInterstitialAd(null);
                    }
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.LevelFinished, 
                        new Dictionary<string, object>
                        {
                            {"level_index", _Args.LevelIndex},
                            {"level_time", Model.LevelStaging.LevelTime},
                            {"dies_count", Model.LevelStaging.DiesCount}
                        });
                    bool allLevelsPassed = SaveUtils.GetValue(SaveKeys.AllLevelsPassed);
                    if (!allLevelsPassed)
                    {
                        SaveUtils.PutValue(SaveKeys.CurrentLevelIndex, _Args.LevelIndex + 1);
                        if (_Args.LevelIndex + 1 >= LevelsLoader.GetLevelsCount(GameClientUtils.GameId))
                            SaveUtils.PutValue(SaveKeys.AllLevelsPassed, true);
                    }
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    foreach (var mazeItem in mazeItems)
                        mazeItem.Appear(false);
                    Coroutines.Run(Coroutines.WaitWhile(() =>
                        {
                            return mazeItems.Any(_Item => _Item.AppearingState != EAppearingState.Dissapeared);
                        },
                        () =>
                        {
                            CommandsProceeder.RaiseCommand(EInputCommand.UnloadLevel, null, true);
                        }));
                    break;
                case ELevelStage.Unloaded:
                    if (SaveUtils.GetValue(SaveKeys.AllLevelsPassed ) && (_Args.LevelIndex - 2) % 3 == 0)
                        CommandsProceeder.RaiseCommand(EInputCommand.LoadFirstLevelFromRandomGroup, null, true);
                    else if (m_NextLevelMustBeFirstInGroup)
                        CommandsProceeder.RaiseCommand(EInputCommand.LoadFirstLevelFromCurrentGroup, null, true);
                    else if (RazorMazeUtils.LoadNextLevelAutomatically)
                        CommandsProceeder.RaiseCommand(EInputCommand.LoadNextLevel, null, true);
                    break;
                case ELevelStage.CharacterKilled:
                    MazeShaker.OnCharacterDeathAnimation(
                        ContainersGetter.GetContainer(ContainerNames.Character).transform.position,
                        mazeItems,
                        () =>
                        {
                            if (!LevelMonoInstaller.Release)
                                return;
                            var panel = DialogPanels.CharacterDiedDialogPanel;
                            panel.Init();
                            ProposalDialogViewer.Show(panel);
                        });
                    break;
            }
        }

        private void ProceedTime(LevelStageArgs _Args)
        {
            bool pause = _Args.Stage == ELevelStage.Paused;
            ViewGameTicker.Pause = pause;
            ModelGameTicker.Pause = pause;
        }

        private void ProceedSounds(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
            {
                Managers.AudioManager.PlayClip(AudioClipArgsLevelStart);
                if (!m_FirstTimeLevelLoaded)
                {
                    Managers.AudioManager.PlayClip(AudioClipArgsMainTheme);
                    m_FirstTimeLevelLoaded = true;
                }
            }
            else if (_Args.Stage == ELevelStage.Finished && _Args.PreviousStage != ELevelStage.Paused)
                Managers.AudioManager.PlayClip(AudioClipArgsLevelComplete);
        }
        
        #endregion
    }
}