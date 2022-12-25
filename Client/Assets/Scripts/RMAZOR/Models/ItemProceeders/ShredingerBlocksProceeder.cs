using System;
using System.Collections;
using System.Linq;
using Common.Extensions;
using Common.Utils;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models.ItemProceeders.Additional;
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
        #region inject
        
        private ShredingerBlocksProceeder(
            ModelSettings      _Settings,
            IModelData         _Data,
            IModelCharacter    _Character,
            IModelGameTicker   _GameTicker,
            IModelMazeRotation _Rotation) 
            : base(
                _Settings,
                _Data,
                _Character,
                _GameTicker,
                _Rotation) { }
        
        #endregion
        
        #region api

        protected override EMazeItemType[]     Types => new[] {EMazeItemType.ShredingerBlock};
        public event ShredingerBlockHandler ShredingerBlockEvent;
        public Func<IMazeItemProceedInfo[]> GetAllProceedInfos { get; set; }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            var path = RmazorUtils.GetFullPath(_Args.From, _Args.To);
            foreach (var info in ProceedInfos.Where(_Info => _Info.IsProceeding))
            {
                if (!path.Contains(info.CurrentPosition))
                    continue;
                if (info.CurrentPosition == _Args.To)
                    continue;
                if (info.ProceedingStage != ModelCommonData.StageIdle)
                    continue;
                SwitchStage(info, ModelCommonData.ShredingerStageClosed);
                ProceedCoroutine(info, ProceedBlock(info, ModelCommonData.StageIdle));
            }
        }

        #endregion
        
        #region nonpublic methods

        private IEnumerator ProceedBlock(IMazeItemProceedInfo _Info, int _Stage)
        {
            yield return Cor.Delay(
                Settings.shredingerBlockProceedTime,
                GameTicker,
                () =>
                {
                    var gravityItems = GetAllProceedInfos()
                        .Where(_Inf => RmazorUtils.GravityItemTypes.ContainsAlt(_Inf.Type))
                        .ToList();
                    if (gravityItems.Any())
                    {
                        Cor.Run(Cor.WaitWhile(
                            () => gravityItems.Any(_Inf => _Inf.ProceedingStage != ModelCommonData.StageIdle),
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