using Common.Managers;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using UnityEngine;

namespace RMAZOR.Camera_Providers
{
    public interface ICameraProviderFake : ICameraProvider { }
    
    public class CameraProviderFake : CameraProviderBase, ICameraProviderFake
    {
        protected override string CameraName => "Static Camera";

        public CameraProviderFake(
            IPrefabSetManager _PrefabSetManager, 
            IViewGameTicker   _ViewGameTicker) 
            : base(_PrefabSetManager, _ViewGameTicker) { }
        
        public override Camera Camera => Camera.main;
    }
}