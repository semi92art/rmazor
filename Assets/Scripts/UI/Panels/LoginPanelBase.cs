using DialogViewers;
using TMPro;
using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public abstract class LoginPanelBase: IMenuDialogPanel
    {
        #region protected members
        
        protected readonly IMenuDialogViewer MenuDialogViewer;
        protected TMP_InputField m_LoginInputField;
        protected TMP_InputField m_PasswordInputField;
        protected TextMeshProUGUI m_LoginErrorHandler;
        protected TextMeshProUGUI m_PasswordErrorHandler;
        
        #endregion

        protected LoginPanelBase(IMenuDialogViewer _MenuDialogViewer)
        {
            MenuDialogViewer = _MenuDialogViewer;
        }
        
        
        #region api

        public MenuUiCategory Category => MenuUiCategory.Login;
        public RectTransform Panel { get; private set; }

        public void Show()
        {
            Panel = Create();
            MenuDialogViewer.Show( this);
        }

        #endregion
        
        #region protected methods

        protected abstract RectTransform Create();
        
        protected void SetLoginError(string _Text)
        {
            m_LoginErrorHandler.text = _Text;
        }

        protected void SetPasswordError(string _Text)
        {
            m_PasswordErrorHandler.text = _Text;
        }
        
        protected virtual bool IsNoError()
        {
            return string.IsNullOrEmpty(m_LoginErrorHandler.text)
                   && string.IsNullOrEmpty(m_PasswordErrorHandler.text);
        }

        protected virtual void CleanErrorHandlers()
        {
            m_LoginErrorHandler.text = string.Empty;
            m_PasswordErrorHandler.text = string.Empty;
        }

        #endregion
    }
}