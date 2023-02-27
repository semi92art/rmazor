using System.Collections.Generic;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.Common.ViewLevelStageSwitchers
{
    public interface IViewLevelStageSwitcher
    {
        void SwitchLevelStage(
            EInputCommand              _SwitchStageCommand,
            Dictionary<string, object> _Arguments = null);
    }
    
    public class ViewLevelStageSwitcher : IViewLevelStageSwitcher
    {
        #region inject

        private IModelGame                                 Model                       { get; }
        private IViewInputCommandsProceeder                CommandsProceeder           { get; }
        private IViewLevelStageSwitcherLoadLevel           SwitcherLoadLevel           { get; }
        private IViewLevelStageSwitcherReadyToStartLevel   SwitcherReadyToStartLevel   { get; }
        private IViewLevelStageSwitcherStartUnloadingLevel SwitcherStartUnloadingLevel { get; }

        private ViewLevelStageSwitcher(
            IModelGame                                 _Model,
            IViewInputCommandsProceeder                _CommandsProceeder,
            IViewLevelStageSwitcherLoadLevel           _SwitcherLoadLevel,
            IViewLevelStageSwitcherReadyToStartLevel   _SwitcherReadyToStartLevel,
            IViewLevelStageSwitcherStartUnloadingLevel _SwitcherStartUnloadingLevel)
        {
            Model                       = _Model;
            CommandsProceeder           = _CommandsProceeder;
            SwitcherLoadLevel           = _SwitcherLoadLevel;
            SwitcherReadyToStartLevel   = _SwitcherReadyToStartLevel;
            SwitcherStartUnloadingLevel = _SwitcherStartUnloadingLevel;
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
                case EInputCommand.ExitLevelStaging:
                    CommandsProceeder.RaiseCommand(_SwitchStageCommand, newArguments, true);
                    break;
                case EInputCommand.LoadLevel:
                    SwitcherLoadLevel.SwitchLevelStage(newArguments);
                    break;
                case EInputCommand.ReadyToStartLevel:
                    SwitcherReadyToStartLevel.SwitchLevelStage(newArguments);
                    break;
                case EInputCommand.StartUnloadingLevel:
                    SwitcherStartUnloadingLevel.SwitchLevelStage(newArguments);
                    break;
                case EInputCommand.StartOrContinueLevel:
                case EInputCommand.PauseLevel:
                case EInputCommand.UnPauseLevel:
                case EInputCommand.KillCharacter:
                case EInputCommand.FinishLevel:
                case EInputCommand.UnloadLevel:
                    if (Model.LevelStaging.LevelStage == ELevelStage.None)
                        break;
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

        #endregion
    }
}