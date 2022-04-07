using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Exceptions;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.BackgroundIdleItems;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders;

namespace RMAZOR.Views.Common
{
    public class ViewMazeBackgroundTextureController : ViewMazeBackgroundTextureControllerBase, IUpdateTick
    {
        #region nonpublic members

        private IList<LinesTextureSetItem>      m_LinesTextureSetItems;
        private IList<CirclesTextureSetItem>    m_CirclesTextureSetItems;
        private IList<Circles2TextureSetItem>   m_Circles2TextureSetItems;
        private IList<TrianglesTextureSetItem>  m_TrianglesTextureSetItems;
        private IList<Triangles2TextureSetItem> m_Triangles2TextureSetItems;
        private bool                            m_IsFirstLoad = true;
        private List<ITextureSetItem>           m_TextureSetItems;
        
        #endregion
        
        #region inject
        
        private IViewMazeBackgroundLinesTextureProvider      LinesTextureProvider      { get; }
        private IViewMazeBackgroundCirclesTextureProvider    CirclesTextureProvider    { get; }
        private IViewMazeBackgroundTrianglesTextureProvider  TrianglesTextureProvider  { get; }
        private IModelGame                                   Model                     { get; }
        private IViewGameTicker                              Ticker                    { get; }
        private IViewMazeBackgroundIdleItems                 IdleItems                 { get; }

        public ViewMazeBackgroundTextureController(
            RemoteProperties                             _RemoteProperties,
            IViewMazeBackgroundLinesTextureProvider      _LinesTextureProvider,
            IViewMazeBackgroundCirclesTextureProvider    _CirclesTextureProvider,
            IViewMazeBackgroundTrianglesTextureProvider  _TrianglesTextureProvider,
            IPrefabSetManager                            _PrefabSetManager,
            IColorProvider                               _ColorProvider,
            IModelGame                                   _Model,
            IViewGameTicker                              _Ticker,
            IViewMazeBackgroundIdleItems                 _IdleItems) 
            : base(
                _RemoteProperties,
                _ColorProvider, 
                _PrefabSetManager)
        {
            LinesTextureProvider     = _LinesTextureProvider;
            CirclesTextureProvider   = _CirclesTextureProvider;
            Model                    = _Model;
            Ticker                   = _Ticker;
            IdleItems = _IdleItems;
            TrianglesTextureProvider = _TrianglesTextureProvider;
        }

        #endregion

        #region api

        public override void Init()
        {
            Ticker.Register(this);
            LinesTextureProvider     .Init();
            CirclesTextureProvider   .Init();
            TrianglesTextureProvider .Init();
            IdleItems                .Init();
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
            m_TrianglesTextureSetItems = RemoteProperties.TrianglesTextureSet;
            if (m_TrianglesTextureSetItems.NullOrEmpty())
            {
                var trianglesTextureSet = PrefabSetManager.GetObject<TrianglesTexturePropertiesSetScriptableObject>
                    (set, "triangles_texture_set");
                m_TrianglesTextureSetItems = trianglesTextureSet.set;
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
            m_TextureSetItems = m_LinesTextureSetItems
                .Cast<ITextureSetItem>()
                .Concat(m_CirclesTextureSetItems)
                .Concat(m_TrianglesTextureSetItems)
                .OrderBy(_Item => CalculateTextureHash(
                    _Item.ToString(null, null)))
                .ToList();
        }

        private void OnLevelLoaded(LevelStageArgs _Args)
        {
            ActivateTexturePropertiesSet(_Args.LevelIndex, out var provider);
            IdleItems.SetSpawnPool(GetProviderIndex(provider));
            int group = RazorMazeUtils.GetGroupIndex(_Args.LevelIndex);
            long firstLevInGroup = RazorMazeUtils.GetFirstLevelInGroup(group);
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

        private void ActivateTexturePropertiesSet(long _LevelIndex, out IViewMazeBackgroundTextureProvider _Provider)
        {
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            int idx = group % m_TextureSetItems.Count;
            Dbg.Log("group: " + group + ", idx: " + idx);
            var setItem = m_TextureSetItems[idx];
            LinesTextureProvider.Activate(false);
            CirclesTextureProvider.Activate(false);
            TrianglesTextureProvider.Activate(false);
            switch (setItem)
            {
                case CirclesTextureSetItem circlesSetItem:
                    CirclesTextureProvider.Activate(true);
                    CirclesTextureProvider.SetProperties(circlesSetItem);
                    _Provider = CirclesTextureProvider;
                    break;
                case TrianglesTextureSetItem trianglesSetItem:
                    TrianglesTextureProvider.Activate(true);
                    TrianglesTextureProvider.SetProperties(trianglesSetItem);
                    _Provider = TrianglesTextureProvider;
                    break;
                case LinesTextureSetItem linesSetItem:
                    LinesTextureProvider.Activate(true);
                    LinesTextureProvider.SetProperties(linesSetItem);
                    _Provider = LinesTextureProvider;
                    break;
                default: throw new SwitchCaseNotImplementedException(setItem);
            }
        }

        private int GetProviderIndex(IViewMazeBackgroundTextureProvider _Provider)
        {
            return _Provider switch
            {
                IViewMazeBackgroundCirclesTextureProvider   => 0,
                IViewMazeBackgroundLinesTextureProvider     => 1,
                IViewMazeBackgroundTrianglesTextureProvider => 2,
                _ => throw new SwitchCaseNotImplementedException(_Provider)
            };
        }

        #endregion
    }
}