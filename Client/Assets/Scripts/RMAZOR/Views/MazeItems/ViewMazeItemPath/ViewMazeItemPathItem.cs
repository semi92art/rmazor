using System;
using Common.Enums;
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
        IViewMazeItemPathItemMoney ItemPathItemMoney { get; }
        Component[]                Renderers         { get; }
        void                       InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent);
        void                       UpdateShape();
        void                       Collect(bool                 _Collect);
        void                       EnableInitializedShapes(bool _Enable);
    }
    
    public class ViewMazeItemPathItem : IViewMazeItemPathItem
    {
        private IViewMazeItemPathItemIdle PathItemIdle { get; }

        public bool                       IsCollected       => PathItemIdle.IsCollected;
        public IViewMazeItemPathItemMoney ItemPathItemMoney { get; }

        private ViewMazeItemPathItem(
            IViewMazeItemPathItemIdle _PathItemIdle,
            IViewMazeItemPathItemMoney _ItemPathItemMoney)
        {
            PathItemIdle = _PathItemIdle;
            ItemPathItemMoney    = _ItemPathItemMoney;
        }

        public EAppearingState AppearingState => PathItemIdle.AppearingState;
        
        public object Clone() =>
            new ViewMazeItemPathItem(
                PathItemIdle.Clone() as IViewMazeItemPathItemIdle,
                ItemPathItemMoney.Clone() as IViewMazeItemPathItemMoney);

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            ItemPathItemMoney.OnLevelStageChanged(_Args);
        }

        public void Appear(bool _Appear)
        {
            PathItemIdle.Appear(_Appear);
            ItemPathItemMoney.Appear(_Appear);
        }
        
        public Component[] Renderers => PathItemIdle.Renderers;

        public void InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent)
        {
            PathItemIdle.InitShape(_GetProps, _Parent);
            ItemPathItemMoney.InitShape(_GetProps, _Parent);
        }

        public void UpdateShape()
        {
            PathItemIdle.UpdateShape();
            ItemPathItemMoney.UpdateShape();
        }

        public void Collect(bool _Collect)
        {
            PathItemIdle.Collect(_Collect);
            ItemPathItemMoney.Collect(_Collect);
        }

        public void EnableInitializedShapes(bool _Enable)
        {
            PathItemIdle.EnableInitializedShapes(_Enable);
            ItemPathItemMoney.EnableInitializedShapes(_Enable);
        }
    }
}