﻿using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTransitionTextureProviderPlayground
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTransitionTextureProviderPlayground
        : FullscreenTransitionTextureProviderSimpleBase, IFullscreenTransitionTextureProviderPlayground
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground - 1;
        protected override string MaterialAssetName => "transition_texture_material_playground";

        #endregion

        #region inject

        public FullscreenTransitionTextureProviderPlayground(
            IPrefabSetManager _PrefabSetManager, 
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider) 
            : base(
                _PrefabSetManager,
                _ContainersGetter,
                _CameraProvider,
                _ColorProvider) { }

        #endregion

        #region api

        public override void SetTransitionValue(float _Value, bool _Appear)
        {
            float tValue = _Appear ? _Value * 0.5f : 1f - 0.5f * _Value;
            Material.SetFloat(TransitionValueId, tValue);
        }

        #endregion
    }
}