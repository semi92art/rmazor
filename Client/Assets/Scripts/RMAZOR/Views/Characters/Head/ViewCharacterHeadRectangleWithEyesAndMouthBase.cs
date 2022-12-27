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
    public abstract class ViewCharacterHeadRectangleWithEyesAndMouthBase
        : ViewCharacterHeadRectangleWithEyesBase
    {
        #region nonpublic members
        
        private Line m_MouthLine1, m_MouthLine2;

        #endregion

        #region inject

        protected ViewCharacterHeadRectangleWithEyesAndMouthBase(
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

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            switch (_ColorId)
            {
                case ColorIds.Character2:
                    m_MouthLine1 .SetColor(_Color);
                    m_MouthLine2 .SetColor(_Color);
                    break;
            }
        }
        
        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = PrefabObj;
            m_MouthLine1 = go.GetCompItem<Line>("mouth_line_1").SetSortingOrder(SortingOrders.Character + 1);
            m_MouthLine2 = go.GetCompItem<Line>("mouth_line_2").SetSortingOrder(SortingOrders.Character + 1);
            m_MouthLine1.enabled = m_MouthLine2.enabled = false;
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            base.ActivateShapes(_Active);
            m_MouthLine1.enabled = _Active;
            m_MouthLine2.enabled = _Active;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets = base.GetAppearSets(_Appear);
            sets.Add(
                new Component[] {m_MouthLine1, m_MouthLine2},
                () => charCol2);
            return sets;
        }

        #endregion
    }
}