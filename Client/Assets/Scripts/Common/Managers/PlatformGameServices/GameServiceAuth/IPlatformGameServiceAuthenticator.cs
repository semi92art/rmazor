using UnityEngine.Events;

namespace Common.Managers.PlatformGameServices.GameServiceAuth
{
    public interface IPlatformGameServiceAuthenticator
    {
        void AuthenticatePlatformGameService(UnityAction<bool> _OnFinish);
        bool IsAuthenticated { get; }
    }
}