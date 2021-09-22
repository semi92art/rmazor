using Entities;
using Ticker;
using UnityEngine;

namespace UI.Panels
{
    public abstract class DialogPanelBase : GameObservable, IDialogPanel
    {
        public RectTransform Panel { get; protected set; }
        
        protected DialogPanelBase(IUITicker _UITicker) : base(_UITicker)
        { }
        
        public abstract void Init();

        public virtual void OnDialogEnable() { }
        public virtual void OnDialogShow() { }
        public virtual void OnDialogHide() { }
    }
}