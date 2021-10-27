using DI.Extensions;
using DialogViewers;
using Entities;
using LeTai.Asset.TranslucentImage;
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
        protected ICameraProvider CameraProvider { get; }

        protected DialogPanelBase(
            IManagersGetter _Managers, 
            IUITicker _Ticker, 
            IDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider)
        {
            Managers = _Managers;
            Ticker = _Ticker;
            DialogViewer = _DialogViewer;
            CameraProvider = _CameraProvider;
        }

        #endregion

        #region api

        public abstract EUiCategory Category { get; }
        public RectTransform Panel { get; protected set; }
        
        public virtual void Init()
        {
            Ticker.Register(this);
        }

        protected void SetTranslucentBackgroundSource(GameObject _Object)
        {
            var translBack = _Object.GetCompItem<TranslucentImage>("translucent_background");
            translBack.source = CameraProvider.MainCamera.GetComponent<TranslucentImageSource>();
        }
        
        public virtual void OnDialogEnable() { }
        public virtual void OnDialogShow() { }
        public virtual void OnDialogHide() { }

        #endregion
    }
}