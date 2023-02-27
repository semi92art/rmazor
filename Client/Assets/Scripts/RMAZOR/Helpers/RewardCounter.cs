using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
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

        private int m_CurrentLevelGroupXp;
        private int m_CurrentLevelGroupMoney;

        #endregion

        #region api

        public int CurrentLevelXp { get; private set; }

        public int CurrentLevelGroupXp
        {
            get => m_CurrentLevelGroupXp;
            private set
            {
                SaveUtils.PutValue(SaveKeysRmazor.CurrentLevelGroupXp, value);
                m_CurrentLevelGroupXp = value;
            }
        }
        public int CurrentLevelMoney { get; set; }

        public int CurrentLevelGroupMoney
        {
            get => m_CurrentLevelGroupMoney;
            private set
            {
                SaveUtils.PutValue(SaveKeysRmazor.CurrentLevelGroupMoney, value);
                m_CurrentLevelGroupMoney = value;
            }
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            string gameMode         = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string currentLevelType = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded when _Args.PreviousStage == ELevelStage.None && gameMode == ParameterGameModeMain:
                    m_CurrentLevelGroupXp    = SaveUtils.GetValue(SaveKeysRmazor.CurrentLevelGroupXp);
                    m_CurrentLevelGroupMoney = SaveUtils.GetValue(SaveKeysRmazor.CurrentLevelGroupMoney);
                    break;
                case ELevelStage.Finished:
                    if (gameMode != ParameterGameModeMain && gameMode != ParameterGameModePuzzles)
                        break;
                    CurrentLevelGroupMoney += CurrentLevelMoney;
                    CurrentLevelXp = RmazorUtils.CalculateLevelXp(_Args.LevelIndex, gameMode, currentLevelType);
                    CurrentLevelGroupXp += CurrentLevelXp;
                    break;
                case ELevelStage.Unloaded:
                    CurrentLevelMoney = CurrentLevelXp = 0;
                    if (MustResetCurrentLevelGroupRewardsOnLevelUnloaded(_Args))
                        CurrentLevelGroupMoney = CurrentLevelGroupXp = 0;
                    break;
                case ELevelStage.None when _Args.PreviousStage != ELevelStage.None:
                    m_CurrentLevelGroupXp    = 0;
                    m_CurrentLevelGroupMoney = 0;
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