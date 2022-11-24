using System;
using System.Collections.Generic;
using Common.Helpers;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath.ExtraBorders
{
    public interface IViewMazeItemPathExtraBorders1 : IViewMazeItemPathExtraBorders { }
    
    public class ViewMazeItemPathExtraBorders1 : InitBase, IViewMazeItemPathExtraBorders1
    {
        public bool                                            Activated      { get; set; }
        public Func<GameObject>                                GetParent      { get; set; }
        public Func<ViewMazeItemProps>                         GetProps       { get; set; }
        public Func<Color>                                     GetBorderColor { get; set; }
        public Component[]                                     Renderers => new Component[0];
        
        public object Clone() => new ViewMazeItemPathExtraBorders1();

        public void HighlightBordersAndCorners()          { }
        public void DrawBorders()                         { }
        public void AdjustBorders()                       { }
        public void EnableInitializedShapes(bool _Enable) { }

        public void AdjustBordersOnCornerInitialization(bool _Right, bool _Up, bool _Inner) { }


        public Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            return new Dictionary<IEnumerable<Component>, Func<Color>>();
        }
    }
}