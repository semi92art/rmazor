using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Models;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public class FullscreenTransitionTextureProviderMultiple : FullscreenTransitionTextureProviderBase
    {
        private IModelGame                                  Model   { get; }
        private IFullscreenTransitionTextureProviderTriaHex TriaHex { get; }
        private IFullscreenTransitionTextureProviderCircles Circles { get; }

        public FullscreenTransitionTextureProviderMultiple(
            IPrefabSetManager                           _PrefabSetManager,
            IContainersGetter                           _ContainersGetter,
            ICameraProvider                             _CameraProvider,
            IColorProvider                              _ColorProvider,
            IModelGame                                  _Model,
            IFullscreenTransitionTextureProviderTriaHex _TriaHex,
            IFullscreenTransitionTextureProviderCircles _Circles) 
            : base(
                _PrefabSetManager, 
                _ContainersGetter, 
                _CameraProvider, 
                _ColorProvider)
        {
            Model   = _Model;
            TriaHex = _TriaHex;
            Circles = _Circles;
        }

        protected override int    SortingOrder      => default;
        protected override string MaterialAssetName => null;

        public override void Init()
        {
            TriaHex.Init();
            Circles.Init();
            RaiseInitialization();
        }

        public override void Activate(bool _Active)
        {
            TriaHex.Activate(false);
            Circles.Activate(false);
            GetProvider().Activate(true);
        }

        public override void SetTransitionValue(float _Value)
        {
            GetProvider().SetTransitionValue(_Value);
        }

        private IFullscreenTransitionTextureProvider GetProvider()
        {
            int group = RmazorUtils.GetGroupIndex(Model.LevelStaging.LevelIndex);
            int mod = group % 2;
            return mod switch
            {
                0 => TriaHex,
                1 => Circles,
                _ => null
            };
        }
    }
}