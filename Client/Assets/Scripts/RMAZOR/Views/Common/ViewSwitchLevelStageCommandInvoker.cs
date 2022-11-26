using System.Collections.Generic;
using Common;
using Common.Extensions;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using static RMAZOR.Models.CommonInputCommandArg;

namespace RMAZOR.Views.Common
{
    public interface IViewSwitchLevelStageCommandInvoker
    {
        void SwitchLevelStage(
            EInputCommand              _SwitchStageCommand,
            Dictionary<string, object> _Arguments = null);
    }
    
    public class ViewSwitchLevelStageCommandInvoker : IViewSwitchLevelStageCommandInvoker
    {
        #region inject

        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }

        public ViewSwitchLevelStageCommandInvoker(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            Model             = _Model;
            CommandsProceeder = _CommandsProceeder;
        }

        #endregion

        #region api

        public void SwitchLevelStage(
            EInputCommand              _SwitchStageCommand,
            Dictionary<string, object> _Arguments = null)
        {
            var newArguments = SupplementNewArgumentsByPrevious(_Arguments);
            switch (_SwitchStageCommand)
            {
                case EInputCommand.ReadyToStartLevel:
                    SwitchLevelStageReadyToStartLevel(newArguments);
                    break;
                case EInputCommand.LoadNextLevel:
                    SwitchLevelStageLoadNextLevel(newArguments);
                    break;
                case EInputCommand.LoadLevelByIndex:
                    SwitchLevelStageLoadLevelByIndex(newArguments);
                    break;
                case EInputCommand.StartUnloadingLevel:
                    SwitchLevelStageStartUnloadingLevel(newArguments);
                    break;
                case EInputCommand.StartOrContinueLevel:
                case EInputCommand.PauseLevel:
                case EInputCommand.UnPauseLevel:
                case EInputCommand.KillCharacter:
                case EInputCommand.FinishLevel:
                case EInputCommand.UnloadLevel:
                    CommandsProceeder.RaiseCommand(_SwitchStageCommand, newArguments, true);
                    break;
            }
        }

        #endregion

        #region nonpublic methods

        private Dictionary<string, object> SupplementNewArgumentsByPrevious(Dictionary<string, object> _NewArgs)
        {
            var prevArguments = Model.LevelStaging.Arguments ?? new Dictionary<string, object>();
            var newArguments = _NewArgs ?? new Dictionary<string, object>();
            foreach ((string key, object value) in prevArguments)
            {
                if (newArguments.ContainsKey(key))
                    continue;
                newArguments.Add(key, value);
            }
            return newArguments;
        }

        private string GetCurrentLevelType(Dictionary<string, object> _Arguments)
        {
            string currentLevelType = (string)_Arguments.GetSafe(
                KeyCurrentLevelType, out _);
            return currentLevelType;
        }
        
        private string GetNextLevelType(Dictionary<string, object> _Arguments)
        {
            string nextLevelType = (string)_Arguments.GetSafe(
                KeyNextLevelType, out _);
            return nextLevelType;
        }

        private void SwitchLevelStageReadyToStartLevel(Dictionary<string, object> _Args)
        {
            string nextLevelType = GetNextLevelType(_Args);
            _Args.Remove(KeyNextLevelType);
            _Args.RemoveSafe(KeyLoadFirstLevelInGroup, out _);
            string currentLevelType = nextLevelType;
            currentLevelType ??= ParameterLevelTypeMain;
            _Args.SetSafe(KeyCurrentLevelType, currentLevelType);
            CommandsProceeder.RaiseCommand(EInputCommand.ReadyToStartLevel, _Args, true);
        }

