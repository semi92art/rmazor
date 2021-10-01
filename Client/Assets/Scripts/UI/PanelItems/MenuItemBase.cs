using Entities;
using UnityEngine;

namespace UI.PanelItems
{
    public abstract class MenuItemBase : MonoBehaviour
    {
        protected IManagersGetter Managers { get; private set; }
        
        protected void Init( 
            IManagersGetter _Managers)
        {
            Managers = _Managers;
        }
    }
}