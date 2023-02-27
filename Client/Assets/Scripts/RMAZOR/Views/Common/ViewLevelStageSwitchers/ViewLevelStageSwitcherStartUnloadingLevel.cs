using System.Collections.Generic;
using Common.Helpers;
using mazing.common.Runtime.Extensions;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageSwitchers
{
    public interface IViewLevelStageSwitcherStartUnloadingLevel 
        : IViewLevelStageSwitcherSingleStage { }
    
    public class ViewLevelStageSwitcherStartUnloadingLevel : IViewLevelStageSwitcherStartUnloadingLevel
    {
        #region inject
        private GlobalGameSettings          GlobalGameSettings { get; }
        private IModelGame                  Model              { get; }
        private IViewInputCommandsProceeder CommandsProceeder  { get; }

        public ViewLevelStageSwitcherStartUnloadingLevel(
            GlobalGameSettings          _GlobalGameSettings,
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            GlobalGameSettings = _GlobalGameSettings;
            Model              = _Model;
            CommandsProceeder  = _CommandsProceeder;
        }

        #endregion

        #region api

        public void SwitchLevelStage(Dictionary<string, object> _Args)
        {
            if (Model.LevelStaging.LevelStage == ELevelStage.None)
                return;
            string source = (string)_Args.GetSafe(KeySource, out _);
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(_Args);
            _Args.SetSafe(KeyLoadFirstLevelInGroup, false);
            switch (gameMode)
            {
                case ParameterGameModeMain:
                    switch (source)
                    {
                        case ParameterSourceScreenTap:
                        // case ParameterFinishLevelGroupPanel:
                        case ParameterSourceLevelSkipper:
                            _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeDefault);
                            break;
                        case ParameterSourcePlayBonusLevelPanel:
                            long bonusLevelIndex = ViewLevelStageSwitcherUtils.GetNextLevelOfBonusTypeAfterLevelOfDefaultTypeIndex(
                                Model.LevelStaging.LevelIndex, GlobalGameSettings.extraLevelEveryNStage);
                            _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeBonus);
                            _Args.SetSafe(KeyLevelIndex, bonusLevelIndex);
                            break;
                        case ParameterSourceCharacterDiedPanel:
                            _Args.SetSafe(KeyLoadFirstLevelInGroup, true);
                            _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeDefault);
                            break;
                        case ParameterSourceLevelsPanel:
                            break;
                    }
                    break;
                case ParameterGameModeDailyChallenge:
                    break;
                case ParameterGameModeRandom:
                case ParameterGameModePuzzles:
                case ParameterGameModeBigLevels:
                    _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeDefault);
                    break;
            }
            CommandsProceeder.RaiseCommand(EInputCommand.StartUnloadingLevel, _Args, true);
        }

        #endregion
    }
}