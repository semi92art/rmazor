using System;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHeadMouthCommon
        : IViewCharacterHeadPart ,
          IViewCharacterHeadPartExtended { }
    
    public class ViewCharacterHeadMouthCommon 
        : ViewCharacterHeadPartBase,
          IViewCharacterHeadMouthCommon
    {
        #region nonpublic members

        private Line m_MouthLine1, m_MouthLine2;

        #endregion

        #region inject

        private ViewCharacterHeadMouthCommon(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _AppearTransitioner)
            : base(_ViewSettings, _ColorProvider, _AppearTransitioner) { }

        #endregion
        
        #region api

        void IViewCharacterHeadPartExtended.ActivateShapes(bool _Activate)
        {
            ActivateShapes(_Activate);
        }

        Dictionary<IEnumerable<Component>, Func<Color>> IViewCharacterHeadPartExtended.GetAppearSets(bool _Appear)
        {
            return GetAppearSets(_Appear);
        }

        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Character2:
                    m_MouthLine1.SetColor(_Color);
                    m_MouthLine2.SetColor(_Color);
                    break;
            }
        }

        protected override void InitPrefab()
        {
            const int sortingOrder = SortingOrders.Character + 1;
            var color = ColorProvider.GetColor(ColorIds.Character2);
            var go = GetCharacterGameObject();
            m_MouthLine1 = go.GetCompItem<Line>("mouth_line_1")
                .SetSortingOrder(sortingOrder)
                .SetColor(color);
            m_MouthLine2 = go.GetCompItem<Line>("mouth_line_2")
                .SetSortingOrder(sortingOrder)
                .SetColor(color);
            m_MouthLine1.enabled = m_MouthLine2.enabled = false;
        }

        protected override void UpdatePrefab() { }

        protected override void ActivateShapes(bool _Active)
        {
            m_MouthLine1.enabled = _Active;
            m_MouthLine2.enabled = _Active;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            return new Dictionary<IEnumerable<Component>, Func<Color>>
                {{new Component[] {m_MouthLine1, m_MouthLine2}, () => charCol2}};
        }

        #endregion


    }
}