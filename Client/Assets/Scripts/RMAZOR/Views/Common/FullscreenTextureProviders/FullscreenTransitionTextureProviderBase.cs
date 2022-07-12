using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTransitionTextureProvider : IInit
    {
        void Activate(bool            _Active);
        void SetTransitionValue(float _Value);
    }
    
    public abstract class FullscreenTransitionTextureProviderBase : InitBase, IFullscreenTransitionTextureProvider
    {
        #region nonpublic members
        
        protected abstract int    SortingOrder      { get; }
        protected abstract string MaterialAssetName { get; }

        protected Material Material;

        #endregion

        #region inject
        
        private IPrefabSetManager PrefabSetManager { get; }
        private IContainersGetter ContainersGetter { get; }
        private ICameraProvider   CameraProvider   { get; }
        private IColorProvider    ColorProvider    { get; }

        protected FullscreenTransitionTextureProviderBase(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider)
        {
            PrefabSetManager = _PrefabSetManager;
            ContainersGetter = _ContainersGetter;
            CameraProvider   = _CameraProvider;
            ColorProvider    = _ColorProvider;
        }

        #endregion

        #region api
        
        public MeshRenderer Renderer { get; private set; }

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            InitTexture();
            base.Init();
        }
        
        public abstract void Activate(bool            _Active);
        public abstract void SetTransitionValue(float _Value);

        #endregion

        #region nonpublic methods
        
        protected virtual void OnColorChanged(int _ColorId, Color _Color) { }

        private void ScaleTextureToViewport(Component _Renderer)
        {
            var camera = CameraProvider.Camera;
            var tr = _Renderer.transform;
            tr.position = camera.transform.position.PlusZ(20f);
            var bds = GraphicUtils.GetVisibleBounds();
            tr.localScale = new Vector3(bds.size.x, 1f, bds.size.y) * 0.1f;
        }
        
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