using System.Collections.Generic;
using Entities;
using UnityEngine;

namespace UI.PanelItems
{
    public abstract class MenuItemBase : MonoBehaviour
    {
        protected ObserverNotifyer Notifyer;

        protected void Init(IEnumerable<IGameObserver> _Observers)
        {
            Notifyer = new ObserverNotifyer();
            Notifyer.AddObservers(_Observers);
        }
    }
}