using System;
using System.Collections.Generic;
using System.Linq;
using GameHelpers;
using Games.RazorMaze.Views;
using Ticker;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

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
                case InputCommands.LoadFirstLevelFromRandomGroup:
                case InputCommands.LoadRandomLevel:
                case InputCommands.LoadRandomLevelWithRotation:
                case InputCommands.ReadyToStartLevel:
                case InputCommands.StartOrContinueLevel:
                case InputCommands.FinishLevel:
                case InputCommands.PauseLevel:
                case InputCommands.UnPauseLevel:
                case InputCommands.UnloadLevel:
                case InputCommands.KillCharacter:
                case InputCommands.ReadyToUnloadLevel:
                case InputCommands.LoadLevelByIndex:
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
            var (key, args) = m_UiCommands.Dequeue();
            UiCommand?.Invoke(key, args);
        }
        
        private void OnUiCommand(int _Command, object[] _Args)
        {
            MazeInfo info;
            int levelIndex;
            int gameId = GameClientUtils.GameId;
            switch (_Command)
            {
                case InputCommands.LoadCurrentLevel:
                    LevelStaging.LoadLevel(Data.Info, Data.LevelIndex);
                    break;
                case InputCommands.LoadNextLevel:
                    levelIndex = Data.LevelIndex + 1;
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case InputCommands.LoadLevelByIndex:
                    levelIndex = Convert.ToInt32(_Args[0]);
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case InputCommands.LoadFirstLevelFromCurrentGroup:
                    int group = Data.LevelIndex / RazorMazeUtils.LevelsInGroup;
                    levelIndex = group * RazorMazeUtils.LevelsInGroup;
                    info = LevelsLoader.LoadLevel(gameId, levelIndex); 
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case InputCommands.LoadFirstLevelFromRandomGroup:
                    int randLevelIdx = Mathf.RoundToInt(Random.value * LevelsLoader.GetLevelsCount(gameId));
                    int randGroup = randLevelIdx / RazorMazeUtils.LevelsInGroup;
                    levelIndex = randGroup * RazorMazeUtils.LevelsInGroup;
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case InputCommands.LoadRandomLevel:
                    levelIndex = Mathf.RoundToInt(
                        Random.value *
                        LevelsLoader.GetLevelsCount(gameId));
                    info = LevelsLoader.LoadLevel(gameId, levelIndex); 
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case InputCommands.LoadRandomLevelWithRotation:
                    levelIndex = Mathf.RoundToInt(
                        UnityEngine.Random.value *
                        LevelsLoader.GetLevelsCount(gameId));
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    while (info.MazeItems.All(_Item =>
                        _Item.Type != EMazeItemType.GravityBlock && _Item.Type != EMazeItemType.GravityTrap))
                    {
                        levelIndex = Mathf.RoundToInt(
                            UnityEngine.Random.value *
                            LevelsLoader.GetLevelsCount(gameId));
                        info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    }
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case InputCommands.ReadyToStartLevel:
                    LevelStaging.ReadyToStartLevel();
                    break;
                case InputCommands.StartOrContinueLevel:
                    LevelStaging.StartOrContinueLevel();
                    break;
                case InputCommands.FinishLevel:
                    LevelStaging.FinishLevel();
                    break;
                case InputCommands.PauseLevel:
                    LevelStaging.PauseLevel();
                    break;
                case InputCommands.UnPauseLevel:
                    LevelStaging.UnPauseLevel();
                    break;
                case InputCommands.UnloadLevel:
                    LevelStaging.UnloadLevel();
                    break;
                case InputCommands.KillCharacter:
                    LevelStaging.KillCharacter();
                    break;
                case InputCommands.ReadyToUnloadLevel:
                    LevelStaging.ReadyToUnloadLevel();
                    break;
            }
        }

        #endregion
    }
}