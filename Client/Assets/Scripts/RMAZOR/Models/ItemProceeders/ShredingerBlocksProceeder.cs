using System;
using System.Collections;
using System.Linq;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class ShredingerBlockArgs : EventArgs
    {
        public IMazeItemProceedInfo Info { get; }
        public int Stage { get; }

        public ShredingerBlockArgs(IMazeItemProceedInfo _Info, int _Stage)
        {
            Info = _Info;
            Stage = _Stage;
        }
    }

    public delegate void ShredingerBlockHandler(ShredingerBlockArgs _Args);

    public interface IShredingerBlocksProceeder :
        IItemsProceeder, 
        ICharacterMoveFinished,
        IGetAllProceedInfos
    {
        event ShredingerBlockHandler ShredingerBlockEvent;
    }

    public class ShredingerBlocksProceeder : ItemsProceederBase, IShredingerBlocksProceeder
    {
        #region constants
        
        public const int StageClosed = 1;
        
        #endregion

        #region inject
        
        public ShredingerBlocksProceeder(
            ModelSettings _Settings,
            IModelData _Data,
            IModelCharacter _Character,
            IModelGameTicker _GameTicker) 
            : base(_Settings, _Data, _Character, _GameTicker) { }
        
        #endregion
        
        #region api

        protected override EMazeItemType[]     Types => new[] {EMazeItemType.ShredingerBlock};
        public event ShredingerBlockHandler ShredingerBlockEvent;
        public Func<IMazeItemProceedInfo[]> GetAllProceedInfos { get; set; }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            var path = RazorMazeUtils.GetFullPath(_Args.From, _Args.To);
            foreach (var info in ProceedInfos.Where(_Info => _Info.IsProceeding))
            {
                if (!path.Contains(info.CurrentPosition))
                    continue;
                if (info.CurrentPosition == _Args.To)
                    continue;
                if (info.ProceedingStage != StageIdle)
                    continue;
                SwitchStage(info, StageClosed);
                ProceedCoroutine(info, ProceedBlock(info, StageIdle));
            }
        }

        #endregion
        
        #region nonpublic methods

        private IEnumerator ProceedBlock(IMazeItemProceedInfo _Info, int _Stage)
        {
            yield return Cor.Delay(
                Settings.ShredingerBlockProceedTime,
                () =>
                {
                    var gravityItems = GetAllProceedInfos()
                        .Where(_Inf => RazorMazeUtils.GravityItemTypes().ContainsAlt(_Inf.Type))
                        .ToList();
                    if (gravityItems.Any())
                    {
                        Cor.Run(Cor.WaitWhile(
                            () => gravityItems.Any(_Inf => _Inf.ProceedingStage != StageIdle),
                            () => SwitchStage(_Info, _Stage)));
                    }
                    else 
                        SwitchStage(_Info, _Stage);
                });
        }

        private void SwitchStage(IMazeItemProceedInfo _Info, int _Stage)
        {
            _Info.ProceedingStage = _Stage;
            ShredingerBlockEvent?.Invoke(
                new ShredingerBlockArgs(_Info, _Stage));
        }
        
        #endregion
    }
}