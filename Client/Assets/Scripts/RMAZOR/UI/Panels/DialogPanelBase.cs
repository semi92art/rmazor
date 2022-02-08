using Common.CameraProviders;
using Common.Enums;
using Common.Providers;
using Common.Ticker;
using DialogViewers;
using Managers;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;
using UI;
using UI.Panels;
using UnityEngine;

namespace RMAZOR.UI.Panels
{
    public abstract class DialogPanelBase : IDialogPanel
    {
        #region inject

        protected IManagersGetter   Managers         { get; }
        protected IUITicker         Ticker           { get; }
        protected IBigDialogViewer  DialogViewer     { get; }
        protected ICameraProvider   CameraProvider   { get; }
        protected IColorProvider    ColorProvider    { get; }

        protected DialogPanelBase(
            IManagersGetter _Managers, 
            IUITicker _Ticker, 
            IBigDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider)
        {
            Managers = _Managers;
            Ticker = _Ticker;
            DialogViewer = _DialogViewer;
            CameraProvider = _CameraProvider;
            ColorProvider = _ColorProvider;
        }

        #endregion

        #region api

        public abstract EUiCategory     Category       { get; }
        public abstract bool            AllowMultiple  { get; }
        public          EAppearingState AppearingState { get; set; }
        public          RectTransform   PanelObject    { get; protected set; }

        public virtual void LoadPanel()
        {
            Ticker.Register(this);
            ColorProvider.ColorChanged += OnColorChanged;
        }

        protected virtual void OnColorChanged(int _ColorId, Color _Color) { }
        public virtual void OnDialogEnable() { }
        public virtual void OnDialogShow() { }
        public virtual void OnDialogHide() { }

        #endregion
    }
}