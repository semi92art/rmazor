using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public abstract class FullscreenTextureProviderMetaBallsBase: 
        FullscreenTransitionTextureProviderSimpleBase
    {
        protected FullscreenTextureProviderMetaBallsBase(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider) : 
            base(
                _PrefabSetManager,
                _ContainersGetter,
                _CameraProvider,
                _ColorProvider) { }

        protected override int SortingOrder => SortingOrders.GameLogoBackground;
    }
    
    public interface IFullscreenTextureProviderMetaBalls1 
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTextureProviderMetaBalls1 :
        FullscreenTextureProviderMetaBallsBase,
        IFullscreenTextureProviderMetaBalls1
    {
        #region nonpublic members
        protected override string MaterialAssetName => "transition_texture_material_metaballs_1";

        #endregion

        #region inject

        private FullscreenTextureProviderMetaBalls1(
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
    }
    
    public interface IFullscreenTextureProviderMetaBalls2
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTextureProviderMetaBalls2 :
        FullscreenTextureProviderMetaBallsBase,
        IFullscreenTextureProviderMetaBalls2
    {
        #region nonpublic members
        protected override string MaterialAssetName => "transition_texture_material_metaballs_2";

        #endregion

        #region inject

        private FullscreenTextureProviderMetaBalls2(
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
    }
    
    public interface IFullscreenTextureProviderMetaBalls3
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTextureProviderMetaBalls3 :
        FullscreenTextureProviderMetaBallsBase,
        IFullscreenTextureProviderMetaBalls3
    {
        #region nonpublic members
        protected override string MaterialAssetName => "transition_texture_material_metaballs_3";

        #endregion

        #region inject

        private FullscreenTextureProviderMetaBalls3(
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
    }
    
    public interface IFullscreenTextureProviderMetaBalls4
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTextureProviderMetaBalls4 :
        FullscreenTextureProviderMetaBallsBase,
        IFullscreenTextureProviderMetaBalls4
    {
        #region nonpublic members
        protected override string MaterialAssetName => "transition_texture_material_metaballs_4";

        #endregion

        #region inject

        private FullscreenTextureProviderMetaBalls4(
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
    }
    
    public interface IFullscreenTextureProviderMetaBalls5
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTextureProviderMetaBalls5 :
        FullscreenTextureProviderMetaBallsBase,
        IFullscreenTextureProviderMetaBalls5
    {
        #region nonpublic members
        protected override string MaterialAssetName => "transition_texture_material_metaballs_5";

        #endregion

        #region inject

        private FullscreenTextureProviderMetaBalls5(
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
    }
}