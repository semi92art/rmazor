using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public abstract class FullscreenTextureProviderBaseBase : InitBase
    {
        #region nonpublic members
        
        protected static readonly int
            Color1Id = Shader.PropertyToID("_Color1"),
            Color2Id = Shader.PropertyToID("_Color2");

        protected abstract int SortingOrder { get; }
        
        protected Material Material { get; set; }

        #endregion
        
        #region inject
        
        protected IPrefabSetManager PrefabSetManager { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected ICameraProvider   CameraProvider   { get; }
        protected IColorProvider    ColorProvider    { get; }

        protected FullscreenTextureProviderBaseBase(
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

        #region nonpublic methods

        protected void ScaleTextureToViewport(Component _Renderer)
        {
            var camera = CameraProvider.Camera;
            if (camera.IsNull())
                return;
            var tr = _Renderer.transform;
            tr.position = camera.transform.position.PlusZ(20f);
            var bds = GraphicUtils.GetVisibleBounds();
            tr.localScale = new Vector3(bds.size.x, 1f, bds.size.y) * 0.1f;
        }

        #endregion
    }
}