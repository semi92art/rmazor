using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.Exceptions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders;
using RMAZOR.Views.Helpers;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeBackgroundTextureController : IInit, IOnLevelStageChanged
    {
        public void GetBackgroundColors(
            out Color _Current1, 
            out Color _Current2, 
            out Color _Previous1,
            out Color _Previous2,
            out Color _Next1,
            out Color _Next2);
    }
    
    public class ViewMazeBackgroundTextureController : InitBase, IViewMazeBackgroundTextureController
    {
        #region nonpublic members

        private IList<LinesTextureSetItem>     m_LinesTextureSetItems;
        private IList<CirclesTextureSetItem>   m_CirclesTextureSetItems1;
        private IList<Circles2TextureSetItem>  m_Circles2TextureSetItems;
        private IList<TrianglesTextureSetItem> m_TrianglesTextureSetItems;
        private Color                          m_BackCol1Current;
        private Color                          m_BackCol2Current;
        private Color                          m_BackCol1Prev;
        private Color                          m_BackCol2Prev;
        private Color                          m_BackCol1Next;
        private Color                          m_BackCol2Next;
        private bool                           m_IsFirstLoad = true;

        #endregion
        
        #region inject
        
        private IViewMazeBackgroundLinesTextureProvider     LinesTextureProvider     { get; }
        private IViewMazeBackgroundCirclesTextureProvider   CirclesTextureProvider   { get; }
        private IViewMazeBackgroundCircles2TextureProvider  Circles2TextureProvider  { get; }
        private IViewMazeBackgroundTrianglesTextureProvider TrianglesTextureProvider { get; }
        private IPrefabSetManager                           PrefabSetManager         { get; }
        private IColorProvider                              ColorProvider            { get; }
        private IModelGame                                  Model                    { get; }
        private IViewBetweenLevelTransitioner               Transitioner             { get; }

        public ViewMazeBackgroundTextureController(
            IViewMazeBackgroundLinesTextureProvider       _LinesTextureProvider,
            IViewMazeBackgroundCirclesTextureProvider     _CirclesTextureProvider,
            IViewMazeBackgroundCircles2TextureProvider    _Circles2TextureProvider,
            IViewMazeBackgroundTrianglesTextureProvider   _TrianglesTextureProvider,
            IPrefabSetManager                             _PrefabSetManager,
            IColorProvider                                _ColorProvider,
            IModelGame                                    _Model,
            IViewBetweenLevelTransitioner                 _Transitioner)
        {
            LinesTextureProvider     = _LinesTextureProvider;
            CirclesTextureProvider   = _CirclesTextureProvider;
            Circles2TextureProvider  = _Circles2TextureProvider;
            PrefabSetManager         = _PrefabSetManager;
            ColorProvider            = _ColorProvider;
            Model                    = _Model;
            Transitioner             = _Transitioner;
            TrianglesTextureProvider = _TrianglesTextureProvider;
        }

        #endregion

        #region api

        public override void Init()
        {
            LoadSets();
            LinesTextureProvider    .Init();
            CirclesTextureProvider   .Init();
            TrianglesTextureProvider.Init();
            base.Init();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:             OnLevelLoaded(_Args);        break;
                case ELevelStage.ReadyToUnloadLevel: OnReadyToUnloadLevel(_Args); break;
            }
        }

        public void GetBackgroundColors(
            out Color _Current1,
            out Color _Current2, 
            out Color _Previous1,
            out Color _Previous2,
            out Color _Next1,   
            out Color _Next2)
        {
            _Current1  = m_BackCol1Current;
            _Current2  = m_BackCol2Current;
            _Previous1 = m_BackCol1Prev;
            _Previous2 = m_BackCol2Prev;
            _Next1     = m_BackCol1Next;
            _Next2     = m_BackCol2Next;
        }

        #endregion
        
        #region nonpublic methods

        private void LoadSets()
        {
            const string set = "configs";
            var linesTextureSet = PrefabSetManager.GetObject<LinesTexturePropertiesSetScriptableObject>
                (set, "lines_texture_set");
            m_LinesTextureSetItems = linesTextureSet.set;
            var circlesTextureSet1 = PrefabSetManager.GetObject<CirclesTexturePropertiesSetScriptableObject>
                (set, "circles_texture_set");
            m_CirclesTextureSetItems1 = circlesTextureSet1.set;
            var circles2TextureSet = PrefabSetManager.GetObject<Circles2TexturePropertiesSetScriptableObject>
                (set, "circles2_texture_set");
            m_Circles2TextureSetItems = circles2TextureSet.set;
            var trianglesTextureSet = PrefabSetManager.GetObject<TrianglesTexturePropertiesSetScriptableObject>
                (set, "triangles_texture_set");
            m_TrianglesTextureSetItems = trianglesTextureSet.set;
        }

        private void OnLevelLoaded(LevelStageArgs _Args)
        {
            SetColorsOnNewLevelOrNewTheme(
                _Args.LevelIndex,
                ColorProvider.CurrentTheme);
            int providerIndex = IndexOfCorrespondingTextureSet(_Args.LevelIndex);
            ActivateCorrespondingTextureProvider(providerIndex);
            ActivateTexturePropertiesSet(providerIndex, _Args.LevelIndex);
            float transtionTime = GetTransitionTime();
            var texProvider = GetCurrentTextureProvider(_Args.LevelIndex);
            int group = RazorMazeUtils.GetGroupIndex(_Args.LevelIndex);
            long firstLevInGroup = RazorMazeUtils.GetFirstLevelInGroup(group);
            var colFrom1 = _Args.LevelIndex == firstLevInGroup || m_IsFirstLoad ? m_BackCol1Current : m_BackCol1Prev;
            var colFrom2 = _Args.LevelIndex == firstLevInGroup || m_IsFirstLoad ? m_BackCol2Current : m_BackCol2Prev;
            m_IsFirstLoad = false;
            texProvider.Show(
                transtionTime, 
                colFrom1, 
                colFrom2, 
                m_BackCol1Current, 
                m_BackCol2Current);
        }

        private void OnReadyToUnloadLevel(LevelStageArgs _Args)
        {
            float transtionTime = GetTransitionTime();
            var texProvider = GetCurrentTextureProvider(_Args.LevelIndex);
            texProvider.Show(
                transtionTime, 
                m_BackCol1Current, 
                m_BackCol2Current, 
                m_BackCol1Next, 
                m_BackCol1Next);
        }

        private void ActivateCorrespondingTextureProvider(int _ProviderIndex)
        {
            LinesTextureProvider.Activate(_ProviderIndex == 1);
            CirclesTextureProvider.Activate(_ProviderIndex == 2);
            Circles2TextureProvider.Activate(_ProviderIndex == 3);
            TrianglesTextureProvider.Activate(_ProviderIndex == 4);
        }

        private static int IndexOfCorrespondingTextureSet(long _LevelIndex)
        {
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            return (group % 4) switch { 0 => 1, 1 => 2, 2 => 3, 3 => 4, _ => 1 };
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
                    var set2 = m_CirclesTextureSetItems1[levelIndexInt % m_CirclesTextureSetItems1.Count];
                    CirclesTextureProvider.SetProperties(set2);
                    break;
                case 3:
                    var set3 = m_Circles2TextureSetItems[levelIndexInt % m_Circles2TextureSetItems.Count];
                    Circles2TextureProvider.SetProperties(set3);
                    break;
                case 4:
                    var set4 = m_TrianglesTextureSetItems[levelIndexInt % m_TrianglesTextureSetItems.Count];
                    TrianglesTextureProvider.SetProperties(set4);
                    break;
                default: throw new SwitchCaseNotImplementedException(_ProviderIndex);
            }
        }

        private float GetTransitionTime()
        {
            return Transitioner.FullTransitionTime;
        }

        private IViewMazeBackgroundTextureProvider GetCurrentTextureProvider(long _LevelIndex)
        {
            int idx = IndexOfCorrespondingTextureSet(_LevelIndex);
            return idx == 2 ? LinesTextureProvider : CirclesTextureProvider as IViewMazeBackgroundTextureProvider;
        }
        
        private void SetColorsOnNewLevelOrNewTheme(
            long _LevelIndex,
            EColorTheme _Theme)
        {
            GetHSV(_LevelIndex, _Theme, true, out float h1, out float s1, out float v1);
            GetHSV(_LevelIndex, _Theme, false, out float h2, out float s2, out float v2);
            GetHSV(_LevelIndex - 1, _Theme, true, out float h3, out float s3, out float v3);
            GetHSV(_LevelIndex - 1, _Theme, false, out float h4, out float s4, out float v4);
            GetHSV(_LevelIndex + 1, _Theme, true, out float h5, out float s5, out float v5);
            GetHSV(_LevelIndex + 1, _Theme, false, out float h6, out float s6, out float v6);
            m_BackCol1Current = Color.HSVToRGB(h1, s1, v1);
            m_BackCol2Current = Color.HSVToRGB(h2, s2, v2);
            m_BackCol1Prev = Color.HSVToRGB(h3, s3, v3);
            m_BackCol2Prev = Color.HSVToRGB(h4, s4, v4);
            m_BackCol1Next = Color.HSVToRGB(h5, s5, v5);
            m_BackCol2Next = Color.HSVToRGB(h6, s6, v6);
            ColorProvider.SetColor(ColorIds.Background1, m_BackCol1Current);
            ColorProvider.SetColor(ColorIds.Background2, m_BackCol2Current);
        }

        private static void GetHSV(
            long        _LevelIndex,
            EColorTheme _Theme,
            bool        _IsMainColor,
            out float   _H,
            out float   _S,
            out float   _V)
        {
            _H = ViewMazeBackgroundUtils.GetHForHSV(_LevelIndex);
            _S = 50f / 100f;
            _V = _Theme switch
            {
                EColorTheme.Light => (_IsMainColor ? 50f : 70f) / 100f,
                EColorTheme.Dark  => (_IsMainColor ? 5f : 10f) / 100f,
                _                 => throw new SwitchExpressionException(_Theme)
            };
        }
        
        #endregion
    }
}