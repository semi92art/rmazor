using System;
using Network.PacketArgs;
using Newtonsoft.Json;
using UnityEngine;
using Utility = Utils.Utility;

namespace Network
{
    public abstract class PacketBase : IPacket
    {
        #region public properties
        
        public abstract int Id { get; }
        public abstract string Url { get; }
        public virtual string Method => "POST";
        public virtual bool OnlyOne => false;
        public virtual bool Resend => false;
        public object Request { get; }
        public string ResponseRaw { get; private set; }
        
        public ErrorResponseArgs ErrorMessage {
            get
            {
                if (m_ErrorMessage == null)
                    return new ErrorResponseArgs
                    {
                        Id = 500,
                        Message = "Internal server error"
                    };
                return m_ErrorMessage;
            }
        }
        
        public long ResponseCode { get; set; }

        #endregion
        
        #region private fields

        private ErrorResponseArgs m_ErrorMessage;
        private Action m_Success;
        private Action m_Fail;
        private Action m_Cancel;
        
        #endregion
        
        #region constructor

        protected PacketBase(object _Request)
        {
            Request = _Request;
        }

        #endregion
        
        #region private methods
        
        private void InvokeSuccess()
        {
            try
            {
                m_Success?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnSuccess error: {e.Message}");
            }
        }

        private void InvokeFail()
        {
            try
            {
                m_Fail?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnFail error: {e.Message}");
            }
        }

        private void InvokeCancel()
        {
            try
            {
                m_Cancel?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnCancel error: {e.Message}");
            }
        }
        
        #endregion
        
        #region public methods

        public virtual IPacket OnSuccess(Action _Action)
        {
            m_Success = _Action;
            return this;
        }

        public virtual IPacket OnFail(Action _Action)
        {
            m_Fail = _Action;
            return this;
        }

        public virtual IPacket OnCancel(Action _Action)
        {
            m_Cancel = _Action;
            return this;
        }
        
        public virtual void DeserializeResponse(string _Json)
        {
            ResponseRaw = _Json;
            if (!Utility.IsInRange(ResponseCode, 200, 299))
                m_ErrorMessage = JsonConvert.DeserializeObject<ErrorResponseArgs>(_Json);
            
            if (Utility.IsInRange(ResponseCode, 200, 299))
                InvokeSuccess();
            else if (Utility.IsInRange(ResponseCode, 300, 399))
                InvokeCancel();
            else if (Utility.IsInRange(ResponseCode, 400, 599))
                InvokeFail();
        }
        
        #endregion
    }
}