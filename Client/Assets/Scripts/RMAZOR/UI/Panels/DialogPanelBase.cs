using Common.CameraProviders;
using Common.Enums;
using Common.Exceptions;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.UI.DialogViewers;
using RMAZOR.Managers;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.UI.Panels
{
    public abstract class DialogPanelBase : IDialogPanel
    {
        #region nonpublic members

        private EAppearingState  m_AppearingState = EAppearingState.Dissapeared;
        private ClosePanelAction m_OnClose;

        protected RectTransform Container;

        #endregion
        
        #region inject

        protected IManagersGetter          Managers                { get; }
        protected IUITicker                Ticker                  { get; }
        protected ICameraProvider          CameraProvider          { get; }
        protected IColorProvider           ColorProvider           { get; }

        protected DialogPanelBase(
            IManagersGetter          _Managers,
            IUITicker                _Ticker,
            ICameraProvider          _CameraProvider,
            IColorProvider           _ColorProvider)
        {
            Managers                = _Managers;
            Ticker                  = _Ticker;
            CameraProvider          = _CameraProvider;
            ColorProvider           = _ColorProvider;
        }

        #endregion

        #region api

        public abstract EDialogViewerType DialogViewerType { get; }
        public abstract EUiCategory       Category         { get; }

        public EAppearingState AppearingState
        {
            get => m_AppearingState;
            set
            {
                m_AppearingState = value;
                switch (value)
                {
                    case EAppearingState.Appearing:    OnDialogStartAppearing(); break;
                    case EAppearingState.Appeared:     OnDialogAppeared();       break;
                    case EAppearingState.Dissapearing: OnDialogDisappearing();   break;
                    case EAppearingState.Dissapeared:  OnDialogDisappeared();    break;
                    default: throw new SwitchCaseNotImplementedException(value);
                }
            }
        }

        public         RectTransform PanelRectTransform { get; protected set; }
        public virtual Animator      Animator    => null;

        public virtual void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            Ticker.Register(this);
            Container = _Container;
            ColorProvider.ColorChanged += OnColorChanged;
            m_OnClose = _OnClose;
        }

        
        public virtual void OnDialogStartAppearing()   { }
        public virtual void OnDialogAppeared()     { }
        public virtual void OnDialogDisappearing() { }
        public virtual void OnDialogDisappeared()  { }

        #endregion

        #region nonpublic methods

        protected virtual void OnColorChanged(int _ColorId, Color _Color) { }

        protected virtual void OnClose(UnityAction _OnFinish)
        {
            m_OnClose?.Invoke(_OnFinish);
        }

        #endregion
    }
}