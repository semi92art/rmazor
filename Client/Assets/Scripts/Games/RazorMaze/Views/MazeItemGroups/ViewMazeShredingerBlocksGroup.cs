using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;

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
        }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (_Args.BlockWhoStopped == null || _Args.BlockWhoStopped.Type != EMazeItemType.ShredingerBlock)
                return;
            GetItems()
                .Cast<IViewMazeItemShredingerBlock>()
                .SingleOrDefault(_Item => _Item.Props.Position == _Args.BlockWhoStopped.StartPosition)
                ?.OnCharacterMoveFinished(_Args);
        }
    }
}