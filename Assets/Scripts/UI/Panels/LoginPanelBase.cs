using TMPro;
using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public abstract class LoginPanelBase: IDialogPanel
    {
        #region protected members
        
        protected readonly IDialogViewer m_DialogViewer;
        protected TMP_InputField m_LoginInputField;
        protected TMP_InputField m_PasswordInputField;
        protected TextMeshProUGUI m_LoginErrorHandler;
        protected TextMeshProUGUI m_PasswordErrorHandler;
        
        #endregion

        protected LoginPanelBase(IDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }
        
        
        #region api

        public UiCategory Category => UiCategory.Login;
        public RectTransform Panel { get; private set; }

        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show( this);
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