using System.Collections.Generic;
using Entities;
using Ticker;
using UnityEngine;

namespace UI.PanelItems
{
    public abstract class MenuItemBase : MonoBehaviour
    {
        protected ObserverNotifyer Notifyer;

        protected void Init(IEnumerable<GameObserver> _Observers, IUITicker _Ticker)
        {
            Notifyer = new ObserverNotifyer(_Ticker);
            Notifyer.AddObservers(_Observers);
        }
    }
}