using System;
using System.Collections.Generic;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models.MazeInfos;
using UnityEngine.Events;

namespace RMAZOR.Models.InputSchedulers
{
    public interface IInputSchedulerUiProceeder : IInit, IAddCommand
    {
        Func<int, long, bool, MazeInfo>                              LoadMazeInfo { set; }
        event UnityAction<EInputCommand, Dictionary<string, object>> UiCommand;
    }
    
    public class InputSchedulerUiProceeder : InitBase, IInputSchedulerUiProceeder
    {
        #region inject

        private IModelLevelStaging LevelStaging { get; }
        private IModelData         Data         { get; }

        private InputSchedulerUiProceeder(
            IModelLevelStaging _LevelStaging,
            IModelData         _Data)
        {
            LevelStaging = _LevelStaging;
            Data         = _Data;
        }

        #endregion

        #region api

        public Func<int, long, bool, MazeInfo> LoadMazeInfo { private get; set; }

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
                case EInputCommand.ReadyToStartLevel:
                case EInputCommand.StartOrContinueLevel:
                case EInputCommand.FinishLevel:
                case EInputCommand.PauseLevel:
                case EInputCommand.UnPauseLevel:
                case EInputCommand.UnloadLevel:
                case EInputCommand.KillCharacter:
                case EInputCommand.StartUnloadingLevel:
                case EInputCommand.LoadLevelByIndex:
                case EInputCommand.ExitLevelStaging:
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
                    info = LoadMazeInfo(gameId, levelIndex, false);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.LoadLevelByIndex:
                    object levelIndexArg = _Args.GetSafe(CommonInputCommandArg.KeyLevelIndex, out bool keyExist);
                    if (!keyExist)
                        Dbg.LogError("Level index does not exist in command arguments");
                    levelIndex = Convert.ToInt64(levelIndexArg);
                    string levelType = (string) _Args.GetSafe(CommonInputCommandArg.KeyNextLevelType, out _);
                    bool isBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
                    info = LoadMazeInfo(gameId, levelIndex, isBonus);
                    LevelStaging.LoadLevel(info, levelIndex);
                    break;
                case EInputCommand.ReadyToStartLevel:    LevelStaging.ReadyToStartLevel(_Args);    break;
                case EInputCommand.StartOrContinueLevel: LevelStaging.StartOrContinueLevel(_Args); break;
                case EInputCommand.FinishLevel:          LevelStaging.FinishLevel(_Args);          break;
                case EInputCommand.PauseLevel:           LevelStaging.PauseLevel(_Args);           break;
                case EInputCommand.UnPauseLevel:         LevelStaging.UnPauseLevel(_Args);         break;
                case EInputCommand.StartUnloadingLevel:  LevelStaging.ReadyToUnloadLevel(_Args);   break;
                case EInputCommand.UnloadLevel:          LevelStaging.UnloadLevel(_Args);          break;
                case EInputCommand.KillCharacter:        LevelStaging.KillCharacter(_Args);        break;
                case EInputCommand.ExitLevelStaging:     LevelStaging.ExitLevelStaging(_Args);     break;
            }
        }

        #endregion
    }
}