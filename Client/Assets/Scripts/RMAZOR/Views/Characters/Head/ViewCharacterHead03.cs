using System;
using System.Collections.Generic;
using System.Linq;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHead03 : IViewCharacterHead { }

    public class ViewCharacterHead03 
        : ViewCharacterHeadWithBorderObjectBase, 
          IViewCharacterHead03
    {
        #region nonpublic members
        
        #endregion

        #region inject
        
        private IViewCharacterHeadBodyCommon  BodyCommon  { get; }
        private IViewCharacterHeadEyesCommon  EyesCommon  { get; }
        private IViewCharacterHeadMouthCommon MouthCommon { get; }

        protected ViewCharacterHead03(
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

        public override string Id => "03";

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

        protected override void OnColorChanged(int _ColorId, Color _Color) { }

        protected override void ActivateShapes(bool _Active)
        {
            BodyCommon .ActivateShapes(_Active);
            EyesCommon .ActivateShapes(_Active);
            MouthCommon.ActivateShapes(_Active);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            return BodyCommon.GetAppearSets(_Appear)
                .Concat(EyesCommon.GetAppearSets(_Appear))
                .Concat(MouthCommon.GetAppearSets(_Appear))
                .ToDictionary(
                    _Set => _Set.Key, 
                    _Set => _Set.Value);
        }

        #endregion
    }
}