using System.Collections.Generic;
using Common.Helpers;
using mazing.common.Runtime.Extensions;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageSwitchers
{
    public interface IViewLevelStageSwitcherLoadLevel
        : IViewLevelStageSwitcherSingleStage { }
    
    public class ViewLevelStageSwitcherLoadLevel : IViewLevelStageSwitcherLoadLevel
    {
        private GlobalGameSettings          GlobalGameSettings { get; }
        private IModelGame                  Model              { get; }
        private IViewInputCommandsProceeder CommandsProceeder  { get; }

        public ViewLevelStageSwitcherLoadLevel(
            GlobalGameSettings          _GlobalGameSettings,
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            GlobalGameSettings = _GlobalGameSettings;
            Model              = _Model;
            CommandsProceeder  = _CommandsProceeder;
        }
        
        public void SwitchLevelStage(Dictionary<string, object> _Args)
        {
            string gameMode = (string) _Args.GetSafe(KeyGameMode, out _);
            switch (gameMode)
            {
                case ParameterGameModeMain: LoadLevelGameModeMain(_Args); break;
                default:                    LoadLevelByIndex(_Args);      break;
            }
        }

        private void LoadLevelGameModeMain(Dictionary<string, object> _Args)
        {
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(_Args);
            if (gameMode == ParameterGameModeMain)
                SwitchLevelStageLoadNextLevelGameModeMain(_Args);
            else LoadLevelByIndex(_Args);
        }
        
        private void SwitchLevelStageLoadNextLevelGameModeMain(Dictionary<string, object> _Args)
        {
            object loadFirstLevelInGroupArg = _Args.GetSafe(KeyLoadFirstLevelInGroup, out bool argKeyExist);
            if (argKeyExist && (bool) loadFirstLevelInGroupArg)
            {
                SwitchLevelStageLoadFirstLevelFromCurrentGroupInGameModeMain(_Args);
                return;
            }
            string source = (string) _Args.GetSafe(KeySource, out _);
            switch (source)
            {
                case ParameterSourcePlayBonusLevelPanel:
                    SwitchLevelStageLoadNextLevelGameModeMainSourcePlayBonusLevelPanel(_Args);
                    return;
                case ParameterSourceScreenTap:
                    SwitchLevelStageLoadNextLevelGameModeMainSourceScreenTap(_Args);
                    return;
                case ParameterSourceMainMenu:
                case ParameterSourceLevelsPanel:
                    LoadLevelByIndex(_Args);
                    return;
                default:
                    LoadLevelByIndex(_Args);
                    return;
            }
        }

        private void SwitchLevelStageLoadNextLevelGameModeMainSourcePlayBonusLevelPanel(
            Dictionary<string, object> _Args)
        {
            long nextBonusLevelAfterMainLevelIndex = ViewLevelStageSwitcherUtils
                .GetNextLevelOfBonusTypeAfterLevelOfDefaultTypeIndex(
                    Model.LevelStaging.LevelIndex, GlobalGameSettings.extraLevelEveryNStage);
            ViewLevelStageSwitcherUtils.SetLevelIndex(_Args, nextBonusLevelAfterMainLevelIndex);
            _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeBonus);
            LoadLevelByIndex(_Args);
        }

        private void SwitchLevelStageLoadNextLevelGameModeMainSourceScreenTap(Dictionary<string, object> _Args)
        {
            string currentLevelType = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args)
                                      ?? ParameterLevelTypeDefault;
            switch (currentLevelType)
            {
                case ParameterLevelTypeDefault:
                    string nextLevelType = ViewLevelStageSwitcherUtils.GetNextLevelType(_Args);
                    switch (nextLevelType)
                    {
                        case ParameterLevelTypeDefault:
                            ViewLevelStageSwitcherUtils.SetLevelIndex(_Args, Model.LevelStaging.LevelIndex + 1);
                            LoadLevelByIndex(_Args);
                            return;
                        case ParameterLevelTypeBonus:
                        {
                            long nextBonusLevelAfterMainLevelIndex = ViewLevelStageSwitcherUtils
                                .GetNextLevelOfBonusTypeAfterLevelOfDefaultTypeIndex(
                                    Model.LevelStaging.LevelIndex, GlobalGameSettings.extraLevelEveryNStage);
                            ViewLevelStageSwitcherUtils.SetLevelIndex(_Args, nextBonusLevelAfterMainLevelIndex);
                            _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeBonus);
                            LoadLevelByIndex(_Args);
                        }
                            return;
                    }

                    return;
                case ParameterLevelTypeBonus:
                    long nextDefaultLevelAfterBonusLevelIndex = ViewLevelStageSwitcherUtils
                        .GetNExtLevelOfDefaultTypeAfterLEvelOfBonusTypeIndex(
                            Model.LevelStaging.LevelIndex, GlobalGameSettings.extraLevelEveryNStage);
                    ViewLevelStageSwitcherUtils.SetLevelIndex(_Args, nextDefaultLevelAfterBonusLevelIndex);
                    _Args.SetSafe(KeyNextLevelType, ParameterLevelTypeDefault);
                    LoadLevelByIndex(_Args);
                    return;
            }
        }
        
        private void SwitchLevelStageLoadFirstLevelFromCurrentGroupInGameModeMain(
            Dictionary<string, object> _Args)
        {
            long currentLevelIndex = Model.LevelStaging.LevelIndex;
            string currentLevelType = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args);
            long nextLevelIndex = 0;
            switch (currentLevelType)
            {
                case ParameterLevelTypeDefault:
                    long levelIndexInGroup = RmazorUtils.GetIndexInGroup(currentLevelIndex);
                    nextLevelIndex = currentLevelIndex - levelIndexInGroup;
                    break;
                case ParameterLevelTypeBonus:
                    long firstLevelInCurrentGroup = RmazorUtils.GetFirstLevelInGroupIndex(
                        (int) currentLevelIndex * GlobalGameSettings.extraLevelEveryNStage + 1);
                    nextLevelIndex = firstLevelInCurrentGroup;
                    break;
            }
            ViewLevelStageSwitcherUtils.SetLevelIndex(_Args, nextLevelIndex);
            CommandsProceeder.RaiseCommand(EInputCommand.LoadLevel, _Args, true);
        }

        private void LoadLevelByIndex(Dictionary<string, object> _Args)
        {
            string source = (string) _Args.GetSafe(KeySource, out _);
            if (source == ParameterSourceLevelsPanel || source == ParameterSourceMainMenu)
            {
                CommandsProceeder.RaiseCommand(EInputCommand.LoadLevel, _Args, true);
                return;
            }
            // long nextLevelIndex = ViewLevelStageSwitcherUtils.GetLevelIndex(_Args);
            // ViewLevelStageSwitcherUtils.Set
            // _Args.SetSafe(KeyLevelIndex, nextLevelIndex);
            CommandsProceeder.RaiseCommand(EInputCommand.LoadLevel, _Args, true);
        }
    }
}