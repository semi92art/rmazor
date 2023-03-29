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
        
        private Disc m_Hair;

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
                case ColorIds.Character2:
                    m_Hair.SetColor(_Color);
                    break;
            }
        }
        
        protected override void InitPrefab()
        {
            base.InitPrefab();
            var go = GetCharacterGameObject();
            m_Hair = go.GetCompItem<Disc>("hair").SetSortingOrder(SortingOrders.Character + 1);
            m_Hair.enabled = false;
        }
        
        protected override void ActivateShapes(bool _Active)
        {
            BodyCommon .ActivateShapes(_Active);
            EyesCommon .ActivateShapes(_Active);
            MouthCommon.ActivateShapes(_Active);
            m_Hair.enabled = _Active;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            var sets = EyesCommon.GetAppearSets(_Appear)
                .Concat(MouthCommon.GetAppearSets(_Appear))
                .ToDictionary(
                    _Set => _Set.Key, 
                    _Set => _Set.Value);
            sets.Add(new Component[] {m_Hair}, () => charCol2);
            return sets;
        }

        #endregion
    }
}