using mazing.common.Runtime.Entities;

namespace RMAZOR.Helpers
{
    public interface IPermissionsRequester
    {
        Entity<bool> RequestPermissions();
    }

    public class PermissionsRequesterFake : IPermissionsRequester
    {
        public Entity<bool> RequestPermissions()
        {
            return new Entity<bool> {Result = EEntityResult.Success};
        }
    }
}