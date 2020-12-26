using System.Collections.Generic;
using DialogViewers;
using Entities;
using TMPro;
using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public abstract class LoginPanelBase: DialogPanelBase, IMenuUiCategory
    {
        #region protected members
        
        protected readonly IMenuDialogViewer DialogViewer;
        protected TMP_InputField LoginInputField;
        protected TMP_InputField PasswordInputField;
        protected TextMeshProUGUI LoginErrorHandler;
        protected TextMeshProUGUI PasswordErrorHandler;
        
        #endregion

        protected LoginPanelBase(IMenuDialogViewer _DialogViewer)
        {
            DialogViewer = _DialogViewer;
        }
        
        
        #region api

        public abstract MenuUiCategory Category { get; }

        #endregion
        
        #region protected methods

        protected void SetLoginError(string _Text)
        {
            LoginErrorHandler.text = _Text;
        }

        protected void SetPasswordError(string _Text)
        {
            PasswordErrorHandler.text = _Text;
        }
        
        protected virtual bool IsNoError()
        {
            return string.IsNullOrEmpty(LoginErrorHandler.text)
                   && string.IsNullOrEmpty(PasswordErrorHandler.text);
        }

        protected virtual void CleanErrorHandlers()
        {
            LoginErrorHandler.text = string.Empty;
            PasswordErrorHandler.text = string.Empty;
        }

        #endregion
    }
}