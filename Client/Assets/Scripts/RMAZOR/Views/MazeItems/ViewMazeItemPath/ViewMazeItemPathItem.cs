using System;
using mazing.common.Runtime.Enums;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathItem :
        ICloneable,
        IAppear, 
        IOnLevelStageChanged 
    {
        bool                       IsCollected { get; }
        IViewMazeItemPathItemMoney PathItemMoney { get; }
        Component[]                Renderers         { get; }
        void                       InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent);
        void                       UpdateShape();
        void                       Collect(bool                 _Collect);
        void                       EnableInitializedShapes(bool _Enable);
    }
    
    public class ViewMazeItemPathItem : IViewMazeItemPathItem
    {
        #region nonpublic members

        private IViewMazeItemPathItemIdle  PathItemIdle => PathItemIdleGetter.GetItem();
        
        #endregion
        
        #region inject

        private IViewMazeItemPathItemIdleGetter  PathItemIdleGetter  { get; }
        private IViewMazeItemPathItemMoneyGetter PathItemMoneyGetter { get; }
        
        private ViewMazeItemPathItem(
            IViewMazeItemPathItemIdleGetter  _PathItemIdleGetter,
            IViewMazeItemPathItemMoneyGetter _PathItemMoneyGetter)
        {
            PathItemIdleGetter  = _PathItemIdleGetter;
            PathItemMoneyGetter = _PathItemMoneyGetter;
        }

        #endregion

        #region api
        
        public IViewMazeItemPathItemMoney PathItemMoney => PathItemMoneyGetter.GetItem();
        
        public bool IsCollected => PathItemIdle.IsCollected;
        
        public EAppearingState AppearingState => PathItemIdle.AppearingState;
        
        public object Clone() =>
            new ViewMazeItemPathItem(
                PathItemIdleGetter.Clone() as IViewMazeItemPathItemIdleGetter,
                PathItemMoneyGetter.Clone() as IViewMazeItemPathItemMoneyGetter);

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            PathItemMoney.OnLevelStageChanged(_Args);
        }

        public void Appear(bool _Appear)
        {
            PathItemIdle.Appear(_Appear);
            PathItemMoney.Appear(_Appear);
        }
        
        public Component[] Renderers => PathItemIdle.Renderers;

        public void InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent)
        {
            PathItemIdle.InitShape(_GetProps, _Parent);
            PathItemMoney.InitShape(_GetProps, _Parent);
        }

        public void UpdateShape()
        {
            PathItemIdle.UpdateShape();
            PathItemMoney.UpdateShape();
        }

        public void Collect(bool _Collect)
        {
            PathItemIdle.Collect(_Collect);
            PathItemMoney.Collect(_Collect);
        }

        public void EnableInitializedShapes(bool _Enable)
        {
            PathItemIdle.EnableInitializedShapes(_Enable);
            PathItemMoney.EnableInitializedShapes(_Enable);
        }

        #endregion
    }
}