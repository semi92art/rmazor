using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.Helpers;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath.ExtraBorders
{
    public interface IViewMazeItemPathExtraBordersSet : IInit, ICloneable
    {
        IList<IViewMazeItemPathExtraBorders> GetSet();
    }
    
    public class ViewMazeItemPathExtraBordersSet : InitBase, IViewMazeItemPathExtraBordersSet
    {
        #region nonpublic members

        private readonly IList<IViewMazeItemPathExtraBorders> m_Set
            = new List<IViewMazeItemPathExtraBorders>();

        #endregion
        
        #region inject

        private ViewSettings                   ViewSettings  { get; }
        private IViewMazeItemPathExtraBorders2 ExtraBorders2 { get; }
        private IViewMazeItemPathExtraBorders1 ExtraBorders1 { get; }
        private IViewMazeItemPathExtraBorders3 ExtraBorders3 { get; }

        private ViewMazeItemPathExtraBordersSet(
            ViewSettings                   _ViewSettings,
            IViewMazeItemPathExtraBorders2 _ExtraBorders2,
            IViewMazeItemPathExtraBorders1 _ExtraBorders1, 
            IViewMazeItemPathExtraBorders3 _ExtraBorders3)
        {
            ViewSettings  = _ViewSettings;
            ExtraBorders2 = _ExtraBorders2;
            ExtraBorders1 = _ExtraBorders1;
            ExtraBorders3 = _ExtraBorders3;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            var extraBordersInUse = ViewSettings.extraBordersIndices
                .Split(',')
                .Select(_S => Convert.ToInt32(_S))
                .ToList();
            m_Set.Clear();
            foreach (var borders in extraBordersInUse.Select(_Idx =>
                (IViewMazeItemPathExtraBorders) (_Idx switch
            {
                1 => ExtraBorders2,
                2 => ExtraBorders1,
                3 => ExtraBorders3,
                _ => throw new SwitchExpressionException(_Idx)
            })))
            {
                m_Set.Add(borders);
            }
            base.Init();
        }

        public IList<IViewMazeItemPathExtraBorders> GetSet()
        {
            if (!Initialized)
                Init();
            return m_Set;
        }

        public object Clone() =>
            new ViewMazeItemPathExtraBordersSet(
                ViewSettings,
                ExtraBorders2.Clone() as IViewMazeItemPathExtraBorders2,
                ExtraBorders1.Clone() as IViewMazeItemPathExtraBorders1,
                ExtraBorders3.Clone() as IViewMazeItemPathExtraBorders3);

        #endregion
        

    }
}