using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders
{
    public abstract class ViewMazeFullscreenTextureProviderBase : InitBase
    {
        protected abstract int    SortingOrder      { get; }
        protected abstract string TexturePrefabName { get; }
        
        protected Material          Material;
        
        
        protected IPrefabSetManager PrefabSetManager { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected ICameraProvider   CameraProvider   { get; }
        protected IColorProvider    ColorProvider    { get; }

        protected ViewMazeFullscreenTextureProviderBase(
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
        
        public MeshRenderer Renderer { get; private set; }

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            InitTexture();
            base.Init();
        }

        protected virtual void OnColorChanged(int _ColorId, Color _Color) { }

        protected void ScaleTextureToViewport(Component _Renderer)
        {
            var camera = CameraProvider.MainCamera;
            var tr = _Renderer.transform;
            tr.position = camera.transform.position.PlusZ(20f);
            var bds = GraphicUtils.GetVisibleBounds();
            tr.localScale = new Vector3(bds.size.x, 1f, bds.size.y) * 0.1f;
        }
        
        protected void InitTexture()
        {
            var parent = ContainersGetter.GetContainer(ContainerNames.Background);
            var go = PrefabSetManager.InitPrefab(
                parent,
                "views",
                TexturePrefabName);
            Renderer = go.GetCompItem<MeshRenderer>("renderer");
            ScaleTextureToViewport(Renderer);
            Renderer.sortingOrder = SortingOrder;
            Material = Renderer.sharedMaterial;
        }
    }
}