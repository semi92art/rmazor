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
    public interface IViewCharacterHead07 : IViewCharacterHead { }

    public class ViewCharacterHead07 
        : ViewCharacterHead06, 
          IViewCharacterHead07
    {
        #region nonpublic members
        
        private Disc m_Hair2;

        #endregion

        #region inject

        protected ViewCharacterHead07(
            ViewSettings                  _ViewSettings,
            IColorProvider                _ColorProvider,
            IContainersGetter             _ContainersGetter,
            IPrefabSetManager             _PrefabSetManager,
            ICoordinateConverter          _CoordinateConverter,
            IRendererAppearTransitioner   _AppearTransitioner,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewCharacterHeadBodyCommon  _BodyCommon,
            IViewCharacterHeadEyesCommon  _EyesCommon,
            IViewCharacterHeadMouthCommon _MouthCommon) 
            : base(
                _ViewSettings, 
                _ColorProvider,
                _ContainersGetter, 
                _PrefabSetManager,
                _CoordinateConverter, 
                _AppearTransitioner,
                _CommandsProceeder,
                _BodyCommon, 
                _EyesCommon, 
                _MouthCommon) { }
        
        #endregion

        #region api

        public override string Id => "07";

        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            switch (_ColorId)
            {
                case ColorIds.Character2:
                    m_Hair2.SetColor(_Color);
                    break;
            }
        }
        
        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = GetCharacterGameObject();
            m_Hair2 = go.GetCompItem<Disc>("hair_2").SetSortingOrder(SortingOrders.Character + 1);
            m_Hair2.enabled = false;
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            base.ActivateShapes(_Active);
            m_Hair2.enabled = _Active;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets = base.GetAppearSets(_Appear);
            sets.Add(new Component[] {m_Hair2}, () => charCol2);
            return base.GetAppearSets(_Appear);
        }

        #endregion
    }
}