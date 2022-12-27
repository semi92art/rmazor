using System;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public abstract class ViewCharacterHeadRectangleBase : ViewCharacterHeadBase
    {
        private Rectangle m_HeadShape, m_BorderShape;
    
        protected ViewCharacterHeadRectangleBase(
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
            switch (_ColorId)
            {
                case ColorIds.Character:  m_HeadShape.SetColor(_Color);    break;
                case ColorIds.Character2: m_BorderShape.SetColor(_Color); break;
            }
        }

        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = PrefabObj;
            m_HeadShape    = go.GetCompItem<Rectangle>("head shape")
                .SetColor(ColorProvider.GetColor(ColorIds.Character))
                .SetSortingOrder(SortingOrders.Character);
            m_BorderShape  = go.GetCompItem<Rectangle>("border")
                .SetSortingOrder(SortingOrders.Character + 1);
            m_HeadShape.enabled = m_BorderShape.enabled = false;
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            m_HeadShape.enabled    = _Active;
            m_BorderShape.enabled  = _Active;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol = ColorProvider.GetColor(ColorIds.Character);
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new Component[] {m_HeadShape}, () => charCol},
                {new Component[] {m_BorderShape}, () => charCol2},
            };
        }
    }
}