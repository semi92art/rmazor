using UnityEngine;
using UnityEngine.Events;

namespace Common.Managers.PlatformGameServices.GameServiceAuth
{
    public abstract class PlatformGameServiceAuthenticatorBase : IPlatformGameServiceAuthenticator
    {
        public virtual bool IsAuthenticated => Social.localUser.authenticated;
        
        public virtual void AuthenticatePlatformGameService(UnityAction<bool> _OnFinish)
        {
            Social.localUser.Authenticate(_Success =>
            {
                _OnFinish?.Invoke(_Success);
                if (!_Success)
                    Dbg.LogError("Failed to authenticate to Social");
            });
        }
    }
}