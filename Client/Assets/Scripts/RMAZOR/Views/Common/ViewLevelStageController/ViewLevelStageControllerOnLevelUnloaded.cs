using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Managers.PlatformGameServices;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelUnloaded
    {
        void OnLevelUnloaded(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnLevelUnloaded : IViewLevelStageControllerOnLevelUnloaded
    {
        private IScoreManager                       ScoreManager                   { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private IViewGameTicker                     ViewGameTicker                 { get; }

        public ViewLevelStageControllerOnLevelUnloaded(
            IScoreManager                       _ScoreManager,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            IViewGameTicker                     _ViewGameTicker)
        {
            ScoreManager                   = _ScoreManager;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
            ViewGameTicker = _ViewGameTicker;
        }
        
        public void OnLevelUnloaded(LevelStageArgs _Args)
        {
            var scoreEntity = ScoreManager.GetScoreFromLeaderboard(DataFieldIds.Level, false);
            Cor.Run(Cor.WaitWhile(
                () => scoreEntity.Result == EEntityResult.Pending,
                () =>
                {
                    switch (scoreEntity.Result)
                    {
                        case EEntityResult.Pending:
                            Dbg.LogWarning("Timeout when getting score from leaderboard");
                            return;
                        case EEntityResult.Fail:
                            Dbg.LogError("Failed to get score from leaderboard");
                            return;
                        case EEntityResult.Success:
                        {
                            var score = scoreEntity.GetFirstScore();
                            if (!score.HasValue)
                            {
                                Dbg.LogError("Failed to get score from leaderboard");
                                return;
                            }
                            Dbg.Log("Level score from server leaderboard: " + score.Value);
                            ScoreManager.SetScoreToLeaderboard(
                                DataFieldIds.Level, 
                                score.Value + 1, 
                                false);
                            break;
                        }
                        default:
                            throw new SwitchCaseNotImplementedException(scoreEntity.Result);
                    }
                },
                _Seconds: 3f,
                _Ticker: ViewGameTicker));
            if (SaveUtils.GetValue(SaveKeysRmazor.AllLevelsPassed))
            {
                // TODO сделать отдельное сообщение на окончание всех уровней
                var args = new Dictionary<string, object> {{CommonInputCommandArg.KeyLevelIndex, 0}};
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.LoadLevelByIndex, args);
            }
            else
            {
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.LoadNextLevel);
            }
        }
    }
}