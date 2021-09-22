﻿using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public sealed class ViewMazeSpringboardItemsGroup : ViewMazeItemsGroupBase, IViewMazeSpringboardItemsGroup
    {
        public ViewMazeSpringboardItemsGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.Springboard};
        
        public void OnSpringboardEvent(SpringboardEventArgs _Args)
        {
            var item = Common.GetItem<IViewMazeItemSpringboard>(_Args.Item);
            item.MakeJump(_Args);
        }

    }
}