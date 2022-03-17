using System.Collections.Generic;
using Common;
using Common.Exceptions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
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
    
    public class ViewMazeBackgroundTextureController : InitBase, IViewMazeBackgroundTextureController, IUpdateTick
    {
        #region nonpublic members

        private IList<LinesTextureSetItem>       m_LinesTextureSetItems;
        private IList<CirclesTextureSetItem>     m_CirclesTextureSetItems;
        private IList<Circles2TextureSetItem>    m_Circles2TextureSetItems;
        private IList<TrianglesTextureSetItem>   m_TrianglesTextureSetItems;
        private IList<BackAndFrontColorsSetItem> m_BackAndFrontColorsSetItemsLight;
        private Color                            m_BackCol1Current;
        private Color                            m_BackCol2Current;
        private Color                            m_BackCol1Prev;
        private Color                            m_BackCol2Prev;
        private Color                            m_BackCol1Next;
        private Color                            m_BackCol2Next;
        private bool                             m_IsFirstLoad = true;
        

        #endregion
        
        #region inject

        private RemoteProperties                            RemoteProperties         { get; }
        private IViewMazeBackgroundLinesTextureProvider     LinesTextureProvider     { get; }
        private IViewMazeBackgroundCirclesTextureProvider   CirclesTextureProvider   { get; }
        private IViewMazeBackgroundCircles2TextureProvider  Circles2TextureProvider  { get; }
        private IViewMazeBackgroundTrianglesTextureProvider TrianglesTextureProvider { get; }
        private IPrefabSetManager                           PrefabSetManager         { get; }
        private IColorProvider                              ColorProvider            { get; }
        private IModelGame                                  Model                    { get; }
        private IViewBetweenLevelTransitioner               Transitioner             { get; }
        private IViewGameTicker                             Ticker                   { get; }

        public ViewMazeBackgroundTextureController(
            RemoteProperties                            _RemoteProperties,
            IViewMazeBackgroundLinesTextureProvider     _LinesTextureProvider,
            IViewMazeBackgroundCirclesTextureProvider   _CirclesTextureProvider,
            IViewMazeBackgroundCircles2TextureProvider  _Circles2TextureProvider,
            IViewMazeBackgroundTrianglesTextureProvider _TrianglesTextureProvider,
            IPrefabSetManager                           _PrefabSetManager,
            IColorProvider                              _ColorProvider,
            IModelGame                                  _Model,
            IViewBetweenLevelTransitioner               _Transitioner,
            IViewGameTicker                             _Ticker)
        {
            RemoteProperties         = _RemoteProperties;
            LinesTextureProvider     = _LinesTextureProvider;
            CirclesTextureProvider   = _CirclesTextureProvider;
            Circles2TextureProvider  = _Circles2TextureProvider;
            PrefabSetManager         = _PrefabSetManager;
            ColorProvider            = _ColorProvider;
            Model                    = _Model;
            Transitioner             = _Transitioner;
            Ticker                   = _Ticker;
            TrianglesTextureProvider = _TrianglesTextureProvider;
        }

        #endregion

        #region api

        public override void Init()
        {
            Ticker.Register(this);
            ColorProvider.ColorThemeChanged += OnColorThemeChanged;
            LoadSets();
            LinesTextureProvider     .Init();
            CirclesTextureProvider   .Init();
            Circles2TextureProvider  .Init();
            TrianglesTextureProvider .Init();
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
        
        private void LoadSets()
        {
            const string set = "configs";
            m_LinesTextureSetItems = RemoteProperties.LinesTextureSet;
            if (m_LinesTextureSetItems == null)
            {
                var linesTextureSet = PrefabSetManager.GetObject<LinesTexturePropertiesSetScriptableObject>
                    (set, "lines_texture_set");
                m_LinesTextureSetItems = linesTextureSet.set;
            }
            m_CirclesTextureSetItems = RemoteProperties.CirclesTextureSet;
            if (m_CirclesTextureSetItems == null)
            {
                var circlesTextureSet = PrefabSetManager.GetObject<CirclesTexturePropertiesSetScriptableObject>
                    (set, "circles_texture_set");
                m_CirclesTextureSetItems = circlesTextureSet.set;
            }
            m_Circles2TextureSetItems = RemoteProperties.Circles2TextureSet;
            if (m_Circles2TextureSetItems == null)
            {
                var circles2TextureSet = PrefabSetManager.GetObject<Circles2TexturePropertiesSetScriptableObject>
                    (set, "circles2_texture_set");
                m_Circles2TextureSetItems = circles2TextureSet.set;
            }
            m_TrianglesTextureSetItems = RemoteProperties.TrianglesTextureSet;
            if (m_TrianglesTextureSetItems == null)
            {
                var trianglesTextureSet = PrefabSetManager.GetObject<TrianglesTexturePropertiesSetScriptableObject>
                    (set, "triangles_texture_set");
                m_TrianglesTextureSetItems = trianglesTextureSet.set;
            }
            m_BackAndFrontColorsSetItemsLight = RemoteProperties.BackAndFrontColorsSet;
            if (m_BackAndFrontColorsSetItemsLight == null)
            {
                Dbg.Log("remote m_BackAndFrontColorsSetItemsLight is null");
                var backgroundColorsSetLight = PrefabSetManager.GetObject<BackAndFrontColorsSetScriptableObject>
                    (set, "back_and_front_colors_set_light");
                m_BackAndFrontColorsSetItemsLight = backgroundColorsSetLight.set;
            }
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
                    var set2 = m_CirclesTextureSetItems[levelIndexInt % m_CirclesTextureSetItems.Count];
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
            int idx1 = ColorIds.Background1;
            int idx2 = ColorIds.Background2;
            m_BackCol1Current = GetBackgroundColor(idx1, _LevelIndex, _Theme);
            m_BackCol2Current = GetBackgroundColor(idx2, _LevelIndex, _Theme);
            m_BackCol1Prev    = GetBackgroundColor(idx1, _LevelIndex - 1, _Theme);
            m_BackCol2Prev    = GetBackgroundColor(idx2, _LevelIndex - 1, _Theme);
            m_BackCol1Next    = GetBackgroundColor(idx1, _LevelIndex + 1, _Theme);
            m_BackCol2Next    = GetBackgroundColor(idx2, _LevelIndex + 1, _Theme);
            ColorProvider.SetColor(ColorIds.Background1, m_BackCol1Current);
            ColorProvider.SetColor(ColorIds.Background2, m_BackCol2Current);
        }

        private Color GetBackgroundColor(int _ColorId, long _LevelIndex, EColorTheme _Theme)
        {
            var colorsSet = m_BackAndFrontColorsSetItemsLight;
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            int setItemIdx = group % colorsSet.Count;
            var setItem = colorsSet[setItemIdx];
            if (_ColorId == ColorIds.Background1)
                return setItem.bacground1;
            return _ColorId == ColorIds.Background2 ? setItem.bacground2 : Color.magenta;
        }
        
        #endregion
    }
}