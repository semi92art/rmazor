using Entities;
using UnityEngine;

namespace UI.PanelItems
{
    public abstract class MenuItemBase : MonoBehaviour
    {
        protected IGameObservable GameObservable { get; private set; }
        
        protected void Init( 
            IGameObservable _GameObservable)
        {
            GameObservable = _GameObservable;
        }
    }
}