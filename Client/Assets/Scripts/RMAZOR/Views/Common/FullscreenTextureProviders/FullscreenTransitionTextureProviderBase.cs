using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTransitionTextureProvider : IInit
    {
        MeshRenderer Renderer { get; }
        void         Activate(bool            _Active);
        void         SetTransitionValue(float _Value, bool _Appear);
    }
    
    public abstract class FullscreenTransitionTextureProviderBase 
        : FullscreenTextureProviderBaseBase, 
          IFullscreenTransitionTextureProvider
    {
        #region nonpublic members
        
        protected abstract string MaterialAssetName  { get; }
        
        #endregion

        #region inject


        protected FullscreenTransitionTextureProviderBase(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider)
        : base (
            _PrefabSetManager,
            _ContainersGetter, 
            _CameraProvider, 
            _ColorProvider) { }
        #endregion

        #region api
        
        public MeshRenderer Renderer { get; private set; }

        public override void Init()
        {
            InitTexture();
            base.Init();
        }

        public abstract void Activate(bool            _Active);
        public abstract void SetTransitionValue(float _Value, bool _Appear);

        #endregion

        #region nonpublic methods
        private void InitTexture()
        {
            var parent = ContainersGetter.GetContainer(ContainerNames.Background);
            var go = PrefabSetManager.InitPrefab(
                parent, "background", "background_texture");
            Renderer = go.GetCompItem<MeshRenderer>("renderer");
            Renderer.material = PrefabSetManager.InitObject<Material>(
                "materials", MaterialAssetName);
            ScaleTextureToViewport(Renderer);
            Renderer.sortingOrder = SortingOrder;
            Material = Renderer.sharedMaterial;
        }

        #endregion
    }
}