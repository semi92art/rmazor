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
    public interface IViewCharacterHeadBodyCommon
        : IViewCharacterHeadPart,
          IViewCharacterHeadPartExtended { }
    
    public class ViewCharacterHeadBodyCommon 
        : ViewCharacterHeadPartBase, IViewCharacterHeadBodyCommon
    {
        #region nonpublic members
        
        private Rectangle m_HeadShape, m_BorderShape;

        #endregion

        #region inject
        
        protected ViewCharacterHeadBodyCommon(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _AppearTransitioner) 
            : base(
                _ViewSettings,
                _ColorProvider,
                _AppearTransitioner) { }

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
                case ColorIds.Character:  m_HeadShape.SetColor(_Color);   break;
                case ColorIds.Character2: m_BorderShape.SetColor(_Color); break;
            }
        }

        protected override void InitPrefab()
        {
            var go = GetCharacterGameObject();
            m_HeadShape = go.GetCompItem<Rectangle>("head shape")
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
        
        protected override void UpdatePrefab()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}