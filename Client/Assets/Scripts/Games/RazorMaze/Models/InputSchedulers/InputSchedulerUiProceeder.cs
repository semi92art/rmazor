using System;
using System.Collections.Generic;
using System.Linq;
using GameHelpers;
using Games.RazorMaze.Views;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Random = UnityEngine.Random;

namespace Games.RazorMaze.Models.InputSchedulers
{
    public interface IInputSchedulerUiProceeder : IAddCommand, IOnLevelStageChanged
    {
        event UnityAction<EInputCommand, object[]> UiCommand;
    }
    
    public class InputSchedulerUiProceeder : IInputSchedulerUiProceeder, IUpdateTick
    {
        #region nonpublic members

        private int m_UiCommandsCount;
        private readonly Queue<Tuple<EInputCommand, object[]>> m_UiCommands = new Queue<Tuple<EInputCommand, object[]>>();

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

        public event UnityAction<EInputCommand, object[]> UiCommand;
        
        public void UpdateTick()
        {
            ScheduleUiCommands();
        }
        
        public void AddCommand(EInputCommand _Command, object[] _Args = null)
        {
            switch (_Command)
            {
                case EInputCommand.LoadCurrentLevel:
                case EInputCommand.LoadNextLevel:
                case EInputCommand.LoadFirstLevelFromCurrentGroup:
                case EInputCommand.LoadFirstLevelFromRandomGroup:
                case EInputCommand.LoadRandomLevel:
                case EInputCommand.LoadRandomLevelWithRotation:
                case EInputCommand.ReadyToStartLevel:
                case EInputCommand.StartOrContinueLevel:
                case EInputCommand.FinishLevel:
                case EInputCommand.PauseLevel:
                case EInputCommand.UnPauseLevel:
                case EInputCommand.UnloadLevel:
                case EInputCommand.KillCharacter:
                case EInputCommand.ReadyToUnloadLevel:
                case EInputCommand.LoadLevelByIndex:
                    m_UiCommands.Enqueue(new Tuple<EInputCommand, object[]>(_Command, _Args));
                    m_UiCommandsCount++;
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
            if (m_UiCommandsCount == 0)
                return;
            var (key, args) = m_UiCommands.Dequeue();
            m_UiCommandsCount--;
            UiCommand?.Invoke(key, args);
        }
        
        private void OnUiCommand(EInputCommand _Command, object[] _Args)
        {
            MazeInfo info;
            int levelIndex;
            int gameId = GameClientUtils.GameId;
            switch (_Command)
            {
                case EInputCommand.LoadCurrentLevel:
                    LevelStaging.LoadLevel(Data.Info, Data.LevelIndex);
                    break;
                case EInputCommand.LoadNextLevel:
                    levelIndex = Data.LevelIndex + 1;
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadLevelByIndex:
                    levelIndex = Convert.ToInt32(_Args[0]);
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadFirstLevelFromCurrentGroup:
                    int group = Data.LevelIndex / RazorMazeUtils.LevelsInGroup;
                    levelIndex = group * RazorMazeUtils.LevelsInGroup;
                    info = LevelsLoader.LoadLevel(gameId, levelIndex); 
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadFirstLevelFromRandomGroup:
                    int randLevelIdx = Mathf.RoundToInt(Random.value * LevelsLoader.GetLevelsCount(gameId));
                    int randGroup = randLevelIdx / RazorMazeUtils.LevelsInGroup;
                    levelIndex = randGroup * RazorMazeUtils.LevelsInGroup;
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadRandomLevel:
                    levelIndex = Mathf.RoundToInt(
                        Random.value *
                        LevelsLoader.GetLevelsCount(gameId));
                    info = LevelsLoader.LoadLevel(gameId, levelIndex); 
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadRandomLevelWithRotation:
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
                case EInputCommand.ReadyToStartLevel:
                    LevelStaging.ReadyToStartLevel();
                    break;
                case EInputCommand.StartOrContinueLevel:
                    LevelStaging.StartOrContinueLevel();
                    break;
                case EInputCommand.FinishLevel:
                    LevelStaging.FinishLevel();
                    break;
                case EInputCommand.PauseLevel:
                    LevelStaging.PauseLevel();
                    break;
                case EInputCommand.UnPauseLevel:
                    LevelStaging.UnPauseLevel();
                    break;
                case EInputCommand.UnloadLevel:
                    LevelStaging.UnloadLevel();
                    break;
                case EInputCommand.KillCharacter:
                    LevelStaging.KillCharacter();
                    break;
                case EInputCommand.ReadyToUnloadLevel:
                    LevelStaging.ReadyToUnloadLevel();
                    break;
            }
        }

        #endregion
    }
}