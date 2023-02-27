using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
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
            var parent = ContainersGetter.GetContainer(ContainerNamesCommon.Background);
            var go = PrefabSetManager.InitPrefab(
                parent, "background", "background_texture");
            go.name = "Fullscreen Texture Renderer of " + MaterialAssetName;
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