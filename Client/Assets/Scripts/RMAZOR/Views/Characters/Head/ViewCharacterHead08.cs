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
    public interface IViewCharacterHead08 : IViewCharacterHead { }

    public class ViewCharacterHead08
        : ViewCharacterHeadBase,
          IViewCharacterHead08
    {
        #region nonpublic members

        protected override string PrefabName => "character_head_10";

        private Rectangle m_Eye;
        private Disc
            m_HeadShape,
            m_BorderShape1,
            m_BorderShape2,
            m_BorderShape3;

        #endregion

        #region inject

        private ViewCharacterHead08(
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
            switch (_ColorId)
            {
                case ColorIds.Character:
                    m_HeadShape.SetColor(_Color);
                    break;
                case ColorIds.Character2:
                    m_BorderShape1.SetColor(_Color);
                    m_BorderShape2.SetColor(_Color);
                    m_BorderShape3.SetColor(_Color);
                    m_Eye         .SetColor(_Color);
                    break;
            }
        }
        
        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = PrefabObj;
            const int sortingOrder = SortingOrders.Character;
            m_HeadShape = go.GetCompItem<Disc>("head")
                .SetColor(ColorProvider.GetColor(ColorIds.Character))
                .SetSortingOrder(sortingOrder);
            m_BorderShape1 = go.GetCompItem<Disc>("border_1").SetSortingOrder(sortingOrder + 1);
            m_BorderShape2 = go.GetCompItem<Disc>("border_2").SetSortingOrder(sortingOrder + 1);
            m_BorderShape3 = go.GetCompItem<Disc>("border_3").SetSortingOrder(sortingOrder + 1);
            m_Eye          = go.GetCompItem<Rectangle>("eye").SetSortingOrder(sortingOrder + 1);
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            m_HeadShape.enabled    = _Active;
            m_BorderShape1.enabled = _Active;
            m_BorderShape2.enabled = _Active;
            m_BorderShape3.enabled = _Active;
            m_Eye.enabled          = _Active;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol = ColorProvider.GetColor(ColorIds.Character);
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new Component[] {m_HeadShape}, () => charCol},
                {new Component[] {m_BorderShape1, m_BorderShape2, m_BorderShape3, m_Eye}, () => charCol2},
            }; 
            return sets;
        }

        #endregion
    }
}