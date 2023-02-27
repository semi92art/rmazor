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
    public abstract class ViewCharacterHeadWithEyesBase : ViewCharacterHeadBase
    {
        #region nonpublic members

        private Rectangle m_Eye1Shape, m_Eye2Shape;

        #endregion
        
        #region inject

        protected ViewCharacterHeadWithEyesBase(
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

        #endregion
        
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
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
            const int sortingOrder = SortingOrders.Character + 1;
            var color = ColorProvider.GetColor(ColorIds.Character2);
            var go = PrefabObj;
            m_Eye1Shape = go.GetCompItem<Rectangle>("eye_1")
                .SetSortingOrder(sortingOrder)
                .SetColor(color);
            m_Eye2Shape  = go.GetCompItem<Rectangle>("eye_2")
                .SetSortingOrder(sortingOrder)
                .SetColor(color);
            m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            m_Eye1Shape.enabled  = _Active;
            m_Eye2Shape.enabled  = _Active;
        }
        
        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new Component[] {m_Eye1Shape, m_Eye2Shape}, () => charCol2}
            };
            return sets;
        }
    }
}