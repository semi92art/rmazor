using Common.UI;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using RMAZOR.Managers;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
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

        protected IManagersGetter             Managers          { get; }
        protected IUITicker                   Ticker            { get; }
        protected ICameraProvider             CameraProvider    { get; }
        protected IColorProvider              ColorProvider     { get; }
        protected IViewTimePauser             TimePauser        { get; }
        protected IViewInputCommandsProceeder CommandsProceeder { get; }

        protected DialogPanelBase(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            Managers          = _Managers;
            Ticker            = _Ticker;
            CameraProvider    = _CameraProvider;
            ColorProvider     = _ColorProvider;
            TimePauser        = _TimePauser;
            CommandsProceeder = _CommandsProceeder;
        }

        #endregion

        #region api

        public abstract int DialogViewerId { get; }

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


        public virtual void OnDialogStartAppearing()
        {
            CommandsProceeder.LockCommands(RmazorUtils.GetCommandsToLockInUiMenues(), GetType().Name);
        }
        public virtual void OnDialogAppeared()     { }

        public virtual void OnDialogDisappearing() { }

        public virtual void OnDialogDisappeared()
        {
            CommandsProceeder.UnlockCommands(RmazorUtils.GetCommandsToLockInUiMenues(), GetType().Name);
        }

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