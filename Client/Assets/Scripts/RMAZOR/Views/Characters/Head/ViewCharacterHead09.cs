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
    public interface IViewCharacterHead09 : IViewCharacterHead { }

    public class ViewCharacterHead09 
        : ViewCharacterHeadWithBorderObjectBase,
          IViewCharacterHead09
    {
        #region nonpublic members
        
        private Disc      m_Body1, m_Border1;
        private Rectangle m_Body2, m_Border2;
        
        #endregion

        #region inject
        
        private IViewCharacterHeadBodyCommon  BodyCommon  { get; }
        private IViewCharacterHeadEyesCommon  EyesCommon  { get; }

        protected ViewCharacterHead09(
            ViewSettings                  _ViewSettings,
            IColorProvider                _ColorProvider,
            IContainersGetter             _ContainersGetter,
            IPrefabSetManager             _PrefabSetManager,
            ICoordinateConverter          _CoordinateConverter,
            IRendererAppearTransitioner   _AppearTransitioner,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewCharacterHeadBodyCommon  _BodyCommon,
            IViewCharacterHeadEyesCommon  _EyesCommon) 
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
        }
        
        #endregion

        #region api

        public override string Id => "09";

        public override void Init()
        {
            if (Initialized)
                return;
            BodyCommon.GetCharacterGameObject = GetCharacterGameObject;
            EyesCommon.GetCharacterGameObject = GetCharacterGameObject;
            base.Init();
            BodyCommon.Init();
            EyesCommon.Init();
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
            m_Body1   = go.GetCompItem<Disc>("body_1");
            m_Border1 = go.GetCompItem<Disc>("border_1");
            m_Body2   = go.GetCompItem<Rectangle>("body_2");
            m_Border2 = go.GetCompItem<Rectangle>("border_2");
        }

        protected override void ActivateShapes(bool _Active)
        {
            foreach (var renderer in GetLocalRenderers())
                renderer.enabled = _Active;
            BodyCommon .ActivateShapes(_Active);
            EyesCommon .ActivateShapes(_Active);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets1 = new Dictionary<IEnumerable<Component>, Func<Color>>
            {{GetLocalRenderers(), () => charCol2}};
            var sets2 = BodyCommon.GetAppearSets(_Appear)
                .Concat(EyesCommon.GetAppearSets(_Appear));
            return sets1.Concat(sets2).ToDictionary(
                _Set => _Set.Key, 
                _Set => _Set.Value);
        }

        private IEnumerable<ShapeRenderer> GetLocalRenderers()
        {
            return new ShapeRenderer[] {m_Body1, m_Border1, m_Body2, m_Border2};
        }

        #endregion
    }
}