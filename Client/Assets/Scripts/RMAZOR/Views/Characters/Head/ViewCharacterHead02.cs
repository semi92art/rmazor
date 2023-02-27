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
    public interface IViewCharacterHead02 : IViewCharacterHead { }

    public class ViewCharacterHead02
        : VIewCharacterHeadWithEyesAndMouthBase,
          IViewCharacterHead02
    {
        #region nonpublic members

        private Triangle
            m_Body1,  
            m_Border1;

        protected override string PrefabName => "character_head_02";
        
        #endregion

        #region inject

        protected ViewCharacterHead02(
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
                case ColorIds.Character:
                    m_Body1.SetColor(_Color);
                    break;
                case ColorIds.Character2:
                    m_Border1.SetColor(_Color);
                    break;
            }
        }

        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = PrefabObj;
            m_Body1 = go.GetCompItem<Triangle>("body_1")
                .SetSortingOrder(SortingOrders.Character - 2)
                .SetColor(ColorProvider.GetColor(ColorIds.Character));
            m_Border1 = go.GetCompItem<Triangle>("border_1")
                .SetSortingOrder(SortingOrders.Character - 1)
                .SetColor(ColorProvider.GetColor(ColorIds.Character2));
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            base.ActivateShapes(_Active);
            m_Body1.enabled   = _Active;
            m_Border1.enabled = _Active;
        }
        
        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol1 = ColorProvider.GetColor(ColorIds.Character);
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets = base.GetAppearSets(_Appear);
            sets.Add(new Component[] {m_Body1}, () => charCol1);
            sets.Add(new Component[] {m_Border1}, () => charCol2);
            return sets;
        }

        #endregion
    }
}