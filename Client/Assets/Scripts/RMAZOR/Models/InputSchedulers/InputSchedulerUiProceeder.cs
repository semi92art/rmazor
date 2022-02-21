﻿using System;
using System.Linq;
using Common;
using RMAZOR.Helpers;
using RMAZOR.Models.MazeInfos;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace RMAZOR.Models.InputSchedulers
{
    public interface IInputSchedulerUiProceeder : IAddCommand
    {
        event UnityAction<EInputCommand, object[]> UiCommand;
    }
    
    public class InputSchedulerUiProceeder : IInputSchedulerUiProceeder
    {
        #region inject

        private IModelLevelStaging LevelStaging { get; }
        private IModelData Data { get; }
        private ILevelsLoader LevelsLoader { get; }

        public InputSchedulerUiProceeder(
            IModelLevelStaging _LevelStaging,
            IModelData _Data,
            ILevelsLoader _LevelsLoader)
        {
            LevelStaging = _LevelStaging;
            Data = _Data;
            LevelsLoader = _LevelsLoader;
            UiCommand += OnUiCommand;
        }

        #endregion

        #region api

        public event UnityAction<EInputCommand, object[]> UiCommand;

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
                    UiCommand?.Invoke(_Command, _Args);
                    break;
            }
        }

        #endregion

        #region nonpublic methods

        private void OnUiCommand(EInputCommand _Command, object[] _Args)
        {
            MazeInfo info;
            long levelIndex;
            int gameId = GameClientUtils.GameId;
            switch (_Command)
            {
                case EInputCommand.LoadCurrentLevel:
                    LevelStaging.LoadLevel(Data.Info, LevelStaging.LevelIndex);
                    break;
                case EInputCommand.LoadNextLevel:
                    levelIndex = LevelStaging.LevelIndex + 1;
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadLevelByIndex:
                    levelIndex = Convert.ToInt32(_Args[0]);
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadFirstLevelFromCurrentGroup:
                    int group = RazorMazeUtils.GetGroupIndex(LevelStaging.LevelIndex);
                    levelIndex = RazorMazeUtils.GetFirstLevelInGroup(group);
                    info = LevelsLoader.LoadLevel(gameId, levelIndex); 
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadFirstLevelFromRandomGroup:
                    int randLevelIdx = Mathf.RoundToInt(Random.value * LevelsLoader.GetLevelsCount(gameId));
                    int randGroup = RazorMazeUtils.GetGroupIndex(randLevelIdx);
                    levelIndex = RazorMazeUtils.GetFirstLevelInGroup(randGroup);
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
                        Random.value *
                        LevelsLoader.GetLevelsCount(gameId));
                    info = LevelsLoader.LoadLevel(gameId, levelIndex);
                    while (info.MazeItems.All(_Item =>
                        _Item.Type != EMazeItemType.GravityBlock && _Item.Type != EMazeItemType.GravityTrap))
                    {
                        levelIndex = Mathf.RoundToInt(
                            Random.value *
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