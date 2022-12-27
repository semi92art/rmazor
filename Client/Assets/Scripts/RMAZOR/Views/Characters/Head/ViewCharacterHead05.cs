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
    public interface IViewCharacterHead05 : IViewCharacterHead { }

    public class ViewCharacterHead05 
        : ViewCharacterHeadRectangleWithEyesAndMouthBase, 
          IViewCharacterHead05
    {
        #region nonpublic members

        protected override string PrefabName => "character_head_05";
        
        private Disc m_Arc1, m_Arc2;

        #endregion

        #region inject

        private ViewCharacterHead05(
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
                    m_Arc1.SetColor(_Color);
                    m_Arc2.SetColor(_Color);
                    break;
            }
        }
        
        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = PrefabObj;
            m_Arc1 = go.GetCompItem<Disc>("arc_1").SetSortingOrder(SortingOrders.Character + 1);
            m_Arc2 = go.GetCompItem<Disc>("arc_2").SetSortingOrder(SortingOrders.Character + 1);
            m_Arc1.enabled = m_Arc2.enabled = false;
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            base.ActivateShapes(_Active);
            m_Arc1.enabled = _Active;
            m_Arc2.enabled = _Active;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets = base.GetAppearSets(_Appear);
            sets.Add(new Component[] {m_Arc1, m_Arc2}, () => charCol2);
            return base.GetAppearSets(_Appear);
        }

        #endregion
    }
}