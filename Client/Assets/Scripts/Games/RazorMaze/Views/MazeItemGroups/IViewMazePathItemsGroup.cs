using System.Collections.Generic;
using Entities;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazePathItemsGroup : IInit, IOnLevelStageChanged, ICharacterMoveStarted, ICharacterMoveFinished
    {
        List<IViewMazeItemPath> PathItems { get; }
        void OnPathProceed(V2Int _PathItem);
    }
}