﻿using Entities;
using UI.Managers;
using UnityEngine;
using UnityGameLoopDI;

namespace UI.Panels
{
    public abstract class DialogPanelBase : GameObservable, IDialogPanel
    {
        public RectTransform Panel { get; protected set; }
        
        protected DialogPanelBase(ITicker _Ticker) : base(_Ticker)
        { }
        
        public abstract void Init();

        public virtual void OnDialogEnable() { }
        public virtual void OnDialogShow() { }
        public virtual void OnDialogHide() { }
    }
}