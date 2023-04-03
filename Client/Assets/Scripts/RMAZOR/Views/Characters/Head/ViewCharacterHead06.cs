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
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHead06 : IViewCharacterHead { }

    public class ViewCharacterHead06 
        : ViewCharacterHeadWithBorderObjectBase,
          IViewCharacterHead06
    {
        #region nonpublic members
        
        private Triangle m_Ear1Body,    m_Ear2Body;
        private Line     m_Ear1Border1, m_Ear1Border2, m_Ear2Border1, m_Ear2Border2;
        
        #endregion

        #region inject
        
        private IViewCharacterHeadBodyCommon  BodyCommon  { get; }
        private IViewCharacterHeadEyesCommon  EyesCommon  { get; }
        private IViewCharacterHeadMouthCommon MouthCommon { get; }

        protected ViewCharacterHead06(
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

        public override string Id => "06";

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
            switch (_ColorId)
            {
                case ColorIds.Character:
                    m_Ear1Body.SetColor(_Color);
                    m_Ear2Body.SetColor(_Color);
                    break;
                case ColorIds.Character2:
                    m_Ear1Border1.SetColor(_Color);
                    m_Ear1Border2.SetColor(_Color);
                    m_Ear2Border1.SetColor(_Color);
                    m_Ear2Border2.SetColor(_Color);
                    break;
            }
        }

        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = GetCharacterGameObject();
            const int sOrder = SortingOrders.Character;
            m_Ear1Body       = go.GetCompItem<Triangle>("ear_1_body").SetSortingOrder(sOrder + 1);
            m_Ear2Body       = go.GetCompItem<Triangle>("ear_2_body").SetSortingOrder(sOrder + 1);
            m_Ear1Border1    = go.GetCompItem<Line>("ear_1_border_1").SetSortingOrder(sOrder + 2);
            m_Ear1Border2    = go.GetCompItem<Line>("ear_1_border_2").SetSortingOrder(sOrder + 2);
            m_Ear2Border1    = go.GetCompItem<Line>("ear_2_border_1").SetSortingOrder(sOrder + 2);
            m_Ear2Border2    = go.GetCompItem<Line>("ear_2_border_2").SetSortingOrder(sOrder + 2);
        }

        protected override void ActivateShapes(bool _Active)
        {
            var localRenderers = new ShapeRenderer[]
            {m_Ear1Body, m_Ear2Body, m_Ear1Border1, m_Ear1Border2, m_Ear2Border1, m_Ear2Border2};
            foreach (var renderer in localRenderers)
                renderer.enabled = _Active;
            BodyCommon .ActivateShapes(_Active);
            EyesCommon .ActivateShapes(_Active);
            MouthCommon.ActivateShapes(_Active);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol  = ColorProvider.GetColor(ColorIds.Character);
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets1 = new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new [] {m_Ear1Body, m_Ear2Body}, () => charCol},
                {new [] {m_Ear1Border1, m_Ear1Border2, m_Ear2Border1, m_Ear2Border2}, () => charCol2}
            };
            var sets2 = BodyCommon.GetAppearSets(_Appear)
                .Concat(EyesCommon.GetAppearSets(_Appear))
                .Concat(MouthCommon.GetAppearSets(_Appear));
            return sets1.Concat(sets2).ToDictionary(
                _Set => _Set.Key, 
                _Set => _Set.Value);
        }


        #endregion
    }
}