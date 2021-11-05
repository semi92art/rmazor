using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeShredingerBlocksGroup : 
        IViewMazeItemGroup,
        ICharacterMoveFinished
    {
        void OnShredingerBlockEvent(ShredingerBlockArgs _Args);
    }
    
    public class ViewMazeShredingerBlocksGroup : ViewMazeItemsGroupBase, IViewMazeShredingerBlocksGroup
    {
        public ViewMazeShredingerBlocksGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.ShredingerBlock};
        
        public void OnShredingerBlockEvent(ShredingerBlockArgs _Args)
        {
            var item = Common.GetItem<IViewMazeItemShredingerBlock>(_Args.Info);
            item.BlockClosed = _Args.Stage == ShredingerBlocksProceeder.StageClosed;
            Dbg.Log(item.BlockClosed);
        }

        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            if (!_Args.ShredingerBlockPosWhoStopped.HasValue)
                return;
            GetItems()
                .Cast<IViewMazeItemShredingerBlock>()
                .SingleOrDefault(_Item => _Item.Props.Position == _Args.ShredingerBlockPosWhoStopped.Value)
                ?.OnCharacterMoveFinished(_Args);
        }
    }
}