using System;
using System.Collections;
using System.Linq;
using Games.RazorMaze.Models.ProceedInfos;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class ShredingerBlockArgs : EventArgs
    {
        public MazeItem Item { get; }
        public int Stage { get; }

        public ShredingerBlockArgs(MazeItem _Item, int _Stage)
        {
            Item = _Item;
            Stage = _Stage;
        }
    }

    public delegate void ShredingerBlockHandler(ShredingerBlockArgs _Args);

    public interface IShredingerBlocksProceeder : IItemsProceeder, ICharacterMoveContinued, ICharacterMoveFinished
    {
        event ShredingerBlockHandler ShredingerBlockEvent;
    }
    
    
    public class ShredingerBlocksProceeder : ItemsProceederBase, IShredingerBlocksProceeder
    {
        #region constants
        
        public const int StageClosed = 1;
        
        #endregion
        
        #region nonpublic members
        
        protected override EMazeItemType[] Types => new[] {EMazeItemType.ShredingerBlock};
        
        #endregion
        
        #region inject
        
        public ShredingerBlocksProceeder(ModelSettings _Settings, IModelMazeData _Data, IModelCharacter _Character) 
            : base(_Settings, _Data, _Character) { }
        
        #endregion
        
        #region api
        
        public event ShredingerBlockHandler ShredingerBlockEvent;

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            // TODO если понадобится более быстрая активация блоков шредингера во время движения
        }
        
        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            var infos = GetProceedInfos(Types).Values;
            var path = RazorMazeUtils.GetFullPath(_Args.From, _Args.To);
            foreach (var info in infos.Where(_Info => _Info.IsProceeding))
            {
                if (path.Contains(info.Item.Position)
                    && info.Item.Position != _Args.To
                    && info.ProceedingStage == StageIdle)
                {
                    SwitchStage(info, StageClosed);
                    Coroutines.Run(ProceedBlock(info, StageIdle));
                }
            }
        }

        #endregion
        
        #region nonpublic methods

        private IEnumerator ProceedBlock(IMazeItemProceedInfo _Info, int _Stage)
        {
            yield return Coroutines.Delay(
                () => SwitchStage(_Info, _Stage),
                Settings.shredingerBlockProceedTime);
        }

        private void SwitchStage(IMazeItemProceedInfo _Info, int _Stage)
        {
            _Info.ProceedingStage = _Stage;
            ShredingerBlockEvent?.Invoke(
                new ShredingerBlockArgs(_Info.Item, _Stage));
        }
        
        #endregion
    }
}