        private void SwitchLevelStageLoadNextLevel(Dictionary<string, object> _Args)
        {
            object loadFirstLevelInGroupArg = _Args.GetSafe(KeyLoadFirstLevelInGroup, out bool keyExist);
            if (keyExist && (bool) loadFirstLevelInGroupArg)
            {
                SwitchLevelStageLoadFirstLevelFromCurrentGroup(_Args);
                return;
            }
            string currentLevelType = GetCurrentLevelType(_Args);
            string nextLevelType = GetNextLevelType(_Args);
            switch (currentLevelType)
            {
                case ParameterLevelTypeMain:
                    switch (nextLevelType)
                    {
                        case ParameterLevelTypeMain:
                            CommandsProceeder.RaiseCommand(EInputCommand.LoadNextLevel, _Args, true);
                            break;
                        case ParameterLevelTypeBonus:
                            SwitchLevelStageLoadLevelByIndex(_Args);
                            break;
                    }
                    break;
                case ParameterLevelTypeBonus:
                    SwitchLevelStageLoadLevelByIndex(_Args);
                    break;
            }
        }
        
        private void SwitchLevelStageLoadFirstLevelFromCurrentGroup(
            Dictionary<string, object> _Args)
        {
            long currentLevelIndex = Model.LevelStaging.LevelIndex;
            string currentLevelType = GetCurrentLevelType(_Args);
            long nextLevelIndex = 0;
            switch (currentLevelType)
            {
                case ParameterLevelTypeMain:
                    long levelIndexInGroup = RmazorUtils.GetIndexInGroup(currentLevelIndex);
                    nextLevelIndex = currentLevelIndex - levelIndexInGroup;
                    break;
                case ParameterLevelTypeBonus:
                    int nextLevelGroupIndex = (int) currentLevelIndex + 1;
                    nextLevelIndex = RmazorUtils.GetFirstLevelInGroup(nextLevelGroupIndex);
                    break;
            }
            _Args.SetSafe(KeyLevelIndex, nextLevelIndex);
            CommandsProceeder.RaiseCommand(EInputCommand.LoadLevelByIndex, _Args, true);
        }

        private void SwitchLevelStageLoadLevelByIndex(Dictionary<string, object> _Args)
        {
            long currentLevelIndex = Model.LevelStaging.LevelIndex;
            string nextLevelType = GetNextLevelType(_Args);
            long nextLevelIndex = nextLevelType switch
            {
                ParameterLevelTypeBonus => 
                    RmazorUtils.GetLevelsGroupIndex(currentLevelIndex) - 1,
                ParameterLevelTypeMain =>
                    RmazorUtils.GetFirstLevelInGroup((int) currentLevelIndex + 1) +
                    RmazorUtils.GetLevelsInGroup((int) currentLevelIndex + 1),
                _ => currentLevelIndex
            };
            _Args.SetSafe(KeyLevelIndex, nextLevelIndex);
            CommandsProceeder.RaiseCommand(EInputCommand.LoadLevelByIndex, _Args, true);
        }

        private void SwitchLevelStageStartUnloadingLevel(Dictionary<string, object> _Args)
        {
            string source = (string)_Args.GetSafe(KeySource, out _);
            string currentLevelType = GetCurrentLevelType(_Args);
            _Args.SetSafe(KeyLoadFirstLevelInGroup, false);
            switch (source)
            {
                case ParameterScreenTap:
                    _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeMain);
                    break;
                case ParameterFinishLevelGroupPanel:
                    bool currentLevelIsBonus = currentLevelType == ParameterLevelTypeBonus;
                    string nextLevelType = currentLevelIsBonus
                        ? ParameterLevelTypeMain
                        : ParameterLevelTypeBonus;
                    _Args.SetSafe(KeyNextLevelType, nextLevelType);
                    break;
                case ParameterPlayBonusLevelPanel:
                    int levelsGroup = RmazorUtils.GetLevelsGroupIndex(Model.LevelStaging.LevelIndex);
                    int bonusLevelIndex = levelsGroup - 1;
                    _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeBonus);
                    _Args.SetSafe(KeyLevelIndex, bonusLevelIndex);
                    break;
                case ParameterCharacterDiedPanel:
                    _Args.SetSafe(KeyLoadFirstLevelInGroup, true);
                    _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeMain);
                    break;
                case ParameterLevelSkipper:
                    Dbg.Log("!!! 3");
                    _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeMain);
                    break;
            }
            CommandsProceeder.RaiseCommand(EInputCommand.StartUnloadingLevel, _Args, true);
        }

        #endregion
    }
}