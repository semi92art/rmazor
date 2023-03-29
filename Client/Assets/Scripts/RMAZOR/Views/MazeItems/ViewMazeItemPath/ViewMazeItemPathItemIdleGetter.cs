using System;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathItemIdleGetter : IInit, ICloneable, IOnLevelStageChanged
    {
        IViewMazeItemPathItemIdle GetItem();
    }
    
    public class ViewMazeItemPathItemIdleGetter : InitBase, IViewMazeItemPathItemIdleGetter
    {
        #region nonpublic members

        private int m_PathItemContentShapeTypeHash;
        private int 
            m_PathItemHashDisc,
            m_PathItemHashSquare,
            m_PathItemHashEmptyDisc,
            m_PathItemHashEmptySquare;
        
        #endregion
        
        #region inject

        private ViewSettings                    ViewSettings       { get; }
        private IViewMazeItemPathItemIdleDisc   PathItemIdleDisc   { get; }
        private IViewMazeItemPathItemIdleSquare PathItemIdleSquare { get; }
        private IViewMazeItemPathItemIdleEmpty  PathItemIdleEmpty  { get; }

        private ViewMazeItemPathItemIdleGetter(
            ViewSettings                    _ViewSettings,
            IViewMazeItemPathItemIdleDisc   _PathItemIdleDisc,
            IViewMazeItemPathItemIdleSquare _PathItemIdleSquare,
            IViewMazeItemPathItemIdleEmpty  _PathItemIdleEmpty)
        {
            ViewSettings       = _ViewSettings;
            PathItemIdleDisc   = _PathItemIdleDisc;
            PathItemIdleSquare = _PathItemIdleSquare;
            PathItemIdleEmpty  = _PathItemIdleEmpty;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            m_PathItemHashDisc        = Animator.StringToHash("disc");
            m_PathItemHashSquare      = Animator.StringToHash("square");
            m_PathItemHashEmptyDisc   = Animator.StringToHash("empty_disc");
            m_PathItemHashEmptySquare = Animator.StringToHash("empty_square");
            base.Init();
        }

        public IViewMazeItemPathItemIdle GetItem()
        {
            if (!Initialized)
                Init();
            if (m_PathItemContentShapeTypeHash == 0)
                m_PathItemContentShapeTypeHash = Animator.StringToHash(ViewSettings.pathItemContentShapeType);
            int hash = m_PathItemContentShapeTypeHash;
            if (hash == m_PathItemHashDisc)        return PathItemIdleDisc;
            if (hash == m_PathItemHashSquare)      return PathItemIdleSquare;
            if (hash == m_PathItemHashEmptyDisc)   return PathItemIdleEmpty;
            if (hash == m_PathItemHashEmptySquare) return PathItemIdleEmpty;
            return PathItemIdleDisc;
        }
        
        public object Clone()
        {
            return new ViewMazeItemPathItemIdleGetter(
                ViewSettings,
                PathItemIdleDisc  .Clone() as IViewMazeItemPathItemIdleDisc,
                PathItemIdleSquare.Clone() as IViewMazeItemPathItemIdleSquare,
                PathItemIdleEmpty .Clone() as IViewMazeItemPathItemIdleEmpty);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {

        }

        #endregion
    }
}