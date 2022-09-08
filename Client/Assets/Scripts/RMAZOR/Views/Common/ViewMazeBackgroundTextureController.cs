using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.FullscreenTextureProviders;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;

namespace RMAZOR.Views.Common
{
    public class ViewMazeBackgroundTextureController : ViewMazeBackgroundTextureControllerBase
    {
        #region nonpublic members

        private IList<Triangles2TextureProps> m_Triangles2TextureSetItems;
        private List<ITextureProps>           m_TextureSetItems;
        
        private bool m_IsFirstLoad = true;
        
        #endregion
        
        #region inject

        private IFullscreenTextureProviderParallaxGradientCircles TextureProviderParallaxCircles { get; }
        private IFullscreenTextureProviderRaveSquares     TextureProviderRaveSquares     { get; }
        private IFullscreenTextureProviderSwirlForPlanet  TextureProviderSwirlForPlanet  { get; }
        private IFullscreenTextureProviderTriangles2      TextureProviderTriangles2      { get; }
        private IFullscreenTextureProviderFallInDeep      TextureProviderFallInDeep      { get; }
        private IFullscreenTextureProviderBluePurplePlaid TextureProviderBluePurplePlaid { get; }

        private ViewMazeBackgroundTextureController(
            IRemotePropertiesRmazor                           _RemoteProperties,
            IColorProvider                                    _ColorProvider,
            IPrefabSetManager                                 _PrefabSetManager,
            IFullscreenTextureProviderParallaxGradientCircles _TextureProviderParallaxCircles,
            IFullscreenTextureProviderRaveSquares             _TextureProviderRaveSquares,
            IFullscreenTextureProviderSwirlForPlanet          _TextureProviderSwirlForPlanet,
            IFullscreenTextureProviderTriangles2              _TextureProviderTriangles2,
            IFullscreenTextureProviderFallInDeep              _TextureProviderFallInDeep,
            IFullscreenTextureProviderBluePurplePlaid _TextureProviderBluePurplePlaid)
            : base(
                _RemoteProperties,
                _ColorProvider, 
                _PrefabSetManager)
        {
            TextureProviderParallaxCircles = _TextureProviderParallaxCircles;
            TextureProviderRaveSquares     = _TextureProviderRaveSquares;
            TextureProviderSwirlForPlanet  = _TextureProviderSwirlForPlanet;
            TextureProviderTriangles2      = _TextureProviderTriangles2;
            TextureProviderFallInDeep      = _TextureProviderFallInDeep;
            TextureProviderBluePurplePlaid = _TextureProviderBluePurplePlaid;
        }

        #endregion

        #region api

        public override void Init()
        {
            CommonDataRmazor.BackgroundTextureController = this;
            TextureProviderParallaxCircles .Init();
            TextureProviderRaveSquares     .Init();
            TextureProviderSwirlForPlanet  .Init();
            TextureProviderTriangles2      .Init();
            TextureProviderFallInDeep      .Init();
            TextureProviderBluePurplePlaid.Init();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.LevelStage == ELevelStage.Loaded)
                OnLevelLoaded(_Args);
        }
        
        #if UNITY_EDITOR

        public void SetAdditionalInfo(AdditionalColorPropsAdditionalInfo _AdditionalInfo)
        {
            AdditionalInfo = _AdditionalInfo;
            TextureProviderSwirlForPlanet.SetAdditionalParams(
                new TextureProviderSwirlForPlanetAdditionalParams(AdditionalInfo.swirlForPlanetColorCoefficient1));
        }
        
        #endif

        #endregion
        
        #region nonpublic methods

        private void OnLevelLoaded(LevelStageArgs _Args)
        {
            ActivateConcreteBackgroundTexture(_Args.LevelIndex, out var provider);
            int group = RmazorUtils.GetGroupIndex(_Args.LevelIndex);
            long firstLevInGroup = RmazorUtils.GetFirstLevelInGroup(group);
            bool predicate = _Args.LevelIndex == firstLevInGroup || m_IsFirstLoad;
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

        private void ActivateConcreteBackgroundTexture(long _LevelIndex, out IFullscreenTextureProvider _Provider)
        {
            TextureProviderParallaxCircles.Activate(false);
            TextureProviderSwirlForPlanet .Activate(false);
            TextureProviderRaveSquares    .Activate(false);
            TextureProviderTriangles2     .Activate(false);
            TextureProviderFallInDeep     .Activate(false);
            TextureProviderBluePurplePlaid.Activate(false);
            int group = RmazorUtils.GetGroupIndex(_LevelIndex);
            int c = group % 6;
            switch (c)
            {
                case 0: _Provider = TextureProviderParallaxCircles; break;
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
                default: throw new SwitchCaseNotImplementedException(c);
            }
            _Provider.Activate(true);
        }

        #endregion
    }
}