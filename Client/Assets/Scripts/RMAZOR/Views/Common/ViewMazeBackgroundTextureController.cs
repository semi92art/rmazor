using System.Collections.Generic;
using Common.Exceptions;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders;

namespace RMAZOR.Views.Common
{
    public class ViewMazeBackgroundTextureController : ViewMazeBackgroundTextureControllerBase, IUpdateTick
    {
        #region nonpublic members

        private IList<LinesTextureSetItem>     m_LinesTextureSetItems;
        private IList<CirclesTextureSetItem>   m_CirclesTextureSetItems;
        private IList<Circles2TextureSetItem>  m_Circles2TextureSetItems;
        private IList<TrianglesTextureSetItem> m_TrianglesTextureSetItems;
        private bool                           m_IsFirstLoad = true;
        
        #endregion
        
        #region inject

        
        private IViewMazeBackgroundLinesTextureProvider     LinesTextureProvider     { get; }
        private IViewMazeBackgroundCirclesTextureProvider   CirclesTextureProvider   { get; }
        private IViewMazeBackgroundTrianglesTextureProvider TrianglesTextureProvider { get; }
        private IModelGame                                  Model                    { get; }
        private IViewGameTicker                             Ticker                   { get; }

        public ViewMazeBackgroundTextureController(
            RemoteProperties                            _RemoteProperties,
            IViewMazeBackgroundLinesTextureProvider     _LinesTextureProvider,
            IViewMazeBackgroundCirclesTextureProvider   _CirclesTextureProvider,
            IViewMazeBackgroundTrianglesTextureProvider _TrianglesTextureProvider,
            IPrefabSetManager                           _PrefabSetManager,
            IColorProvider                              _ColorProvider,
            IModelGame                                  _Model,
            IViewGameTicker                             _Ticker) 
            : base(
                _RemoteProperties,
                _ColorProvider, 
                _PrefabSetManager)
        {
            LinesTextureProvider     = _LinesTextureProvider;
            CirclesTextureProvider   = _CirclesTextureProvider;
            Model                    = _Model;
            Ticker                   = _Ticker;
            TrianglesTextureProvider = _TrianglesTextureProvider;
        }

        #endregion

        #region api

        public override void Init()
        {
            Ticker.Register(this);
            ColorProvider.ColorThemeChanged += OnColorThemeChanged;
            LinesTextureProvider     .Init();
            CirclesTextureProvider   .Init();
            TrianglesTextureProvider .Init();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.Loaded)
                OnLevelLoaded(_Args);
        }

        public void UpdateTick()
        {
            // TODO acceleration
        }

        #endregion
        
        #region nonpublic methods

        private void OnColorThemeChanged(EColorTheme _Theme)
        {
            SetColorsOnNewLevelOrNewTheme(Model.LevelStaging.LevelIndex, _Theme);
        }
        
        protected override void LoadSets()
        {
            base.LoadSets();
            const string set = "configs";
            m_LinesTextureSetItems = RemoteProperties.LinesTextureSet;
            if (m_LinesTextureSetItems.NullOrEmpty())
            {
                var linesTextureSet = PrefabSetManager.GetObject<LinesTexturePropertiesSetScriptableObject>
                    (set, "lines_texture_set");
                m_LinesTextureSetItems = linesTextureSet.set;
            }
            m_CirclesTextureSetItems = RemoteProperties.CirclesTextureSet;
            if (m_CirclesTextureSetItems.NullOrEmpty())
            {
                var circlesTextureSet = PrefabSetManager.GetObject<CirclesTexturePropertiesSetScriptableObject>
                    (set, "circles_texture_set");
                m_CirclesTextureSetItems = circlesTextureSet.set;
            }
            m_Circles2TextureSetItems = RemoteProperties.Circles2TextureSet;
            if (m_Circles2TextureSetItems.NullOrEmpty())
            {
                var circles2TextureSet = PrefabSetManager.GetObject<Circles2TexturePropertiesSetScriptableObject>
                    (set, "circles2_texture_set");
                m_Circles2TextureSetItems = circles2TextureSet.set;
            }
            m_TrianglesTextureSetItems = RemoteProperties.TrianglesTextureSet;
            if (m_TrianglesTextureSetItems.NullOrEmpty())
            {
                var trianglesTextureSet = PrefabSetManager.GetObject<TrianglesTexturePropertiesSetScriptableObject>
                    (set, "triangles_texture_set");
                m_TrianglesTextureSetItems = trianglesTextureSet.set;
            }

        }

        private void OnLevelLoaded(LevelStageArgs _Args)
        {
            int providerIndex = IndexOfCorrespondingTextureSet(_Args.LevelIndex);
            ActivateCorrespondingTextureProvider(providerIndex);
            ActivateTexturePropertiesSet(providerIndex, _Args.LevelIndex);
            var texProvider = GetCurrentTextureProvider(_Args.LevelIndex);
            int group = RazorMazeUtils.GetGroupIndex(_Args.LevelIndex);
            long firstLevInGroup = RazorMazeUtils.GetFirstLevelInGroup(group);
            var colFrom1 = _Args.LevelIndex == firstLevInGroup || m_IsFirstLoad ? BackCol1Current : BackCol1Prev;
            var colFrom2 = _Args.LevelIndex == firstLevInGroup || m_IsFirstLoad ? BackCol2Current : BackCol2Prev;
            m_IsFirstLoad = false;
            texProvider.Show(
                MathUtils.Epsilon, 
                colFrom1, 
                colFrom2, 
                BackCol1Current, 
                BackCol2Current);
        }

        private void ActivateCorrespondingTextureProvider(int _ProviderIndex)
        {
            LinesTextureProvider.Activate(_ProviderIndex == 1);
            CirclesTextureProvider.Activate(_ProviderIndex == 2);
            TrianglesTextureProvider.Activate(_ProviderIndex == 3);
        }

        private static int IndexOfCorrespondingTextureSet(long _LevelIndex)
        {
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            return (group % 3) switch { 0 => 1, 1 => 2, 2 => 3, _ => 1 };
        }

        private void ActivateTexturePropertiesSet(int _ProviderIndex, long _LeveIndex)
        {
            int levelIndexInt = (int) _LeveIndex;
            switch (_ProviderIndex)
            {
                case 1:
                    var set1 = m_LinesTextureSetItems[levelIndexInt % m_LinesTextureSetItems.Count];
                    LinesTextureProvider.SetProperties(set1);
                    break;
                case 2:
                    var set2 = m_CirclesTextureSetItems[levelIndexInt % m_CirclesTextureSetItems.Count];
                    CirclesTextureProvider.SetProperties(set2);
                    break;
                case 3:
                    var set4 = m_TrianglesTextureSetItems[levelIndexInt % m_TrianglesTextureSetItems.Count];
                    TrianglesTextureProvider.SetProperties(set4);
                    break;
                default: throw new SwitchCaseNotImplementedException(_ProviderIndex);
            }
        }

        private IViewMazeBackgroundTextureProvider GetCurrentTextureProvider(long _LevelIndex)
        {
            int idx = IndexOfCorrespondingTextureSet(_LevelIndex);
            return idx switch
            {
                1 => LinesTextureProvider,
                2 => CirclesTextureProvider,
                3 => TrianglesTextureProvider,
                _ => throw new SwitchCaseNotImplementedException(idx)
            };
        }

        #endregion
    }
}