using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Debugging;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.FullscreenTextureProviders;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public class ViewMazeBackgroundTextureController : ViewMazeBackgroundTextureControllerBase
    {
        #region constants

        private const float QualityLevel = 0.5f;
        
        #endregion
        
        #region nonpublic members

        private static readonly int  MainTexId = Shader.PropertyToID("_MainTex");

        private IList<Triangles2TextureProps> m_Triangles2TextureSetItems;
        private List<ITextureProps>           m_TextureSetItems;
        private Camera                        m_RenderCamera;
        private RenderTexture                 m_RenderTexture;
        private MeshRenderer                  m_MainRenderer;
        private Material                      m_MainRendererMaterial;

        private bool m_IsFirstLoad = true;
        private bool m_IsLowPerformance;

        #endregion
        
        #region inject

        private ICameraProvider                           CameraProvider                 { get; }
        private IContainersGetter                         ContainersGetter               { get; }
        private IFullscreenTextureProviderRaveSquares     TextureProviderRaveSquares     { get; }
        private IFullscreenTextureProviderSwirlForPlanet  TextureProviderSwirlForPlanet  { get; }
        private IFullscreenTextureProviderTriangles2      TextureProviderTriangles2      { get; }
        private IFullscreenTextureProviderFallInDeep      TextureProviderFallInDeep      { get; }
        private IFullscreenTextureProviderBluePurplePlaid TextureProviderBluePurplePlaid { get; }
        private IFullscreenTextureProviderLogichroma      TextureProviderLogichroma      { get; }
        private IFullscreenTextureProviderGradient        TextureProviderGradient        { get; }
        private IFullscreenTextureProviderSolid           TextureProviderSolid           { get; }
        private IFpsCounter                               FpsCounter                     { get; }

        private ViewMazeBackgroundTextureController(
            ICameraProvider                           _CameraProvider,
            IContainersGetter                         _ContainersGetter,
            IRemotePropertiesRmazor                   _RemoteProperties,
            IColorProvider                            _ColorProvider,
            IPrefabSetManager                         _PrefabSetManager,
            IFullscreenTextureProviderRaveSquares     _TextureProviderRaveSquares,
            IFullscreenTextureProviderSwirlForPlanet  _TextureProviderSwirlForPlanet,
            IFullscreenTextureProviderTriangles2      _TextureProviderTriangles2,
            IFullscreenTextureProviderFallInDeep      _TextureProviderFallInDeep,
            IFullscreenTextureProviderBluePurplePlaid _TextureProviderBluePurplePlaid,
            IFullscreenTextureProviderLogichroma      _TextureProviderLogichroma,
            IFullscreenTextureProviderGradient        _TextureProviderGradient,
            IFullscreenTextureProviderSolid           _TextureProviderSolid,
            IFpsCounter                               _FpsCounter)
            : base(
                _RemoteProperties,
                _ColorProvider, 
                _PrefabSetManager)
        {
            CameraProvider                 = _CameraProvider;
            ContainersGetter               = _ContainersGetter;
            TextureProviderRaveSquares     = _TextureProviderRaveSquares;
            TextureProviderSwirlForPlanet  = _TextureProviderSwirlForPlanet;
            TextureProviderTriangles2      = _TextureProviderTriangles2;
            TextureProviderFallInDeep      = _TextureProviderFallInDeep;
            TextureProviderBluePurplePlaid = _TextureProviderBluePurplePlaid;
            TextureProviderLogichroma      = _TextureProviderLogichroma;
            TextureProviderGradient        = _TextureProviderGradient;
            TextureProviderSolid           = _TextureProviderSolid;
            FpsCounter                     = _FpsCounter;
        }

        #endregion

        #region api

        public override void Init()
        {
            CommonDataRmazor.BackgroundTextureController = this;
            InitTextureProviders();
            SetTextureProvidersPosition();
            InitRenderTexture();
            InitRenderCamera();
            InitMainBackgroundRenderer();
            // CheckPerformance(); // пока не уверен что это нужно
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.LevelStage == ELevelStage.Loaded)
                OnLevelLoaded(_Args);
        }
        
        public void SetAdditionalInfo(AdditionalColorPropsAdditionalInfo _AdditionalInfo)
        {
            AdditionalInfo = _AdditionalInfo;
            TextureProviderSwirlForPlanet.SetAdditionalParams(
                new TextureProviderSwirlForPlanetAdditionalParams(AdditionalInfo.swirlForPlanetColorCoefficient1));
        }

        #endregion
        
        #region nonpublic methods

        private void InitTextureProviders()
        {
            foreach (var texProvider in GetProviders())
                texProvider.Init();
        }

        private void SetTextureProvidersPosition()
        {
            foreach (var texProvider in GetProviders())
                texProvider.Renderer.transform.SetLocalPosXY(Vector2.right * 300f);
        }

        private void InitMainBackgroundRenderer()
        {
            var parent = ContainersGetter.GetContainer(ContainerNames.Background);
            var go = PrefabSetManager.InitPrefab(
                parent, "background", "background_texture");
            m_MainRenderer = go.GetCompItem<MeshRenderer>("renderer");
            m_MainRendererMaterial = PrefabSetManager.InitObject<Material>(
                "materials", "main_back_main_material");
            m_MainRendererMaterial.SetTexture(MainTexId, m_RenderTexture);
            m_MainRenderer.sharedMaterial = m_MainRendererMaterial;
            ScaleTextureToViewport(m_MainRenderer);
            m_MainRenderer.sortingOrder = SortingOrders.BackgroundTexture;
        }
        
        private void ScaleTextureToViewport(Component _Renderer)
        {
            var camera = CameraProvider.Camera;
            var tr = _Renderer.transform;
            tr.position = camera.transform.position.PlusZ(20f);
            var bds = GraphicUtils.GetVisibleBounds();
            tr.localScale = new Vector3(bds.size.x, 1f, bds.size.y) * 0.1f;
        }

        private void InitRenderTexture()
        {
            var scrSize = GraphicUtils.ScreenSize;
            m_RenderTexture = new RenderTexture(
                (int)(scrSize.x * QualityLevel), 
                (int)(scrSize.y * QualityLevel), 0);
        }

        private void InitRenderCamera()
        {
            var camGo = new GameObject("Background Texture Camera");
            var container = ContainersGetter.GetContainer(ContainerNames.Background);
            camGo.SetParent(container);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.transform.SetLocalPosXY(Vector2.right * 300f).SetLocalPosZ(-1f);
            cam.depth = 2f;
            cam.orthographicSize = CameraProvider.Camera.orthographicSize;
            cam.targetTexture = m_RenderTexture;
            cam.cullingMask = LayerMask.GetMask("μ Mu");
            m_RenderCamera = cam;
        }
        
        private void OnLevelLoaded(LevelStageArgs _Args)
        {
            ActivateAndShowBackgroundTexture(_Args.LevelIndex);
        }

        protected override void LoadSets()
        {
            base.LoadSets();
            const string set = "configs";
            m_Triangles2TextureSetItems = RemoteProperties.Tria2TextureSet;
            if (m_Triangles2TextureSetItems.NullOrEmpty())
            {
                var triangles2TextureSet = PrefabSetManager.GetObject<Triangles2TexturePropsSetScriptableObject>
                    (set, "triangles2_texture_set");
                m_Triangles2TextureSetItems = triangles2TextureSet.set;
            }
            static int CalculateTextureHash(string _Value)
            {
                ulong hashedValue = 3074457345618258791ul;
                for(int i=0; i<_Value.Length; i++)
                {
                    hashedValue += _Value[i];
                    hashedValue *= 3074457345618258799ul;
                }
                return (int)hashedValue;
            }
            m_TextureSetItems = m_Triangles2TextureSetItems
                .Cast<ITextureProps>()
                .OrderBy(_Item => CalculateTextureHash(
                    _Item.ToString(null, null)))
                
                .Where(_Props => _Props.InUse)
                .ToList();
        }

        private void ActivateAndShowBackgroundTexture(long _LevelIndex)
        {
            ActivateConcreteBackgroundTexture(_LevelIndex, out var provider);
            int group = RmazorUtils.GetGroupIndex(_LevelIndex);
            long firstLevInGroup = RmazorUtils.GetFirstLevelInGroup(group);
            bool predicate = _LevelIndex == firstLevInGroup || m_IsFirstLoad;
            var colFrom1 = predicate ? BackCol1Current : BackCol1Prev;
            var colFrom2 = predicate ? BackCol2Current : BackCol2Prev;
            m_IsFirstLoad = false;
            provider.Show(
                MathUtils.Epsilon, 
                colFrom1, 
                colFrom2, 
                BackCol1Current, 
                BackCol2Current);
        }

        private void ActivateConcreteBackgroundTexture(long _LevelIndex, out IFullscreenTextureProvider _Provider)
        {
            foreach (var texProvider in GetProviders())
                texProvider.Activate(false);
            if (m_IsLowPerformance)
            {
                TextureProviderBluePurplePlaid.Activate(true);
                    _Provider = TextureProviderBluePurplePlaid;
                    return;
            }
            int group = RmazorUtils.GetGroupIndex(_LevelIndex);
            int c = group % 6;
            switch (c)
            {
                case 1: _Provider = TextureProviderRaveSquares;     break;
                case 2: _Provider = TextureProviderSwirlForPlanet;
                    if (AdditionalInfo != null)
                    {
                        TextureProviderSwirlForPlanet.SetAdditionalParams(
                            new TextureProviderSwirlForPlanetAdditionalParams(AdditionalInfo.swirlForPlanetColorCoefficient1));
                    }
                    break;
                case 3:
                    int idx = group % m_TextureSetItems.Count;
                    var setItem = m_TextureSetItems[idx];
                    TextureProviderTriangles2.Activate(true);
                    TextureProviderTriangles2.SetProperties((Triangles2TextureProps)setItem);
                    _Provider = TextureProviderTriangles2;
                    break;
                case 4: _Provider = TextureProviderFallInDeep;      break;
                case 5: _Provider = TextureProviderBluePurplePlaid; break;
                case 0: _Provider = TextureProviderLogichroma;      break;
                default: throw new SwitchCaseNotImplementedException(c);
            }
            _Provider.Activate(true);
        }

        private void CheckPerformance()
        {
            bool isLowPerformanceCached = SaveUtils.GetValue(SaveKeysCommon.LowPerformanceDevice);
            if (isLowPerformanceCached)
            {
                m_IsLowPerformance = true;
                ActivateAndShowBackgroundTexture(0);
                return;
            }
            var entity = FpsCounter.IsLowPerformance;
            Cor.Run(Cor.WaitWhile(
                () => !FpsCounter.Initialized 
                      || entity.Result == EEntityResult.Pending,
                () =>
                {
                    if (entity.Result != EEntityResult.Success)
                    {
                        Dbg.LogWarning("Failed to check performance");
                        return;
                    }
                    m_IsLowPerformance = entity.Value;
                    if (m_IsLowPerformance)
                        ActivateAndShowBackgroundTexture(0);
                }));
        }

        private IEnumerable<IFullscreenTextureProvider> GetProviders()
        {
            return new List<IFullscreenTextureProvider>
            {
                TextureProviderSwirlForPlanet ,
                TextureProviderRaveSquares    ,
                TextureProviderTriangles2     ,
                TextureProviderFallInDeep     ,
                TextureProviderBluePurplePlaid,
                TextureProviderLogichroma     ,
                TextureProviderGradient       ,
                TextureProviderSolid          ,
            };
        }

        #endregion
    }
}