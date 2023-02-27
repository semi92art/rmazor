using System;
using System.Runtime.CompilerServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathItemIdleGetter : IInit, ICloneable
    {
        IViewMazeItemPathItemIdle GetItem();
    }
    
    public class ViewMazeItemPathItemIdleGetter : InitBase, IViewMazeItemPathItemIdleGetter
    {
        #region inject

        private ViewSettings                    ViewSettings       { get; }
        private IViewMazeItemPathItemIdleDisc   PathItemIdleDisc   { get; }
        private IViewMazeItemPathItemIdleSquare PathItemIdleSquare { get; }

        private ViewMazeItemPathItemIdleGetter(
            ViewSettings                    _ViewSettings,
            IViewMazeItemPathItemIdleDisc   _PathItemIdleDisc,
            IViewMazeItemPathItemIdleSquare _PathItemIdleSquare)
        {
            ViewSettings       = _ViewSettings;
            PathItemIdleDisc   = _PathItemIdleDisc;
            PathItemIdleSquare = _PathItemIdleSquare;
        }

        #endregion

        #region api

        public IViewMazeItemPathItemIdle GetItem()
        {
            return ViewSettings.pathItemContentShapeType switch
            {
                "disc"   => PathItemIdleDisc,
                "square" => PathItemIdleSquare,
                _        => throw new SwitchExpressionException(ViewSettings.pathItemContentShapeType)
            };
        }
        
        public object Clone()
        {
            return new ViewMazeItemPathItemIdleGetter(
                ViewSettings,
                PathItemIdleDisc.Clone() as IViewMazeItemPathItemIdleDisc,
                PathItemIdleSquare.Clone() as IViewMazeItemPathItemIdleSquare);
        }

        #endregion



    }
}