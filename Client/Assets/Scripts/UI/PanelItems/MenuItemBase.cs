using System.Collections.Generic;
using Entities;
using UnityEngine;
using UnityGameLoopDI;

namespace UI.PanelItems
{
    public abstract class MenuItemBase : MonoBehaviour
    {
        protected ObserverNotifyer Notifyer;

        protected void Init(IEnumerable<GameObserver> _Observers, ITicker _Ticker)
        {
            Notifyer = new ObserverNotifyer(_Ticker);
            Notifyer.AddObservers(_Observers);
        }
    }
}