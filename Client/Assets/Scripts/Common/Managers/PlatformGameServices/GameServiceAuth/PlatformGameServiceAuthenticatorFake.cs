using UnityEngine.Events;

namespace Common.Managers.PlatformGameServices.GameServiceAuth
{
    public class PlatformGameServiceAuthenticatorFake : IPlatformGameServiceAuthenticator
    {
        public void AuthenticatePlatformGameService(UnityAction<bool> _OnFinish) { }

        public bool IsAuthenticated => false;
    }
}