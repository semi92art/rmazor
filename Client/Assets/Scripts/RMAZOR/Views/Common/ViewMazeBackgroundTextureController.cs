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
using RMAZOR.Views.Common.FullscreenTextureProviders;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;

namespace RMAZOR.Views.Common
{
    public class ViewMazeBackgroundTextureController : ViewMazeBackgroundTextureControllerBase, IUpdateTick
    {
        #region nonpublic members

        private IList<Triangles2TextureProps>   m_Triangles2TextureSetItems;
        private bool                            m_IsFirstLoad = true;
        private List<ITextureProps>             m_TextureSetItems;
        
        #endregion
        
        #region inject

        private IFullscreenTextureProviderTriangles2 Triangles2TextureProvider { get; }
        private IFullscreenTextureProviderSolidColor TextureProviderSolidColor { get; }
        private IViewGameTicker                      Ticker                    { get; }
        private IViewMazeBackgroundIdleItems         IdleItems                 { get; }

        private ViewMazeBackgroundTextureController(
            IRemotePropertiesRmazor              _RemoteProperties,
            IColorProvider                       _ColorProvider,
            IPrefabSetManager                    _PrefabSetManager,
            IFullscreenTextureProviderTriangles2 _Triangles2TextureProvider,
            IFullscreenTextureProviderSolidColor _TextureProviderSolidColor,
            IViewGameTicker                      _Ticker,
            IViewMazeBackgroundIdleItems         _IdleItems) 
            : base(
                _RemoteProperties,
                _ColorProvider, 
                _PrefabSetManager)
        {
            Triangles2TextureProvider = _Triangles2TextureProvider;
            TextureProviderSolidColor = _TextureProviderSolidColor;
            Ticker                    = _Ticker;
            IdleItems                 = _IdleItems;
        }

        #endregion

        #region api

        public override void Init()
        {
            Ticker.Register(this);
            Triangles2TextureProvider .Init();
            TextureProviderSolidColor .Init();
            IdleItems                 .Init();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            IdleItems.OnLevelStageChanged(_Args);
            if (_Args.LevelStage == ELevelStage.Loaded)
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

        private void ActivateTexturePropertiesSet(long _LevelIndex, out IFullscreenTextureProvider _Provider)
        {
            _Provider = TextureProviderSolidColor;
            TextureProviderSolidColor.Activate(true);
            Triangles2TextureProvider.Activate(false);

            // int group = RmazorUtils.GetGroupIndex(_LevelIndex);
            // int idx = group % m_TextureSetItems.Count;
            // var setItem = m_TextureSetItems[idx];
            // Triangles2TextureProvider.Activate(false);
            // switch (setItem)
            // {
            //     case Triangles2TextureProps triangles2Props:
            //         Triangles2TextureProvider.Activate(true);
            //         Triangles2TextureProvider.SetProperties(triangles2Props);
            //         _Provider = Triangles2TextureProvider;
            //         break;
            //     default: throw new SwitchCaseNotImplementedException(setItem);
            // }
        }

        private static int GetProviderIndex(IFullscreenTextureProvider _Provider)
        {
            return _Provider switch
            {
                IFullscreenTextureProviderTriangles2 => 1,
                IFullscreenTextureProviderSolidColor => 1,
                _ => throw new SwitchCaseNotImplementedException(_Provider)
            };
        }

        #endregion
    }
}