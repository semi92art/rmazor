using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public interface IAppodealInterstitialAd : IAdBase { }
    
    public class AppodealInterstitialAd : IAppodealInterstitialAd
    {
        public bool Ready { get; }
        public void Init(string        _UnitId)
        {
            throw new System.NotImplementedException();
        }

        public void ShowAd(UnityAction _OnShown)
        {
            throw new System.NotImplementedException();
        }

        public void LoadAd()
        {
            throw new System.NotImplementedException();
        }
    }
}