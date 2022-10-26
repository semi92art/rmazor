using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Helpers;
using Common.Managers;
using RMAZOR.Helpers;
using RMAZOR.Views.Common.FullscreenTextureProviders;

namespace RMAZOR.Views.Common
{
    public interface IBackgroundTextureProviderSet : IInit
    {
        IFullscreenTextureProvider        GetProvider(string _Name);
        IList<IFullscreenTextureProvider> GetSet();
    }

    public class BackgroundTextureProviderSetImpl : InitBase, IBackgroundTextureProviderSet
    {
        #region nonpublic members

        private readonly Dictionary<string, IFullscreenTextureProvider> m_TextureProvidersDict
            = new Dictionary<string, IFullscreenTextureProvider>();

        #endregion
        
        
        #region inject
        
        private ViewSettings                         ViewSettings              { get; }
        private IFullscreenTextureProviderCustom     TextureProviderCustom     { get; }
        private IFullscreenTextureProviderTriangles2 TextureProviderTriangles2 { get; }
        private IPrefabSetManager                    PrefabSetManager          { get; }

        public BackgroundTextureProviderSetImpl(
            ViewSettings                         _ViewSettings,
            IFullscreenTextureProviderCustom     _TextureProviderCustom,
            IFullscreenTextureProviderTriangles2 _TextureProviderTriangles2,
            IPrefabSetManager                    _PrefabSetManager)
        {
            ViewSettings              = _ViewSettings;
            TextureProviderCustom     = _TextureProviderCustom;
            TextureProviderTriangles2 = _TextureProviderTriangles2;
            PrefabSetManager          = _PrefabSetManager;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            InitTextureProviders();
            base.Init();
        }

        public IFullscreenTextureProvider GetProvider(string _Name)
        {
            string name = m_TextureProvidersDict.ContainsKey(_Name) ? _Name : "solid";
            Dbg.Log(name);
            return m_TextureProvidersDict[name];
        }

        public IList<IFullscreenTextureProvider> GetSet()
        {
            return m_TextureProvidersDict.Values.ToList();
        }

        #endregion

        #region nonpublic members

        private void InitTextureProviders()
        {
            var setRawScrObj = PrefabSetManager.GetObject<MainBackgroundMaterialInfoSetScriptableObject>(
                "background", "main_background_set", EPrefabSource.Bundle);
            var providersToInitNames = ViewSettings.backgroundTextures
                .Split(',')
                .ToList();
            foreach (string name in providersToInitNames)
            {
                var setItem = setRawScrObj.set.FirstOrDefault(
                    _SetItem =>
                        _SetItem.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (setItem == null)
                    continue;
                IFullscreenTextureProvider provider;
                if (setItem.name == "triangles_2")
                {
                    provider = TextureProviderTriangles2;
                }
                else
                {
                    provider = (IFullscreenTextureProvider)TextureProviderCustom.Clone();
                }
                provider.SetMaterial(setItem.material);
                provider.Init();
                m_TextureProvidersDict.Add(setItem.name, provider);
            }
        }
        
        #endregion
    }
}