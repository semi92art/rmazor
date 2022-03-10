using System.Collections;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders
{
    public interface IViewMazeBackgroundTextureProvider : IInit
    {
        void Activate(bool   _Active);
        void Show(float _Time, Color _ColorFrom1, Color _ColorFrom2, Color _ColorTo1, Color _ColorTo2);
    }

    public abstract class ViewMazeBackgroundTextureProviderBase :
        InitBase, IViewMazeBackgroundTextureProvider
    {
        #region nonpublic members

        private static readonly int
            Color1Id = Shader.PropertyToID("_Color1"),
            Color2Id = Shader.PropertyToID("_Color2");
        protected static readonly int          
            TilingId     = Shader.PropertyToID("_Tiling"),
            DirectionId  = Shader.PropertyToID("_Direction"),
            WrapScaleId  = Shader.PropertyToID("_WrapScale"),
            WrapTilingId = Shader.PropertyToID("_WarpTiling");

        protected abstract int          SortingOrder      { get; }
        protected abstract string       TexturePrefabName { get; }
        
        protected          Material     Material;
        private            MeshRenderer m_Renderer;
        private            IEnumerator  m_LastCoroutine;

        #endregion

        #region inject

        private   IPrefabSetManager PrefabSetManager { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected ICameraProvider   CameraProvider   { get; }
        private   IViewGameTicker   Ticker           { get; }
        private   IColorProvider    ColorProvider    { get; }

        protected ViewMazeBackgroundTextureProviderBase(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IViewGameTicker   _Ticker,
            IColorProvider    _ColorProvider)
        {
            PrefabSetManager = _PrefabSetManager;
            ContainersGetter = _ContainersGetter;
            CameraProvider   = _CameraProvider;
            Ticker           = _Ticker;
            ColorProvider    = _ColorProvider;
        }

        #endregion

        #region api

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            InitTexture();
            base.Init();
        }
        
        public void Activate(bool _Active)
        {
            m_Renderer.enabled = _Active;
        }

        public void Show(float _Time, Color _ColorFrom1, Color _ColorFrom2, Color _ColorTo1, Color _ColorTo2)
        {
            Cor.Stop(m_LastCoroutine);
            m_LastCoroutine = ShowTexture(_Time, _ColorFrom1, _ColorFrom2, _ColorTo1, _ColorTo2);
            Cor.Run(m_LastCoroutine);
        }

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Background1)
                Material.SetColor(Color1Id, _Color);
            else if (_ColorId == ColorIds.Background2)
                Material.SetColor(Color2Id, _Color);
        }

        private void InitTexture()
        {
            var parent = ContainersGetter.GetContainer(ContainerNames.Background);
            var go = PrefabSetManager.InitPrefab(
                parent,
                "views",
                TexturePrefabName);
            m_Renderer = go.GetCompItem<MeshRenderer>("renderer");
            ScaleTextureToViewport(m_Renderer);
            m_Renderer.sortingOrder = SortingOrder;
            Material = m_Renderer.sharedMaterial;
        }

        private void ScaleTextureToViewport(Component _Renderer)
        {
            var camera = CameraProvider.MainCamera;
            var tr = _Renderer.transform;
            tr.position = camera.transform.position.PlusZ(20f);
            var bds = GraphicUtils.GetVisibleBounds();
            tr.localScale = new Vector3(bds.size.x, 1f, bds.size.y) * 0.1f;
        }

        private IEnumerator ShowTexture(
            float _Time,
            Color _ColorFrom1, 
            Color _ColorFrom2,
            Color _ColorTo1,
            Color _ColorTo2)
        {
            if (_Time < MathUtils.Epsilon)
            {
                Material.SetColor(Color1Id, _ColorTo1);
                Material.SetColor(Color2Id, _ColorTo2);
                yield break;
            }
            yield return Cor.Lerp(
                0f,
                1f,
                _Time,
                _P =>
                {
                    var newCol1 = Color.Lerp(_ColorFrom1, _ColorTo1, _P);
                    var newCol2 = Color.Lerp(_ColorFrom2, _ColorTo2, _P);
                    Material.SetColor(Color1Id, newCol1);
                    Material.SetColor(Color2Id, newCol2);
                },
                Ticker);
        }

        #endregion
    }
}