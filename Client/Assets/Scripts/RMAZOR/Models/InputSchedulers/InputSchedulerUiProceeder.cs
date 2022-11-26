using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Helpers;
using RMAZOR.Helpers;
using RMAZOR.Models.MazeInfos;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace RMAZOR.Models.InputSchedulers
{
    public interface IInputSchedulerUiProceeder : IInit, IAddCommand
    {
        event UnityAction<EInputCommand, Dictionary<string, object>> UiCommand;
    }
    
    public class InputSchedulerUiProceeder : InitBase, IInputSchedulerUiProceeder
    {
        #region inject

        private IModelLevelStaging LevelStaging { get; }
        private IModelData         Data         { get; }
        private ILevelsLoader      LevelsLoader { get; }

        private InputSchedulerUiProceeder(
            IModelLevelStaging _LevelStaging,
            IModelData         _Data,
            ILevelsLoader      _LevelsLoader)
        {
            LevelStaging = _LevelStaging;
            Data         = _Data;
            LevelsLoader = _LevelsLoader;

        }

        #endregion

        #region api

        public event UnityAction<EInputCommand, Dictionary<string, object>> UiCommand;

        public override void Init()
        {
            UiCommand += OnUiCommand;
            base.Init();
        }

        public void AddCommand(EInputCommand _Command, Dictionary<string, object> _Args = null)
        {
            switch (_Command)
            {
                case EInputCommand.LoadCurrentLevel:
                case EInputCommand.LoadNextLevel:
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
                case EInputCommand.StartUnloadingLevel:
                case EInputCommand.LoadLevelByIndex:
                    UiCommand?.Invoke(_Command, _Args);
                    break;
            }
        }

        #endregion

        #region nonpublic methods

        private void OnUiCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            MazeInfo info;
            long levelIndex;
            int gameId = CommonData.GameId;
            switch (_Command)
            {
                case EInputCommand.LoadCurrentLevel:
                    LevelStaging.LoadLevel(Data.Info, LevelStaging.LevelIndex);
                    break;
                case EInputCommand.LoadNextLevel:
                    levelIndex = LevelStaging.LevelIndex + 1;
                    info = LevelsLoader.GetLevelInfo(gameId, levelIndex, null);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadLevelByIndex:
                    object levelIndexArg = _Args.GetSafe(CommonInputCommandArg.KeyLevelIndex, out bool keyExist);
                    if (!keyExist)
                        Dbg.LogError("Level index does not exist in command arguments");
                    levelIndex = Convert.ToInt64(levelIndexArg);
                    info = LevelsLoader.GetLevelInfo(gameId, levelIndex, _Args);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadFirstLevelFromRandomGroup:
                    int randLevelIdx = Mathf.RoundToInt(Random.value * LevelsLoader.GetLevelsCount(gameId, null));
                    int randGroup = RmazorUtils.GetLevelsGroupIndex(randLevelIdx);
                    levelIndex = RmazorUtils.GetFirstLevelInGroup(randGroup);
                    info = LevelsLoader.GetLevelInfo(gameId, levelIndex, null);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadRandomLevel:
                    levelIndex = Mathf.RoundToInt(
                        Random.value *
                        LevelsLoader.GetLevelsCount(gameId, null));
                    info = LevelsLoader.GetLevelInfo(gameId, levelIndex, null); 
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadRandomLevelWithRotation:
                    levelIndex = Mathf.RoundToInt(
                        Random.value *
                        LevelsLoader.GetLevelsCount(gameId, null));
                    info = LevelsLoader.GetLevelInfo(gameId, levelIndex, null);
                    while (info.MazeItems.All(_Item =>
                        _Item.Type != EMazeItemType.GravityBlock && _Item.Type != EMazeItemType.GravityTrap))
                    {
                        levelIndex = Mathf.RoundToInt(
                            Random.value *
                            LevelsLoader.GetLevelsCount(gameId, null));
                        info = LevelsLoader.GetLevelInfo(gameId, levelIndex, null);
                    }
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.ReadyToStartLevel:    LevelStaging.ReadyToStartLevel(_Args);    break;
                case EInputCommand.StartOrContinueLevel: LevelStaging.StartOrContinueLevel(_Args); break;
                case EInputCommand.FinishLevel:          LevelStaging.FinishLevel(_Args);          break;
                case EInputCommand.PauseLevel:           LevelStaging.PauseLevel(_Args);           break;
                case EInputCommand.UnPauseLevel:         LevelStaging.UnPauseLevel(_Args);         break;
                case EInputCommand.StartUnloadingLevel:   LevelStaging.ReadyToUnloadLevel(_Args);   break;
                case EInputCommand.UnloadLevel:          LevelStaging.UnloadLevel(_Args);          break;
                case EInputCommand.KillCharacter:        LevelStaging.KillCharacter(_Args);        break;
            }
        }

        #endregion
    }
}