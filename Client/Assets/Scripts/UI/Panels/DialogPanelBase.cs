using Entities;
using Ticker;
using UnityEngine;

namespace UI.Panels
{
    public abstract class DialogPanelBase : IDialogPanel
    {
        public IManagersGetter Managers { get; }
        protected ITicker Ticker { get; }
        public RectTransform Panel { get; protected set; }

        protected DialogPanelBase(IManagersGetter _Managers, ITicker _Ticker)
        {
            Managers = _Managers;
            Ticker = _Ticker;
        }

        public virtual void Init()
        {
            Ticker.Register(this);
        }
        public virtual void OnDialogEnable() { }
        public virtual void OnDialogShow() { }
        public virtual void OnDialogHide() { }
    }
}