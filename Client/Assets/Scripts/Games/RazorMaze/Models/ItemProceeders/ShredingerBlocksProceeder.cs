using System;
using System.Collections;
using System.Linq;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class ShredingerBlockArgs : EventArgs
    {
        public MazeItem Item { get; }
        public int Stage { get; }
        public bool Opened { get; }

        public ShredingerBlockArgs(MazeItem _Item, int _Stage, bool _Opened)
        {
            Item = _Item;
            Stage = _Stage;
            Opened = _Opened;
        }
    }

    public delegate void ShredingerBlockHandler(ShredingerBlockArgs _Args);

    public interface IShredingerBlocksProceeder : IItemsProceeder, ICharacterMoveContinued
    {
        event ShredingerBlockHandler ShredingerBlockEvent;
    }
    
    
    public class ShredingerBlocksProceeder : ItemsProceederBase, IShredingerBlocksProceeder
    {
        #region constants

        public const int StageOpened = 1;
        public const int StageClosed = 2;
        
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
            var infos = GetProceedInfos(Types).Values;
            foreach (var info in infos.Where(_Info => !_Info.IsProceeding))
            {
                if (info.Item.Position == _Args.Position)
                {
                    SwitchStage(info, true, StageOpened);
                }
            }
            
            foreach (var info in infos.Where(_Info => _Info.IsProceeding))
            {
                if (info.Item.Position != _Args.Position && info.ProceedingStage == StageOpened)
                {
                    SwitchStage(info, true, StageClosed);
                    Coroutines.Run(ProceedBlock(info));
                }
            }
        }

        #endregion
        
        #region nonpublic methods

        private IEnumerator ProceedBlock(IMazeItemProceedInfo _Info)
        {
            yield return Coroutines.Delay(
                () => SwitchStage(_Info, false, StageIdle),
                Settings.shredingerBlockProceedTime);
        }

        private void SwitchStage(IMazeItemProceedInfo _Info, bool _IsProceeding, int _Stage)
        {
            _Info.IsProceeding = _IsProceeding;
            _Info.ProceedingStage = _Stage;
            ShredingerBlockEvent?.Invoke(
                new ShredingerBlockArgs(_Info.Item, _Stage, _Stage != StageClosed));
        }
        
        #endregion
    }
}