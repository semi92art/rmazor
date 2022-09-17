using System;
using Common.SpawnPools;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.Additional
{
    public interface IViewTurretBody : ICloneable, IAppear, IActivated
    {
        void        SetTurretContainer(GameObject _Container);
        void        Update(ViewMazeItemProps _Props);
        void        OpenBarrel(bool               _Open, bool _Instantly,         bool _Forced = false);
        void        HighlightBarrel(bool          _Open, bool _Instantly = false, bool _Forced = false);
    }
}