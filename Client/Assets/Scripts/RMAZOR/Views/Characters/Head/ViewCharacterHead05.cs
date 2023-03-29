using System;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHead05 : IViewCharacterHead { }

    public class ViewCharacterHead05
        : ViewCharacterHeadWithBorderObjectBase,
          IViewCharacterHead05
    {
        #region nonpublic members
        
        private Rectangle m_Eye;

        private Disc
            m_HeadShape,
            m_BorderShape1;
        
        private Line 
            m_MouthLine1,
            m_MouthLine2;

        #endregion

        #region inject

        private ViewCharacterHead05(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _AppearTransitioner,
            IViewInputCommandsProceeder _CommandsProceeder) 
            : base(
                _ViewSettings, 
                _ColorProvider,
                _ContainersGetter, 
                _PrefabSetManager,
                _CoordinateConverter, 
                _AppearTransitioner,
                _CommandsProceeder) { }
        
        #endregion

        #region nonpublic methods

        public override string Id => "05";

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Character:
                    m_HeadShape.SetColor(_Color);
                    break;
                case ColorIds.Character2:
                    m_BorderShape1.SetColor(_Color);
                    m_Eye         .SetColor(_Color);
                    m_MouthLine1  .SetColor(_Color);
                    m_MouthLine2  .SetColor(_Color);
                    break;
            }
        }
        
        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = GetCharacterGameObject();
            const int sortingOrder = SortingOrders.Character;
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            m_HeadShape = go.GetCompItem<Disc>("head")
                .SetColor(ColorProvider.GetColor(ColorIds.Character))
                .SetSortingOrder(sortingOrder);
            m_BorderShape1 = go.GetCompItem<Disc>("border")
                .SetSortingOrder(sortingOrder + 1)
                .SetColor(charCol2);
            m_Eye = go.GetCompItem<Rectangle>("eye")
                .SetSortingOrder(sortingOrder + 1)
                .SetColor(charCol2);
            m_MouthLine1 = go.GetCompItem<Line>("mouth_line_1")
                .SetSortingOrder(sortingOrder)
                .SetColor(charCol2);
            m_MouthLine2 = go.GetCompItem<Line>("mouth_line_2")
                .SetSortingOrder(sortingOrder)
                .SetColor(charCol2);
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            m_HeadShape.enabled    = _Active;
            m_BorderShape1.enabled = _Active;
            m_Eye.enabled          = _Active;
            m_MouthLine1.enabled   = _Active;
            m_MouthLine2.enabled   = _Active;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol = ColorProvider.GetColor(ColorIds.Character);
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new Component[] {m_HeadShape}, () => charCol},
                {new Component[] {m_BorderShape1, m_Eye, m_MouthLine1, m_MouthLine2}, () => charCol2},
            }; 
            return sets;
        }

        #endregion
    }
}