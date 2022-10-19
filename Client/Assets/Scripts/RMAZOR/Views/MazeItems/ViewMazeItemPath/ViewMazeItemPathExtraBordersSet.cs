using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Helpers;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathExtraBordersSet : IInit, ICloneable
    {
        IList<IViewMazeItemPathExtraBorders> GetSet();
    }
    
    public class ViewMazeItemPathExtraBordersSet : InitBase, IViewMazeItemPathExtraBordersSet
    {
        #region nonpublic members

        private readonly IList<IViewMazeItemPathExtraBorders> m_Set = new List<IViewMazeItemPathExtraBorders>();

        #endregion
        
        #region inject

        private ViewSettings                   ViewSettings  { get; }
        private IViewMazeItemPathExtraBorders1 ExtraBorders1 { get; }
        private IViewMazeItemPathExtraBorders2 ExtraBorders2 { get; }
        private IViewMazeItemPathExtraBorders3 ExtraBorders3 { get; }
        
        public ViewMazeItemPathExtraBordersSet(
            ViewSettings                   _ViewSettings,
            IViewMazeItemPathExtraBorders1 _ExtraBorders1,
            IViewMazeItemPathExtraBorders2 _ExtraBorders2, 
            IViewMazeItemPathExtraBorders3 _ExtraBorders3)
        {
            ViewSettings = _ViewSettings;
            ExtraBorders1 = _ExtraBorders1;
            ExtraBorders2 = _ExtraBorders2;
            ExtraBorders3 = _ExtraBorders3;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            Dbg.Log("extra_borders_indices: " + ViewSettings.extraBordersIndices);
            var extraBordersInUse = ViewSettings.extraBordersIndices
                .Split(',')
                .Select(_S => Convert.ToInt32(_S))
                .ToList();
            m_Set.Clear();
            if (extraBordersInUse.Contains(1))
                m_Set.Add(ExtraBorders1);
            if (extraBordersInUse.Contains(2))
                m_Set.Add(ExtraBorders2);
            if (extraBordersInUse.Contains(3))
                m_Set.Add(ExtraBorders3);
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
                ExtraBorders1.Clone() as IViewMazeItemPathExtraBorders1,
                ExtraBorders2.Clone() as IViewMazeItemPathExtraBorders2,
                ExtraBorders3.Clone() as IViewMazeItemPathExtraBorders3);

        #endregion
        

    }
}