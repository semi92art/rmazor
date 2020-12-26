using Entities;
using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public abstract class DialogPanelBase : GameObservable, IDialogPanel
    {
        public RectTransform Panel { get; protected set; }
        public abstract void Init();

        public virtual void OnDialogEnable() { }
        public virtual void OnDialogShow() { }
        public virtual void OnDialogHide() { }

    }
}