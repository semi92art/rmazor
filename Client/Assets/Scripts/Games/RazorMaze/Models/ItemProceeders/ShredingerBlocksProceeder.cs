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
        public bool Opened { get; }

        public ShredingerBlockArgs(MazeItem _Item, int _Stage, bool _Opened)
        {
            Item = _Item;
            Stage = _Stage;
            Opened = _Opened;
        }
    }

    public delegate void ShredingerBlockHandler(ShredingerBlockArgs _Args);

    public interface IShredingerBlocksProceeder : ICharacterMoveContinued, IOnMazeChanged
    {
        event ShredingerBlockHandler ShredingerBlockEvent;
    }
    
    
    public class ShredingerBlocksProceeder : ItemsProceederBase, IShredingerBlocksProceeder
    {
        #region nonpublic members
        
        protected override EMazeItemType[] Types => new[] {EMazeItemType.ShredingerBlock};
        
        #endregion
        
        #region inject
        
        public ShredingerBlocksProceeder(ModelSettings _Settings, IModelMazeData _Data) 
            : base(_Settings, _Data) { }
        
        #endregion
        
        #region api
        
        public event ShredingerBlockHandler ShredingerBlockEvent;

        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            foreach (var type in Types)
            {
                var infos = GetProceedInfos(type);
                foreach (var info in infos.Values.Where(_Info => !_Info.IsProceeding))
                {
                    if (info.Item.Position == _Args.Current)
                    {
                        SwitchStage(info, true, 1);
                    }
                }
                
                foreach (var info in infos.Values.Where(_Info => _Info.IsProceeding))
                {
                    if (info.Item.Position != _Args.Current && info.ProceedingStage == 1)
                    {
                        SwitchStage(info, true, 2);
                        Coroutines.Run(ProceedBlock(info));
                    }
                }
            }
        }

        #endregion
        
        #region nonpublic methods

        private IEnumerator ProceedBlock(IMazeItemProceedInfo _Info)
        {
            yield return Coroutines.Delay(
                () => SwitchStage(_Info, false, 0),
                Settings.shredingerBlockProceedTime);
        }

        private void SwitchStage(IMazeItemProceedInfo _Info, bool _IsProceeding, int _Stage)
        {
            _Info.IsProceeding = _IsProceeding;
            _Info.ProceedingStage = _Stage;
            ShredingerBlockEvent?.Invoke(
                new ShredingerBlockArgs(_Info.Item, _Stage, _Stage != 2));
        }
        
        #endregion
    }
}