using Entities;
using Managers;
using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public abstract class DialogPanelBase : GameObservable, IDialogPanel
    {
        protected IBankManager BankManager { get; }
        protected DialogPanelBase(){}
        protected DialogPanelBase(IBankManager _BankManager) => BankManager = _BankManager;
        
        public RectTransform Panel { get; protected set; }
        public abstract void Init();
        public virtual void OnDialogEnable() { }
        public virtual void OnDialogShow() { }
        public virtual void OnDialogHide() { }

    }
}