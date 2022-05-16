using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public interface IAdBase
    {
        bool Ready { get; }
        void Init(string _AppId, string _UnitId);
        void LoadAd();
        void ShowAd(UnityAction _OnShown);
    }
}