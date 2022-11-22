using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.Common
{
    public interface IViewSwitchLevelStageCommandInvoker
    {
        void SwitchLevelStage(
            EInputCommand              _SwitchStageCommand,
            bool                       _UsePreviousArgs,
            Dictionary<string, object> _Arguments = null);
    }
    
    public class ViewSwitchLevelStageCommandInvoker : IViewSwitchLevelStageCommandInvoker
    {
        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }

        public ViewSwitchLevelStageCommandInvoker(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            Model = _Model;
            CommandsProceeder = _CommandsProceeder;
        }
        
        public void SwitchLevelStage(
            EInputCommand              _SwitchStageCommand,
            bool                       _UsePreviousArgs,
            Dictionary<string, object> _Arguments = null)
        {
            long levelIndex = Model.LevelStaging.LevelIndex;
            var prevArguments = Model.LevelStaging.Arguments ?? new Dictionary<string, object>();
            var newArguments = _Arguments ?? new Dictionary<string, object>();
            string nextLevelType = (string) prevArguments.GetSafe(
                CommonInputCommandArg.KeyNextLevelType, out _);
            if (_UsePreviousArgs)
            {
                foreach ((string key, object value) in prevArguments)
                {
                    if (newArguments.ContainsKey(key))
                        continue;
                    newArguments.Add(key, value);
                }
            }
            switch (_SwitchStageCommand)
            {
                case EInputCommand.ReadyToStartLevel:
                    switch (nextLevelType)
                    {
                        case CommonInputCommandArg.ParameterLevelTypeBonus:
                            newArguments.Remove(CommonInputCommandArg.KeyNextLevelType);
                            newArguments.SetSafe(
                                CommonInputCommandArg.KeyCurrentLevelType,
                                CommonInputCommandArg.ParameterLevelTypeBonus);
                            break;
                        case CommonInputCommandArg.ParameterLevelTypeMain:
                            newArguments.Remove(CommonInputCommandArg.KeyNextLevelType);
                            newArguments.SetSafe(
                                CommonInputCommandArg.KeyCurrentLevelType,
                                CommonInputCommandArg.ParameterLevelTypeMain);
                            break;
                    }
                    CommandsProceeder.RaiseCommand(_SwitchStageCommand, newArguments, true);
                    break;
                case EInputCommand.LoadFirstLevelFromCurrentGroup:
                    if (nextLevelType == CommonInputCommandArg.ParameterLevelTypeMain)
                    {
                        int nextLevelGroupIndex = (int) levelIndex + 1;
                        levelIndex = RmazorUtils.GetFirstLevelInGroup(nextLevelGroupIndex) +
                                     RmazorUtils.GetLevelsInGroup(nextLevelGroupIndex);
                        newArguments.SetSafe(CommonInputCommandArg.KeyLevelIndex, levelIndex);
                        CommandsProceeder.RaiseCommand(EInputCommand.LoadLevelByIndex, newArguments, true);
                        return;
                    }
                    CommandsProceeder.RaiseCommand(_SwitchStageCommand, newArguments, true);
                    break;
                case EInputCommand.LoadNextLevel:
                    CommandsProceeder.RaiseCommand(_SwitchStageCommand, newArguments, true);
                    break;
                case EInputCommand.LoadLevelByIndex:
                    levelIndex = nextLevelType switch
                    {
                        CommonInputCommandArg.ParameterLevelTypeBonus => 
                            RmazorUtils.GetLevelsGroupIndex(levelIndex) - 1,
                        CommonInputCommandArg.ParameterLevelTypeMain =>
                            RmazorUtils.GetFirstLevelInGroup((int) levelIndex + 1) +
                            RmazorUtils.GetLevelsInGroup((int) levelIndex + 1),
                        _ => levelIndex
                    };
                    newArguments.SetSafe(CommonInputCommandArg.KeyLevelIndex, levelIndex);
                    CommandsProceeder.RaiseCommand(_SwitchStageCommand, newArguments, true);
                    break;
                case EInputCommand.StartOrContinueLevel:
                case EInputCommand.PauseLevel:
                case EInputCommand.UnPauseLevel:
                case EInputCommand.KillCharacter:
                case EInputCommand.FinishLevel:
                    // string currentLevelType = (string) prevArguments.GetSafe(
                    //     CommonInputCommandArg.KeyCurrentLevelType, out _);
                    // if (currentLevelType == CommonInputCommandArg.ParamenterLevelTypeBonus)
                    //     newArguments.SetSafe(CommonInputCommandArg.KeyCurrentLevelType);
                    // if (prevArguments.Contains("bonus_level"))
                    // {
                    //     newArguments = newArguments?
                    //         .RemoveRange(new[] {"bonus_level"})
                    //         .Concat(new[] {"bonus_level"})
                    //         .ToArray();
                    // }
                    CommandsProceeder.RaiseCommand(_SwitchStageCommand, newArguments, true);
                    break;
                case EInputCommand.StartUnloadingLevel:
                {
                    // if (prevArguments.Contains("bonus_level"))
                    // {
                    //     newArguments = newArguments?
                    //         .Concat(new[] {"bonus_level"})
                    //         .Distinct()
                    //         .ToArray();
                    // }
                    CommandsProceeder.RaiseCommand(_SwitchStageCommand, newArguments, true);
                }
                    break;
                case EInputCommand.UnloadLevel:
                    CommandsProceeder.RaiseCommand(_SwitchStageCommand, newArguments, true);
                    break;
            }
        }
    }
}