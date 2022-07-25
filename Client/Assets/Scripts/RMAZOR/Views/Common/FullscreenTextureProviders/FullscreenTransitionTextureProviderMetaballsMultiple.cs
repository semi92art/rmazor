using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Models;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public class FullscreenTransitionTextureProviderMetaBallsMultiple : FullscreenTransitionTextureProviderBase
    {
        private IModelGame                           Model      { get; }
        private IFullscreenTextureProviderMetaBalls1 MetaBalls1 { get; }
        private IFullscreenTextureProviderMetaBalls2 MetaBalls2 { get; }
        private IFullscreenTextureProviderMetaBalls3 MetaBalls3 { get; }
        private IFullscreenTextureProviderMetaBalls4 MetaBalls4 { get; }
        private IFullscreenTextureProviderMetaBalls5 MetaBalls5 { get; }

        public FullscreenTransitionTextureProviderMetaBallsMultiple(
            IPrefabSetManager                    _PrefabSetManager,
            IContainersGetter                    _ContainersGetter,
            ICameraProvider                      _CameraProvider,
            IColorProvider                       _ColorProvider,
            IModelGame                           _Model,
            IFullscreenTextureProviderMetaBalls1 _MetaBalls1,
            IFullscreenTextureProviderMetaBalls2 _MetaBalls2,
            IFullscreenTextureProviderMetaBalls3 _MetaBalls3,
            IFullscreenTextureProviderMetaBalls4 _MetaBalls4,
            IFullscreenTextureProviderMetaBalls5 _MetaBalls5) 
            : base(
                _PrefabSetManager, 
                _ContainersGetter, 
                _CameraProvider, 
                _ColorProvider)
        {
            Model      = _Model;
            MetaBalls1 = _MetaBalls1;
            MetaBalls2 = _MetaBalls2;
            MetaBalls3 = _MetaBalls3;
            MetaBalls4 = _MetaBalls4;
            MetaBalls5 = _MetaBalls5;
        }

        protected override int    SortingOrder      => default;
        protected override string MaterialAssetName => null;

        public override void Init()
        {
            MetaBalls1.Init();
            MetaBalls2.Init();
            MetaBalls3.Init();
            MetaBalls4.Init();
            MetaBalls5.Init();
            RaiseInitialization();
        }

        public override void Activate(bool _Active)
        {
            MetaBalls1.Activate(false);
            MetaBalls2.Activate(false);
            MetaBalls3.Activate(false);
            MetaBalls4.Activate(false);
            MetaBalls5.Activate(false);
            GetProvider().Activate(true);
        }

        public override void SetTransitionValue(float _Value)
        {
            GetProvider().SetTransitionValue(_Value);
        }

        private IFullscreenTransitionTextureProvider GetProvider()
        {
            long levelIndexInGroup = RmazorUtils.GetIndexInGroup(Model.LevelStaging.LevelIndex);
            return levelIndexInGroup switch
            {
                0 => MetaBalls1,
                1 => MetaBalls2,
                2 => MetaBalls3,
                3 => MetaBalls4,
                4 => MetaBalls5,
                _ => MetaBalls1
            };
        }
    }
}