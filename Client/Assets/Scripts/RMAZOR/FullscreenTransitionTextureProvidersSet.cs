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
        
        private IFullscreenTransitionTextureProviderPlayground       ProviderPlayground       { get; }
        private IFullscreenTransitionTextureProviderCircles          ProviderCircles          { get; }
        private IFullscreenTransitionTextureProviderCircles2         ProviderCircles2         { get; }
        private IFullscreenTransitionTextureProviderCirclesToSquares ProviderCirclesToSquares { get; }

        private FullscreenTransitionTextureProvidersSet(
            IFullscreenTransitionTextureProviderPlayground       _ProviderPlayground,
            IFullscreenTransitionTextureProviderCircles          _ProviderCircles,
            IFullscreenTransitionTextureProviderCircles2         _ProviderCircles2,
            IFullscreenTransitionTextureProviderCirclesToSquares _ProviderCirclesToSquares)
        {
            ProviderPlayground       = _ProviderPlayground;
            ProviderCircles          = _ProviderCircles;
            ProviderCircles2         = _ProviderCircles2;
            ProviderCirclesToSquares = _ProviderCirclesToSquares;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            ProviderPlayground      .Init();
            ProviderCircles         .Init();
            ProviderCircles2        .Init();
            ProviderCirclesToSquares.Init();
            base.Init();
        }

        public IList<IFullscreenTransitionTextureProvider> GetSet()
        {
            return new List<IFullscreenTransitionTextureProvider>
            {
                ProviderPlayground,
                ProviderCircles,
                ProviderCircles2,
                ProviderCirclesToSquares
            };
        }

        public IFullscreenTransitionTextureProvider GetTextureProvider(string _Name = null)
        {
            return _Name switch
            {
                "circles_to_squares" => ProviderCirclesToSquares,
                "playground"         => ProviderPlayground,
                "circles"            => ProviderCircles,
                "circles_2"          => ProviderCircles2,
                _                    => ProviderCirclesToSquares
            };
        }

        #endregion
    }
}