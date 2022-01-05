using UnityEngine.Events;

namespace Managers.Advertising
{
    public interface IAdBase
    {
        bool Ready { get; }
        void Init(string _UnitId);
        void ShowAd(UnityAction _OnShown);
        void LoadAd();
    }
}