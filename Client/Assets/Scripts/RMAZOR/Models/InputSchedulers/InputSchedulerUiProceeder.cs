using System;
using System.Collections;
using System.Collections.Generic;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models.MazeInfos;
using UnityEngine.Events;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Models.InputSchedulers
{
    public interface IInputSchedulerUiProceeder : IInit, IAddCommand
    {
        Func<LevelInfoArgs, Entity<MazeInfo>>                        LoadMazeInfo { set; }
        event UnityAction<EInputCommand, Dictionary<string, object>> UiCommand;
    }
    
    public class InputSchedulerUiProceeder : InitBase, IInputSchedulerUiProceeder
    {
        #region inject

        private IModelLevelStaging LevelStaging { get; }

        private InputSchedulerUiProceeder(IModelLevelStaging _LevelStaging)
        {
            LevelStaging = _LevelStaging;
        }

        #endregion

        #region api

        public Func<LevelInfoArgs, Entity<MazeInfo>> LoadMazeInfo { private get; set; }

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
                case EInputCommand.ReadyToStartLevel:
                case EInputCommand.StartOrContinueLevel:
                case EInputCommand.FinishLevel:
                case EInputCommand.PauseLevel:
                case EInputCommand.UnPauseLevel:
                case EInputCommand.UnloadLevel:
                case EInputCommand.KillCharacter:
                case EInputCommand.StartUnloadingLevel:
                case EInputCommand.LoadLevel:
                case EInputCommand.ExitLevelStaging:
                    UiCommand?.Invoke(_Command, _Args);
                    break;
            }
        }

        #endregion

        #region nonpublic methods

        private void OnUiCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            switch (_Command)
            {
                case EInputCommand.LoadLevel:            LoadLevelOnUiCommand(_Args); break;
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

        private void LoadLevelOnUiCommand(Dictionary<string, object> _Args)
        {
            object levelIndexArg = _Args.GetSafe(KeyLevelIndex, out bool levelIndexKeyExist);
            if (!levelIndexKeyExist)
                Dbg.LogError("Level index does not exist in command arguments");
            long levelIndex = Convert.ToInt64(levelIndexArg);
            string levelType = (string) _Args.GetSafe(KeyNextLevelType, out _);
            string gameMode = (string) _Args.GetSafe(KeyGameMode, out _);
            var args = new LevelInfoArgs
            {
                LevelIndex = levelIndex,
                LevelType  = levelType,
                GameMode   = gameMode
            };
            var additonalArgKeys = new[] {KeyRandomLevelSize, KeyRemoveTrapsFromLevel};
            foreach (string additionalArgKey in additonalArgKeys)
            {
                object argValue = _Args.GetSafe(additionalArgKey, out bool additionalArgKeyExist);
                if (additionalArgKeyExist)
                    args.Arguments.Add(additionalArgKey, argValue);    
            }
            var entityLevelInfo = LoadMazeInfo(args);
            Cor.Run(LoadLevelCoroutine(entityLevelInfo, levelIndex, _Args));
        }

        private IEnumerator LoadLevelCoroutine(
            Entity<MazeInfo>           _EntityLevelInfo,
            long                       _LevelIndex,
            Dictionary<string, object> _Args)
        {
            yield return Cor.WaitWhile(
                () => _EntityLevelInfo.Result == EEntityResult.Pending,
                () =>
                {
                    if (_EntityLevelInfo.Result == EEntityResult.Fail)
                    {
                        Dbg.LogError(_EntityLevelInfo.Error);
                        return;
                    }
                    var onReadyToLoadLevelAction = (UnityAction)_Args.GetSafe(
                        KeyOnReadyToLoadLevelFinishAction, out _);
                    onReadyToLoadLevelAction?.Invoke();
                    LevelStaging.LoadLevel(_EntityLevelInfo.Value, _LevelIndex, _Args);
                });
        }

        #endregion
    }
}