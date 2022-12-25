using System.Collections;
using Common;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTextureProvider : IInit, System.ICloneable
    {
        float        Quality  { get; }
        MeshRenderer Renderer { get; }
        void         SetMaterial(Material _Material);
        void         Activate(bool        _Active);
        
        void         Show(
            float    _Time,
            Color _ColorFrom1,
            Color _ColorFrom2, 
            Color _ColorTo1,
            Color _ColorTo2);
    }

    public abstract class FullscreenTextureProviderBase :
        FullscreenTextureProviderBaseBase,
        IFullscreenTextureProvider
    {
        #region nonpublic members

        protected static readonly int DirectionId = Shader.PropertyToID("_Direction");

        protected override int SortingOrder => SortingOrders.BackgroundTexture;

        private IEnumerator m_LastCoroutine;
        
        
        #endregion

        #region inject

        protected IViewGameTicker Ticker { get; }

        protected FullscreenTextureProviderBase(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider,
            IViewGameTicker   _Ticker) 
            : base(
                _PrefabSetManager,
                _ContainersGetter, 
                _CameraProvider,
                _ColorProvider)
        {
            Ticker = _Ticker;
        }

        #endregion

        #region api

        public abstract float        Quality  { get; }
        public          MeshRenderer Renderer { get; private set; }
        
        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            InitTexture();
            base.Init();
        }
        
        public void SetMaterial(Material _Material)
        {
            Material = _Material;
        }

        public void Activate(bool _Active)
        {
            Renderer.enabled = _Active;
        }

        public void Show(float _Time, Color _ColorFrom1, Color _ColorFrom2, Color _ColorTo1, Color _ColorTo2)
        {
            Cor.Stop(m_LastCoroutine);
            m_LastCoroutine = ShowTexture(_Time, _ColorFrom1, _ColorFrom2, _ColorTo1, _ColorTo2);
            Cor.Run(m_LastCoroutine);
        }

        public void SetTransitionValue(float _Value, bool _Appear)
        {
            throw new System.NotSupportedException();
        }

        public virtual object Clone() => new object();

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Background1: Material.SetColor(Color1Id, _Color); break;
                case ColorIds.Background2: Material.SetColor(Color2Id, _Color); break;
            }
        }

        private void InitTexture()
        {
            var parent = ContainersGetter.GetContainer(ContainerNamesCommon.Background);
            var go = PrefabSetManager.InitPrefab(
                parent, "background", "background_texture");
            go.name = "Background Texture: " + Material.name;
            Renderer = go.GetCompItem<MeshRenderer>("renderer");
            Renderer.material = Material;
            Renderer.sortingOrder = SortingOrder;
            ScaleTextureToViewport(Renderer);
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
                Ticker,
                _Time,
                _OnProgress: _P =>
                {
                    var newCol1 = Color.Lerp(_ColorFrom1, _ColorTo1, _P);
                    var newCol2 = Color.Lerp(_ColorFrom2, _ColorTo2, _P);
                    Material.SetColor(Color1Id, newCol1);
                    Material.SetColor(Color2Id, newCol2);
                });
        }

        #endregion
    }
}