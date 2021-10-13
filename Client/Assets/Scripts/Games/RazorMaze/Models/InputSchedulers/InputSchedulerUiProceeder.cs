using System;
using System.Collections.Generic;
using System.Linq;
using GameHelpers;
using Games.RazorMaze.Views;
using Ticker;

namespace Games.RazorMaze.Models.InputSchedulers
{
    public interface IInputSchedulerUiProceeder : IAddCommand, IOnLevelStageChanged
    {
        event InputCommandHandler UiCommand;
    }
    
    public class InputSchedulerUiProceeder : IInputSchedulerUiProceeder, IUpdateTick
    {
        #region nonpublic members

        private readonly Queue<Tuple<int, object[]>> m_UiCommands = new Queue<Tuple<int, object[]>>();

        #endregion

        #region inject

        private IModelLevelStaging LevelStaging { get; }
        private IModelData Data { get; }
        private ILevelsLoader LevelsLoader { get; }

        public InputSchedulerUiProceeder(
            IUITicker _Ticker,
            IModelLevelStaging _LevelStaging,
            IModelData _Data,
            ILevelsLoader _LevelsLoader)
        {
            LevelStaging = _LevelStaging;
            Data = _Data;
            LevelsLoader = _LevelsLoader;
            _Ticker.Register(this);
            UiCommand += OnUiCommand;
        }

        #endregion

        #region api

        public event InputCommandHandler UiCommand;
        
        public void UpdateTick()
        {
            ScheduleUiCommands();
        }
        
        public void AddCommand(int _Command, object[] _Args = null)
        {
            switch (_Command)
            {
                case InputCommands.LoadCurrentLevel:
                case InputCommands.LoadNextLevel:
                case InputCommands.LoadFirstLevelFromCurrentGroup:
                case InputCommands.ReadyToContinueLevel:
                case InputCommands.ContinueLevel:
                case InputCommands.FinishLevel:
                case InputCommands.PauseLevel:
                case InputCommands.UnloadLevel:
                    m_UiCommands.Enqueue(new Tuple<int, object[]>(_Command, _Args));
                    break;
            }
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            
        }

        #endregion

        #region nonpublic methods

        private void ScheduleUiCommands()
        {
            if (!m_UiCommands.Any())
                return;
            var cmd = m_UiCommands.Dequeue();
            UiCommand?.Invoke(cmd.Item1);
        }
        
        private void OnUiCommand(int _Command, object[] _Args)
        {
            MazeInfo info;
            int levelIndex;
            switch (_Command)
            {
                case InputCommands.LoadCurrentLevel:
                    LevelStaging.LoadLevel(Data.Info, Data.LevelIndex);
                    break;
                case InputCommands.LoadNextLevel:
                    levelIndex = Data.LevelIndex + 1;
                    info = LevelsLoader.LoadLevel(1, levelIndex);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case InputCommands.LoadFirstLevelFromCurrentGroup:
                    int group = Data.LevelIndex / RazorMazeUtils.LevelsInGroup;
                    levelIndex = group * RazorMazeUtils.LevelsInGroup;
                    info = LevelsLoader.LoadLevel(1, levelIndex); 
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case InputCommands.ReadyToContinueLevel:
                    LevelStaging.ReadyToStartOrContinueLevel();
                    break;
                case InputCommands.ContinueLevel:
                    LevelStaging.StartOrContinueLevel();
                    break;
                case InputCommands.FinishLevel:
                    LevelStaging.FinishLevel();
                    break;
                case InputCommands.PauseLevel:
                    LevelStaging.PauseLevel();
                    break;
                case InputCommands.UnloadLevel:
                    LevelStaging.UnloadLevel();
                    break;
            }
        }

        #endregion
    }
}