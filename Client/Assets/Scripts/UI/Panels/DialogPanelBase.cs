using Entities;
using Ticker;
using UnityEngine;

namespace UI.Panels
{
    public abstract class DialogPanelBase : IDialogPanel
    {
        public IGameObservable GameObservable { get; }
        protected ITicker Ticker { get; }
        public RectTransform Panel { get; protected set; }

        protected DialogPanelBase(IGameObservable _GameObservable, ITicker _Ticker)
        {
            GameObservable = _GameObservable;
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