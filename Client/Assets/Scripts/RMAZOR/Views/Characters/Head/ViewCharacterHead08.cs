using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHead08 : IViewCharacterHead { }

    public class ViewCharacterHead08 
        : ViewCharacterHeadWithBorderObjectBase,
          IViewCharacterHead08
    {
        #region nonpublic members
        
        private Rectangle m_Glass1,      m_Glass2;
        private Line      m_RimLine1,    m_RimLine2, m_RimLine3, m_RimLine4;
        private Polygon   m_RimPolygon1, m_RimPolygon2;
        private Quad      m_RimQuad1;

        #endregion

        #region inject
        
        private IViewCharacterHeadBodyCommon  BodyCommon  { get; }
        private IViewCharacterHeadEyesCommon  EyesCommon  { get; }
        private IViewCharacterHeadMouthCommon MouthCommon { get; }

        protected ViewCharacterHead08(
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
                _CommandsProceeder)
        {
            BodyCommon  = _BodyCommon;
            EyesCommon  = _EyesCommon;
            MouthCommon = _MouthCommon;
        }
        
        #endregion

        #region api

        public override string Id => "08";

        public override void Init()
        {
            if (Initialized)
                return;
            BodyCommon .GetCharacterGameObject = GetCharacterGameObject;
            EyesCommon .GetCharacterGameObject = GetCharacterGameObject;
            MouthCommon.GetCharacterGameObject = GetCharacterGameObject;
            base.Init();
            BodyCommon .Init();
            EyesCommon .Init();
            MouthCommon.Init();
        }

        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Character2)
                return;
            foreach (var renderer in GetLocalRenderers())
                renderer.SetColor(_Color);
        }

        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = GetCharacterGameObject();
            m_Glass1      = go.GetCompItem<Rectangle>("glass_1");
            m_Glass2      = go.GetCompItem<Rectangle>("glass_2");
            m_RimLine1    = go.GetCompItem<Line>("rim_line_1");
            m_RimLine2    = go.GetCompItem<Line>("rim_line_2");
            m_RimLine3    = go.GetCompItem<Line>("rim_line_3");
            m_RimLine4    = go.GetCompItem<Line>("rim_line_4");
            m_RimPolygon1 = go.GetCompItem<Polygon>("rim_polygon_1");
            m_RimPolygon2 = go.GetCompItem<Polygon>("rim_polygon_2");
            m_RimQuad1    = go.GetCompItem<Quad>("rim_quad_1");
        }

        protected override void ActivateShapes(bool _Active)
        {
            foreach (var renderer in GetLocalRenderers())
                renderer.enabled = _Active;
            BodyCommon .ActivateShapes(_Active);
            EyesCommon .ActivateShapes(_Active);
            MouthCommon.ActivateShapes(_Active);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets1 = new Dictionary<IEnumerable<Component>, Func<Color>>
            {{GetLocalRenderers(), () => charCol2}};
            var sets2 = BodyCommon.GetAppearSets(_Appear)
                .Concat(EyesCommon.GetAppearSets(_Appear))
                .Concat(MouthCommon.GetAppearSets(_Appear));
            return sets1.Concat(sets2).ToDictionary(
                _Set => _Set.Key, 
                _Set => _Set.Value);
        }

        private IEnumerable<ShapeRenderer> GetLocalRenderers()
        {
            return new ShapeRenderer[]
            {
                m_Glass1, m_Glass2,
                m_RimLine1, m_RimLine2, m_RimLine3, m_RimLine4,
                m_RimPolygon1, m_RimPolygon2,
                m_RimQuad1
            };
        }

        #endregion
    }
}