using System;
using mazing.common.Runtime;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.Common.ViewMazeMoneyItems
{
    public interface IViewMazeItemPathItemMoney : 
        IInit, 
        IOnLevelStageChanged,
        ICloneable,
        IAppear
    {
        event UnityAction       Collected;
        bool                    IsCollected { get; set; }
        Func<ViewMazeItemProps> GetProps    { set; }
        Transform               Parent      { set; }
        void                    InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent);
        void                    Collect(bool                      _Collect);
        void                    UpdateShape();
        void                    EnableInitializedShapes(bool _Enable);
    }
}