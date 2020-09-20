using System;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    public abstract class PacketBase : IPacket
    {
        public abstract int Id { get; }
        public abstract object Request { get; }
        public abstract string Url { get; }
        public abstract string Method { get; }
        public virtual bool OnlyOne => false;
        public virtual bool Resend => false;
        public bool IsDone { get; private set; }
        public string ResponseRaw { get; private set; }
        public long ResponseCode { get; set; }

        private Action m_Success;
        private Action m_Fail;
        private Action m_Cancel;

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

        public virtual void InvokeSuccess()
        {
            try
            {
                IsDone = true;
                m_Success?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnSuccess error: {e.Message}");
            }
        }

        public virtual void InvokeFail()
        {
            try
            {
                IsDone = true;
                m_Fail?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnFail error: {e.Message}");
            }
        }

        public virtual void InvokeCancel()
        {
            try
            {
                IsDone = true;
                m_Cancel?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnCancel error: {e.Message}");
            }
        }

        public virtual void DeserializeResponse(string _Json)
        {
            ResponseRaw = _Json;
            //here must be set Response
        }
        
        #endregion
    }
}