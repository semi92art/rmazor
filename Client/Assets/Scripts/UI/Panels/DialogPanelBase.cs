using DialogViewers;
using Entities;
using Ticker;
using UnityEngine;

namespace UI.Panels
{
    public abstract class DialogPanelBase : IDialogPanel
    {
        #region inject

        protected IManagersGetter Managers { get; }
        protected IUITicker Ticker { get; }
        protected IDialogViewer DialogViewer { get; }

        protected DialogPanelBase(
            IManagersGetter _Managers, 
            IUITicker _Ticker, 
            IDialogViewer _DialogViewer)
        {
            Managers = _Managers;
            Ticker = _Ticker;
            DialogViewer = _DialogViewer;
        }

        #endregion

        #region api

        public abstract EUiCategory Category { get; }
        public RectTransform Panel { get; protected set; }
        
        public virtual void Init()
        {
            Ticker.Register(this);
        }
        public virtual void OnDialogEnable() { }
        public virtual void OnDialogShow() { }
        public virtual void OnDialogHide() { }

        #endregion
    }
}