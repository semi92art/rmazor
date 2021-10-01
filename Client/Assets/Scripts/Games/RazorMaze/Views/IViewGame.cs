﻿using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;

namespace Games.RazorMaze.Views
{
    public interface IViewGame :
        IPreInit, 
        IInit,
        IPostInit, 
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        IContainersGetter                  ContainersGetter { get; }
        IViewUI                            UI { get; }
        IInputConfigurator                 InputConfigurator { get; }
        IViewCharacter                     Character { get; }
        IViewMazeCommon                    Common { get; }
        IViewMazeBackground                Background { get; }
        IViewRotation                      Rotation { get; }
        IViewMazePathItemsGroup            PathItemsGroup { get; }
        IViewMazeMovingItemsGroup          MovingItemsGroup { get; }
        IViewMazeTrapsReactItemsGroup      TrapsReactItemsGroup { get; }
        IViewMazeTrapsIncreasingItemsGroup TrapsIncreasingItemsGroup { get; }
        IViewMazeTurretsGroup              TurretsGroup { get; }
        IViewMazePortalsGroup              PortalsGroup { get; }
        IViewMazeShredingerBlocksGroup     ShredingerBlocksGroup { get; }
        IViewMazeSpringboardItemsGroup     SpringboardItemsGroup { get; }
        
    }
}