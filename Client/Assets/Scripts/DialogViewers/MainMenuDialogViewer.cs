﻿using System.Collections.Generic;
using Constants;
using Entities;
using Extensions;
using GameHelpers;
using TMPro;
using UI;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace DialogViewers
{
    public interface IMenuDialogViewer : IDialogViewer
    {
        void AddNotDialogItem(RectTransform _Item, MenuUiCategory _Categories);
    }
    
    public class MainMenuDialogViewer : DialogViewerBase, IMenuDialogViewer
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
        private ObserverNotifyer m_Notifyer;

        #endregion
    
        #region engine methods

        private void Start()
        {
            m_GoBackButton = gameObject.GetCompItem<Button>("go_back_button");
            m_CloseButton = gameObject.GetCompItem<Button>("close_button");
            m_ButtonsAnim = gameObject.GetCompItem<Animator>("buttons_animator");

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
                m_Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick);
                Back();
            });
            m_CloseButton.SetOnClick(() =>
            {
                m_Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick);
                CloseAll();
            });
        }
        
        #endregion

        #region api

        public static IMenuDialogViewer Create(
            RectTransform _Parent,
            IEnumerable<GameObserver> _Observers)
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
                "dialog_viewers",
                "main_menu_viewer");
            var result = go.GetComponent<MainMenuDialogViewer>();
            result.m_Notifyer = new ObserverNotifyer();
            result.m_Notifyer.AddObservers(_Observers);
            return result;
        }

        public override void Show(IDialogPanel _ItemTo, bool _HidePrevious = true)
        {
            var to = new Panel(_ItemTo);
            ShowCore(to, _HidePrevious, false, MenuUiCategoryType);
        }

        public void AddNotDialogItem(RectTransform _Item, MenuUiCategory _Categories)
        {
            if (NotDialogs.ContainsKey(_Item))
                return;
            NotDialogs.Add(_Item, new VisibleInCategories(_Categories, _Item.gameObject.activeSelf));
            GraphicsAlphas.Add(_Item.GetInstanceID(), new GraphicAlphas(_Item));
        }

        #endregion

        #region nonpublic methods
        
        protected override void FinishShowing(
            Panel _ItemFrom,
            Panel _ItemTo,
            bool _GoBack,
            RectTransform _PanelTo,
            int _UiCategoryType)
        {
            base.FinishShowing(_ItemFrom, _ItemTo, _GoBack, _PanelTo, _UiCategoryType);
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
        
        #region engine methods

        protected override void Update()
        {
            if (!MainMenuNotificationViewer.IsShowing && Input.GetKeyDown(KeyCode.Escape))
                Back();
        }

        #endregion
    }
}