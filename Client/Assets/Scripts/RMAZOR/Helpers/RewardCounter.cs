using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.Utils;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Helpers
{
    public interface IRewardCounter : IInit, IOnLevelStageChanged
    {
        int                    CurrentLevelXp         { get; }
        int                    CurrentLevelGroupXp    { get; }
        int                    CurrentLevelMoney      { get; set; }
        int                    CurrentLevelGroupMoney { get; }
    }
    
    public class RewardCounter : InitBase, IRewardCounter
    {
        #region nonpublic members

        private float m_NewMoneyMultiplyCoefficient;
        private int   m_CurrentLevelMoney;

        #endregion

        #region api

        public int CurrentLevelXp { get; private set; }

        public int CurrentLevelGroupXp { get; private set; }

        public int CurrentLevelMoney
        {
            get => m_CurrentLevelMoney;
            set => m_CurrentLevelMoney = Mathf.RoundToInt(value * m_NewMoneyMultiplyCoefficient);
        }

        public int CurrentLevelGroupMoney { get; private set; }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            string gameMode         = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string currentLevelType = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded when _Args.PreviousStage == ELevelStage.None && gameMode == ParameterGameModeMain:
                    m_NewMoneyMultiplyCoefficient = SaveUtils.GetValue(SaveKeysRmazor.MultiplyNewCoinsCoefficient);
                    if (m_NewMoneyMultiplyCoefficient < 1f)
                    {
                        m_NewMoneyMultiplyCoefficient = 1f;
                        SaveUtils.PutValue(SaveKeysRmazor.MultiplyNewCoinsCoefficient, 1f);
                    }
                    CurrentLevelGroupXp    = SaveUtils.GetValue(SaveKeysRmazor.CurrentLevelGroupXp);
                    CurrentLevelGroupMoney = SaveUtils.GetValue(SaveKeysRmazor.CurrentLevelGroupMoney);
                    break;
                case ELevelStage.Finished:
                    if (gameMode != ParameterGameModeMain && gameMode != ParameterGameModePuzzles)
                        break;
                    CurrentLevelGroupMoney += CurrentLevelMoney;
                    CurrentLevelXp = RmazorUtils.CalculateLevelXp(_Args.LevelIndex, gameMode, currentLevelType);
                    CurrentLevelGroupXp += CurrentLevelXp;
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    SaveUtils.PutValue(SaveKeysRmazor.CurrentLevelGroupXp, CurrentLevelGroupXp);
                    SaveUtils.PutValue(SaveKeysRmazor.CurrentLevelGroupMoney, CurrentLevelGroupMoney);
                    break;
                case ELevelStage.Unloaded:
                    CurrentLevelMoney = CurrentLevelXp = 0;
                    if (MustResetCurrentLevelGroupRewardsOnLevelUnloaded(_Args))
                        CurrentLevelGroupMoney = CurrentLevelGroupXp = 0;
                    break;
                case ELevelStage.None when _Args.PreviousStage != ELevelStage.None:
                    CurrentLevelGroupXp    = 0;
                    CurrentLevelGroupMoney = 0;
                    break;
            }
        }

        private static bool MustResetCurrentLevelGroupRewardsOnLevelUnloaded(LevelStageArgs _Args)
        {
            string gameMode      = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string thisLevelType = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            string nextLevelType = ViewLevelStageSwitcherUtils.GetNextLevelType(_Args.Arguments);
            return gameMode switch
            {
                ParameterGameModeMain => (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex)
                                          && nextLevelType != ParameterLevelTypeBonus)
                                         || thisLevelType == ParameterLevelTypeBonus,
                _ => true
            };
        }
        
        #endregion
    }
}