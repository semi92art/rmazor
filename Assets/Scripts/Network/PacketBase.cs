using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

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

        public long ResponseCode
        {
            get => m_ResponseCode;
            set
            {
                m_ResponseCode = value;
                if (Utils.Utility.IsInRange(value, 200, 299))
                    InvokeSuccess();
                else if (Utils.Utility.IsInRange(value, 300, 399))
                    InvokeCancel();
                else if (Utils.Utility.IsInRange(value, 400, 499))
                    InvokeFail();
            }
        }
        
        #endregion
        
        #region private fields
        
        private long m_ResponseCode;
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

        public virtual PacketBase OnSuccess(Action _Action)
        {
            m_Success += _Action;
            return this;
        }

        public virtual PacketBase OnFail(Action _Action)
        {
            m_Fail += _Action;
            return this;
        }

        public virtual PacketBase OnCancel(Action _Action)
        {
            m_Cancel = _Action;
            return this;
        }
        
        public virtual void DeserializeResponse(string _Json)
        {
            ResponseRaw = _Json;
            //here must be set Response
        }
        
        #endregion
    }
}