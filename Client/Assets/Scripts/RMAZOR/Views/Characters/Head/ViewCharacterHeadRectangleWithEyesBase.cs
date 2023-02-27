using System;
using System.Collections.Generic;
using Common;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;
using Common.Extensions;
using mazing.common.Runtime.Extensions;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public abstract class ViewCharacterHeadRectangleWithEyesBase : ViewCharacterHeadRectangleBase
    {
        private Rectangle m_Eye1Shape, m_Eye2Shape;
    
        protected ViewCharacterHeadRectangleWithEyesBase(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _AppearTransitioner) 
            : base(
                _ViewSettings, 
                _ColorProvider,
                _ContainersGetter, 
                _PrefabSetManager,
                _CoordinateConverter,
                _AppearTransitioner) { }
                
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            switch (_ColorId)
            {
                case ColorIds.Character2:
                    m_Eye1Shape  .SetColor(_Color);
                    m_Eye2Shape  .SetColor(_Color);
                    break;
            }
        }

        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = PrefabObj;
            m_Eye1Shape  = go.GetCompItem<Rectangle>("eye_1").SetSortingOrder(SortingOrders.Character + 1);
            m_Eye2Shape  = go.GetCompItem<Rectangle>("eye_2").SetSortingOrder(SortingOrders.Character + 1);
            m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            base.ActivateShapes(_Active);
            m_Eye1Shape.enabled  = _Active;
            m_Eye2Shape.enabled  = _Active;
        }
        
        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets = base.GetAppearSets(_Appear);
            sets.Add(
                new Component[] {m_Eye1Shape, m_Eye2Shape},
                () => charCol2);
            return sets;
        }
    }
}