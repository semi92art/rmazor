using System.Collections.Generic;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeSpringboardsGroup : IViewMazeItemGroup
    {
        void OnSpringboardEvent(SpringboardEventArgs _Args);
    }
    
    public sealed class ViewMazeSpringboardsGroup : ViewMazeItemsGroupBase, IViewMazeSpringboardsGroup
    {
        public ViewMazeSpringboardsGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.Springboard};
        
        public void OnSpringboardEvent(SpringboardEventArgs _Args)
        {
            var item = Common.GetItem<IViewMazeItemSpringboard>(_Args.Info);
            item.MakeJump(_Args);
        }

    }
}