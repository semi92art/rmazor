using System.Collections.Generic;
using mazing.common.Runtime.Extensions;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageSwitchers
{
    public interface IViewLevelStageSwitcherReadyToStartLevel 
        : IViewLevelStageSwitcherSingleStage { }
    
    public class ViewLevelStageSwitcherReadyToStartLevel : IViewLevelStageSwitcherReadyToStartLevel
    {
        #region inject
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IModelGame                  Model             { get; }

        public ViewLevelStageSwitcherReadyToStartLevel(
            IViewInputCommandsProceeder _CommandsProceeder,
            IModelGame                  _Model)
        {
            CommandsProceeder = _CommandsProceeder;
            Model             = _Model;
        }

        #endregion

        #region api

        public void SwitchLevelStage(Dictionary<string, object> _Args)
        {
            if (Model.LevelStaging.LevelStage == ELevelStage.None)
                return;
            string nextLevelType = ViewLevelStageSwitcherUtils.GetNextLevelType(_Args);
            _Args.Remove(KeyNextLevelType);
            _Args.RemoveSafe(KeyLoadFirstLevelInGroup, out _);
            string currentLevelType = nextLevelType;
            currentLevelType ??= ParameterLevelTypeDefault;
            _Args.SetSafe(KeyCurrentLevelType, currentLevelType);
            CommandsProceeder.RaiseCommand(EInputCommand.ReadyToStartLevel, _Args, true);
        }

        #endregion
    }
}