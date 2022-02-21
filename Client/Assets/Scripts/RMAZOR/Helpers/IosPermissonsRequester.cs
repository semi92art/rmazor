using Common;
using Common.Entities;
using SA.iOS.AppTrackingTransparency;

namespace RMAZOR.Helpers
{
    public class IosPermissonsRequester : IPermissionsRequester
    {
        public Entity<bool> RequestPermissions()
        {
            var entity = new Entity<bool>();
            ISN_ATTrackingManager.RequestTrackingAuthorization(_Status =>
            {
                Dbg.Log($"Tracking authorization status: {_Status}");
                entity.Result = EEntityResult.Success;
            });
            return entity;
        }
    }
}