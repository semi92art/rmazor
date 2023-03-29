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
    public interface IViewCharacterHeadEyesCommon 
        : IViewCharacterHeadPart,
          IViewCharacterHeadPartExtended { }

    public class ViewCharacterHeadEyesCommon : ViewCharacterHeadPartBase, IViewCharacterHeadEyesCommon
    {
        #region nonpublic members
        
        private Rectangle m_Eye1Shape, m_Eye2Shape;

        #endregion

        #region inject

        private ViewCharacterHeadEyesCommon(
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
                case ColorIds.Character2:
                    m_Eye1Shape.SetColor(_Color);
                    m_Eye2Shape.SetColor(_Color);
                    break;
            }
        }

        protected override void InitPrefab()
        {
            var go = GetCharacterGameObject();
            m_Eye1Shape  = go.GetCompItem<Rectangle>("eye_1").SetSortingOrder(SortingOrders.Character + 1);
            m_Eye2Shape  = go.GetCompItem<Rectangle>("eye_2").SetSortingOrder(SortingOrders.Character + 1);
            m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
        }

        protected override void UpdatePrefab() { }

        protected override void ActivateShapes(bool _Active)
        {
            m_Eye1Shape.enabled = _Active;
            m_Eye2Shape.enabled = _Active;
        }
        
        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {{new Component[] {m_Eye1Shape, m_Eye2Shape}, () => charCol2}};
        }

        #endregion
    }
}