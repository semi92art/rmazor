using System;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathItemMoneyGetter : IInit, ICloneable, IOnLevelStageChanged
    {
        IViewMazeItemPathItemMoney GetItem();
    }
    
    public class ViewMazeItemPathItemMoneyGetter : InitBase, IViewMazeItemPathItemMoneyGetter
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

        public IViewMazeItemPathItemMoney GetItem()
        {
            if (!Initialized)
                Init();
            if (m_PathItemContentShapeTypeHash == 0)
                m_PathItemContentShapeTypeHash = Animator.StringToHash(ViewSettings.pathItemContentShapeType);
            int hash = m_PathItemContentShapeTypeHash;
            if (hash == m_PathItemHashDisc)        return PathItemMoneyDisc;
            if (hash == m_PathItemHashSquare)      return PathItemMoneySquare;
            if (hash == m_PathItemHashEmptyDisc)   return PathItemMoneyDisc;
            if (hash == m_PathItemHashEmptySquare) return PathItemMoneySquare;
            return PathItemMoneyDisc;
        }
        
        public object Clone()
        {
            return new ViewMazeItemPathItemMoneyGetter(
                ViewSettings,
                PathItemMoneyDisc.Clone() as IViewMazeItemPathItemMoneyDisc,
                PathItemMoneySquare.Clone() as IViewMazeItemPathItemMoneySquare);
        }

        #endregion

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {

        }
    }
}