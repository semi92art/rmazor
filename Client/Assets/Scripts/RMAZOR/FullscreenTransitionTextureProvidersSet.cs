using System.Collections.Generic;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using RMAZOR.Views.Common.FullscreenTextureProviders;

namespace RMAZOR
{
    public interface IFullscreenTransitionTextureProvidersSet : IInit
    {
        IList<IFullscreenTransitionTextureProvider>    GetSet();
        IFullscreenTransitionTextureProvider GetTextureProvider(string _Name = null);
    }
    
    public class FullscreenTransitionTextureProvidersSet : InitBase, IFullscreenTransitionTextureProvidersSet
    {
        #region inject
        
        private IFullscreenTransitionTextureProviderPlayground ProviderPlayground { get; }
        private IFullscreenTransitionTextureProviderCircles    ProviderCircles    { get; }

        private FullscreenTransitionTextureProvidersSet(
            IFullscreenTransitionTextureProviderPlayground _ProviderPlayground,
            IFullscreenTransitionTextureProviderCircles    _ProviderCircles)
        {
            ProviderPlayground = _ProviderPlayground;
            ProviderCircles    = _ProviderCircles;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            ProviderPlayground.Init();
            ProviderCircles   .Init();
            base.Init();
        }

        public IList<IFullscreenTransitionTextureProvider> GetSet()
        {
            return new List<IFullscreenTransitionTextureProvider>
            {
                ProviderPlayground,
                ProviderCircles
            };
        }

        public IFullscreenTransitionTextureProvider GetTextureProvider(string _Name = null)
        {
            return _Name switch
            {
                "playground" => ProviderPlayground,
                "circles"    => ProviderCircles,
                _            => ProviderPlayground
            };
        }

        #endregion
    }
}