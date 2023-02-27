using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
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
        
        protected abstract string PrefabName { get; }

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
            ColorProvider.ColorChanged += OnColorChanged;
            Container = _Container;
            m_OnClose = _OnClose;
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, PrefabName);
            PanelRectTransform = go.RTransform();
            PanelRectTransform.SetLocalPosZ(0f);
            PanelRectTransform.SetGoActive(false);
            GetPrefabContentObjects(go);
            LocalizeTextObjectsOnLoad();
            SubscribeButtonEvents();
        }
        
        #endregion

        #region nonpublic methods
        
        protected virtual void OnDialogStartAppearing()
        {
            CommandsProceeder.LockCommands(RmazorUtils.GetCommandsToLockInGameUiMenus(), GetType().Name);
        }

        protected virtual void OnDialogAppeared() { }

        protected virtual void OnDialogDisappearing() { }

        protected virtual void OnDialogDisappeared()
        {
            CommandsProceeder.UnlockCommands(RmazorUtils.GetCommandsToLockInGameUiMenus(), GetType().Name);
        }

        protected abstract void GetPrefabContentObjects(GameObject _Go);

        protected abstract void LocalizeTextObjectsOnLoad();
        protected abstract void SubscribeButtonEvents();
        
        protected virtual void OnColorChanged(int _ColorId, Color _Color) { }

        protected void OnClose(UnityAction _OnFinish = null)
        {
            m_OnClose?.Invoke(_OnFinish);
        }

        protected void PlayButtonClickSound()
        {
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }

        #endregion
    }
}