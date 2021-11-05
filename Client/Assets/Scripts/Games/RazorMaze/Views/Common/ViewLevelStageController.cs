using System.Collections.Generic;
using System.Linq;
using Constants;
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
using UnityEngine;
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
        #region constants

        private const string SoundClipNameLevelStart = "level_start";
        private const string SoundClipNameLevelComplete = "level_complete";

        #endregion
        
        #region nonpublic members

        private readonly List<IOnLevelStageChanged> m_Proceeders = new List<IOnLevelStageChanged>();

        private bool m_NextLevelMustBeFirstInGroup;

        #endregion

        #region inject

        private IGameTicker           GameTicker           { get; }
        private IModelGame            Model                { get; }
        private IManagersGetter       Managers             { get; }
        private IViewCharacter        Character            { get; }
        private IViewInput            Input                { get; }
        private IContainersGetter     ContainersGetter     { get; }
        private IMazeShaker           MazeShaker           { get; }
        private IDialogPanels         DialogPanels         { get; }
        private IProposalDialogViewer ProposalDialogViewer { get; }
        private ILevelsLoader         LevelsLoader         { get; }

        public ViewLevelStageController(
            IGameTicker _GameTicker,
            IModelGame _Model,
            IManagersGetter _Managers,
            IViewCharacter _Character,
            IViewInput _Input,
            IContainersGetter _ContainersGetter,
            IMazeShaker _MazeShaker,
            IDialogPanels _DialogPanels,
            IProposalDialogViewer _ProposalDialogViewer,
            ILevelsLoader _LevelsLoader)
        {
            GameTicker = _GameTicker;
            Model = _Model;
            Managers = _Managers;
            Character = _Character;
            Input = _Input;
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
            Input.Command += OnCommand;
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

        private void OnCommand(int _Key, object[] _Args)
        {
            if (_Key != InputCommands.ReadyToUnloadLevel)
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
            if (m_Proceeders
                    .FirstOrDefault(_P => _P is IViewMazePathItemsGroup)
                is IViewMazePathItemsGroup pathItemsGroup)
            {
                mazeItems.AddRange(pathItemsGroup.PathItems);
            }
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    SaveUtils.PutValue(SaveKey.CurrentLevelIndex, _Args.LevelIndex);
                    m_NextLevelMustBeFirstInGroup = false;
                    Character.Appear(true);
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
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.LevelFinished, 
                        new Dictionary<string, object>
                        {
                            {"level_index", _Args.LevelIndex},
                            {"level_time", Model.LevelStaging.LevelTime},
                            {"dies_count", Model.LevelStaging.DiesCount}
                        });
                    bool allLevelsPassed = SaveUtils.GetValue<bool>(SaveKey.AllLevelsPassed);
                    if (!allLevelsPassed)
                    {
                        SaveUtils.PutValue(SaveKey.CurrentLevelIndex, _Args.LevelIndex + 1);
                        if (_Args.LevelIndex + 1 >= LevelsLoader.GetLevelsCount(GameClientUtils.GameId))
                            SaveUtils.PutValue(SaveKey.AllLevelsPassed, true);
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
                            Model.LevelStaging.UnloadLevel();
                        }));
                    break;
                case ELevelStage.Unloaded:
                    if (SaveUtils.GetValue<bool>(SaveKey.AllLevelsPassed ) && (_Args.LevelIndex - 2) % 3 == 0)
                        Input.RaiseCommand(InputCommands.LoadFirstLevelFromRandomGroup, null, true);
                    else if (m_NextLevelMustBeFirstInGroup)
                        Input.RaiseCommand(InputCommands.LoadFirstLevelFromCurrentGroup, null, true);
                    else if (RazorMazeUtils.LoadNextLevelAutomatically)
                        Input.RaiseCommand(InputCommands.LoadNextLevel, null, true);
                    break;
                case ELevelStage.CharacterKilled:
                    MazeShaker.OnCharacterDeathAnimation(
                        ContainersGetter.GetContainer(ContainerNames.Character).transform.position,
                        mazeItems,
                        () =>
                        {
                            var panel = DialogPanels.CharacterDiedDialogPanel;
                            panel.Init();
                            ProposalDialogViewer.Show(panel);
                        });
                    break;
            }
        }

        private void ProceedTime(LevelStageArgs _Args)
        {
            GameTicker.Pause = _Args.Stage == ELevelStage.Paused;
        }

        private void ProceedSounds(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                Managers.Notify(_SM => _SM.PlayClip(SoundClipNameLevelStart));
            else if (_Args.Stage == ELevelStage.Finished)
                Managers.Notify(_SM => _SM.PlayClip(SoundClipNameLevelComplete));
        }
        
        #endregion
    }
}