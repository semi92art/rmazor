using System;
using System.Runtime.CompilerServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using RMAZOR.Views.Common.ViewMazeMoneyItems;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathItemMoneyGetter : IInit, ICloneable
    {
        IViewMazeItemPathItemMoney GetItem();
    }
    
    public class ViewMazeItemPathItemMoneyGetter : InitBase, IViewMazeItemPathItemMoneyGetter
    {
        #region inject

        private ViewSettings                     ViewSettings        { get; }
        private IViewMazeItemPathItemMoneyDisc   PathItemMoneyDisc   { get; }
        private IViewMazeItemPathItemMoneySquare PathItemMoneySquare { get; }

        private ViewMazeItemPathItemMoneyGetter(
            ViewSettings                     _ViewSettings,
            IViewMazeItemPathItemMoneyDisc   _PathItemMoneyDisc,
            IViewMazeItemPathItemMoneySquare _PathItemMoneySquare)
        {
            ViewSettings        = _ViewSettings;
            PathItemMoneyDisc   = _PathItemMoneyDisc;
            PathItemMoneySquare = _PathItemMoneySquare;
        }

        #endregion

        #region api

        public IViewMazeItemPathItemMoney GetItem()
        {
            return ViewSettings.pathItemContentShapeType switch
            {
                "disc"   => PathItemMoneyDisc,
                "square" => PathItemMoneySquare,
                _        => throw new SwitchExpressionException(ViewSettings.pathItemContentShapeType)
            };
        }
        
        public object Clone()
        {
            return new ViewMazeItemPathItemMoneyGetter(
                ViewSettings,
                PathItemMoneyDisc.Clone() as IViewMazeItemPathItemMoneyDisc,
                PathItemMoneySquare.Clone() as IViewMazeItemPathItemMoneySquare);
        }

        #endregion



    }
}