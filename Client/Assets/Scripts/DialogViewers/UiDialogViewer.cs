using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Ticker;
using UI;
using UI.Factories;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace DialogViewers
{
    public class UiDialogViewer : DialogViewerBase
    {
        #region nonpublic members

        private static int AkEnableGoBackButton => AnimKeys.Anim;
        private static int AkEnableCloseButton => AnimKeys.Anim2;
        private static int AkDisableGoBackButton => AnimKeys.Stop;
        private static int AkDisableCloseButton => AnimKeys.Stop2;
        private static int AkDisableBothButtons => AnimKeys.Stop3;
        private static int AkState => AnimKeys.State;
        
        private const int AkStateBothButtonsDisabled = 0;
        private const int AkStateGoBackButtonEnabled = 1;
        private const int AkStateBothButtonsEnabled = 2;
        
        private Button m_GoBackButton;
        private Button m_CloseButton;
        private Animator m_ButtonsAnim;

        #endregion

        #region inject

        public UiDialogViewer(IManagersGetter _Managers, IUITicker _Ticker) 
            : base(_Managers, _Ticker) { }
        
        #endregion

        #region api

        public override void Init(RectTransform _Parent)
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
                "dialog_viewers",
                "dialog_viewer");
            
            m_Background = go.GetCompItem<Image>("background");
            m_DialogContainer = go.GetCompItem<RectTransform>("dialog_container");
            m_GoBackButton = go.GetCompItem<Button>("go_back_button");
            m_CloseButton = go.GetCompItem<Button>("close_button");
            m_ButtonsAnim = go.GetCompItem<Animator>("buttons_animator");

            var borderColor = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiBorderDefault);
            var backgroundColor = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiDialogBackground);
            m_GoBackButton.GetCompItem<Image>("border").color = borderColor;
            m_CloseButton.GetCompItem<Image>("border").color = borderColor;
            m_GoBackButton.GetCompItem<Image>("background").color = backgroundColor;
            m_CloseButton.GetCompItem<Image>("background").color = backgroundColor;
            m_GoBackButton.GetCompItem<Image>("icon").color = borderColor;
            m_CloseButton.GetCompItem<Image>("icon").color = borderColor;
            
            m_GoBackButton.SetOnClick(() =>
            {
                Managers.Notify(_SM => _SM.PlayClip(AudioClipNames.UIButtonClick));
                Back();
            });
            m_CloseButton.SetOnClick(() =>
            {
                Managers.Notify(_SM => _SM.PlayClip(AudioClipNames.UIButtonClick));
                CloseAll();
            });
            base.Init(_Parent);
        }

        public override void Show(IDialogPanel _ItemTo, bool _HidePrevious = true)
        {
            var to = new Panel(_ItemTo);
            ShowCore(to, _HidePrevious, false);
        }

        public override void AddNotDialogItem(RectTransform _Item, EUiCategory _Categories)
        {
            if (NotDialogs.ContainsKey(_Item))
                return;
            NotDialogs.Add(_Item, new VisibleInCategories(_Categories, _Item.gameObject.activeSelf));
            GraphicsAlphas.Add(_Item.GetInstanceID(), new GraphicAlphas(_Item));
        }
        
        public override void UpdateTick()
        {
            if (!NotificationViewer.IsShowing && Input.GetKeyDown(KeyCode.Escape))
                Back();
        }

        #endregion

        #region nonpublic methods
        
        protected override void FinishShowing(
            Panel _ItemFrom,
            Panel _ItemTo,
            bool _GoBack,
            RectTransform _PanelTo)
        {
            base.FinishShowing(_ItemFrom, _ItemTo, _GoBack, _PanelTo);
            SetBackAndCloseButtonsState(_GoBack, _PanelTo == null);
        }

        private void SetBackAndCloseButtonsState(bool _GoBack, bool _CloseAll)
        {
            if (m_GoBackButton == null || m_CloseButton == null)
                return;
            int state = m_ButtonsAnim.GetInteger(AkState);
            switch (state)
            {
                case AkStateBothButtonsDisabled:
                    if (!_CloseAll)
                    {
                        m_ButtonsAnim.SetTrigger(AkEnableGoBackButton);
                        m_ButtonsAnim.SetInteger(AkState, AkStateGoBackButtonEnabled);    
                    }
                    break;
                case AkStateGoBackButtonEnabled:
                    m_ButtonsAnim.SetTrigger(_GoBack ? AkDisableGoBackButton : AkEnableCloseButton);
                    m_ButtonsAnim.SetInteger(AkState, _GoBack ?
                        AkStateBothButtonsDisabled : AkStateBothButtonsEnabled);
                    break;
                case AkStateBothButtonsEnabled:
                    int? trigger = null;
                    if (_CloseAll) trigger = AkDisableBothButtons;
                    else if (_GoBack && PanelStack.GetItem(1) == null) trigger = AkDisableCloseButton;
                    int? newState;
                    if (_CloseAll) newState = AkStateBothButtonsDisabled;
                    else if (_GoBack && PanelStack.GetItem(1) == null) newState = AkStateGoBackButtonEnabled;
                    else newState = AkStateBothButtonsEnabled;

                    if (trigger.HasValue)
                        m_ButtonsAnim.SetTrigger(trigger.Value);
                    m_ButtonsAnim.SetInteger(AkState, newState.Value);
                    break;
            }
        }

        #endregion
    }
}