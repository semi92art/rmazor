using Common.CameraProviders;
using Common.Enums;
using Common.Exceptions;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using RMAZOR.Managers;
using UnityEngine;

namespace RMAZOR.UI.Panels
{
    public abstract class DialogPanelBase : IDialogPanel
    {
        #region nonpublic members

        private EAppearingState m_AppearingState = EAppearingState.Dissapeared;

        #endregion
        
        #region inject

        protected IManagersGetter          Managers                { get; }
        protected IUITicker                Ticker                  { get; }
        protected IDialogViewersController DialogViewersController { get; }
        protected ICameraProvider          CameraProvider          { get; }
        protected IColorProvider           ColorProvider           { get; }

        protected DialogPanelBase(
            IManagersGetter          _Managers,
            IUITicker                _Ticker,
            IDialogViewersController _DialogViewersController,
            ICameraProvider          _CameraProvider,
            IColorProvider           _ColorProvider)
        {
            Managers                = _Managers;
            Ticker                  = _Ticker;
            DialogViewersController = _DialogViewersController;
            CameraProvider          = _CameraProvider;
            ColorProvider           = _ColorProvider;
        }

        #endregion

        #region api

        public abstract EUiCategory     Category       { get; }
        public abstract bool            AllowMultiple  { get; }

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

        public         RectTransform PanelObject { get; protected set; }
        public virtual Animator      Animator    => null;

        public virtual void LoadPanel()
        {
            Ticker.Register(this);
            ColorProvider.ColorChanged += OnColorChanged;
        }

        protected virtual void OnColorChanged(int _ColorId, Color _Color) { }
        
        public virtual void OnDialogStartAppearing()   { }
        public virtual void OnDialogAppeared()     { }
        public virtual void OnDialogDisappearing() { }
        public virtual void OnDialogDisappeared()  { }

        #endregion
    }
}