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

        private IList<TrianglesTextureProps>    m_TrianglesTextureSetItems;
        private IList<Triangles2TextureProps>   m_Triangles2TextureSetItems;
        private bool                            m_IsFirstLoad = true;
        private List<ITextureProps>             m_TextureSetItems;
        
        #endregion
        
        #region inject
        
        // private IViewMazeBackgroundTrianglesTextureProvider  TrianglesTextureProvider  { get; }
        private IViewMazeBackgroundTriangles2TextureProvider Triangles2TextureProvider { get; }
        private IViewGameTicker                              Ticker                    { get; }
        private IViewMazeBackgroundIdleItems                 IdleItems                 { get; }

        public ViewMazeBackgroundTextureController(
            RemoteProperties                             _RemoteProperties,
            // IViewMazeBackgroundTrianglesTextureProvider  _TrianglesTextureProvider,
            IViewMazeBackgroundTriangles2TextureProvider _Triangles2TextureProvider,
            IPrefabSetManager                            _PrefabSetManager,
            IColorProvider                               _ColorProvider,
            IViewGameTicker                              _Ticker,
            IViewMazeBackgroundIdleItems                 _IdleItems) 
            : base(
                _RemoteProperties,
                _ColorProvider, 
                _PrefabSetManager)
        {
            Ticker                    = _Ticker;
            IdleItems                 = _IdleItems;
            // TrianglesTextureProvider  = _TrianglesTextureProvider;
            Triangles2TextureProvider = _Triangles2TextureProvider;
        }

        #endregion

        #region api

        public override void Init()
        {
            Ticker.Register(this);
            // TrianglesTextureProvider  .Init();
            Triangles2TextureProvider .Init();
            IdleItems                 .Init();
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
            m_TrianglesTextureSetItems = RemoteProperties.TrianglesTextureSet;
            if (m_TrianglesTextureSetItems.NullOrEmpty())
            {
                var trianglesTextureSet = PrefabSetManager.GetObject<TrianglesTexturePropsSetScriptableObject>
                    (set, "triangles_texture_set");
                m_TrianglesTextureSetItems = trianglesTextureSet.set;
            }
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
                // .Concat(m_Triangles2TextureSetItems)
                .OrderBy(_Item => CalculateTextureHash(
                    _Item.ToString(null, null)))
                
                .Where(_Props => _Props.InUse)
                .ToList();
        }

        private void OnLevelLoaded(LevelStageArgs _Args)
        {
            ActivateTexturePropertiesSet(_Args.LevelIndex, out var provider);
            IdleItems.SetSpawnPool(GetProviderIndex(provider));
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

        private void ActivateTexturePropertiesSet(long _LevelIndex, out IViewMazeBackgroundTextureProvider _Provider)
        {
            int group = RmazorUtils.GetGroupIndex(_LevelIndex);
            int idx = group % m_TextureSetItems.Count;
            Dbg.Log("group: " + group + ", idx: " + idx);
            var setItem = m_TextureSetItems[idx];
            // TrianglesTextureProvider.Activate(false);
            Triangles2TextureProvider.Activate(false);
            switch (setItem)
            {
                // case TrianglesTextureProps trianglesProps:
                //     TrianglesTextureProvider.Activate(true);
                //     TrianglesTextureProvider.SetProperties(trianglesProps);
                //     _Provider = TrianglesTextureProvider;
                //     break;
                case Triangles2TextureProps triangles2Props:
                    Triangles2TextureProvider.Activate(true);
                    Triangles2TextureProvider.SetProperties(triangles2Props);
                    _Provider = Triangles2TextureProvider;
                    break;
                default: throw new SwitchCaseNotImplementedException(setItem);
            }
        }

        private static int GetProviderIndex(IViewMazeBackgroundTextureProvider _Provider)
        {
            return _Provider switch
            {
                IViewMazeBackgroundTrianglesTextureProvider =>  0,
                IViewMazeBackgroundTriangles2TextureProvider => 1,
                _ => throw new SwitchCaseNotImplementedException(_Provider)
            };
        }

        #endregion
    }
}