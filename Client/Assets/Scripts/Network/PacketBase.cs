using System;
using Newtonsoft.Json;
using Utils;

namespace Network
{
    public abstract class PacketBase : IPacket
    {
        #region public properties
        
        public abstract string Id { get; }
        public abstract string Url { get; }
        public virtual string Method => "POST";
        public virtual bool OnlyOne => false;
        public virtual bool Resend => false;
        public bool IsDone { get; set; }
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
        
        #region nonpublic members

        private ErrorResponseArgs m_ErrorMessage;
        private Action m_Success;
        private Action m_Fail;

        #endregion
        
        #region constructor

        protected PacketBase(object _Request)
        {
            Request = _Request;
        }

        #endregion
        
        #region nonpublic methods
        
        public void InvokeSuccess()
        {
            try
            {
                m_Success?.Invoke();
            }
            catch (Exception e)
            {
                Dbg.LogError($"OnSuccess error: {e.Message};\n StackTrace: {e.StackTrace}");
            }
        }

        public void InvokeFail()
        {
            try
            {
                m_Fail?.Invoke();
            }
            catch (Exception e)
            {
                Dbg.LogError($"OnFail error: {e.Message};\n StackTrace: {e.StackTrace}");
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

        public virtual void DeserializeResponse(string _Json)
        {
            ResponseRaw = _Json;
            try
            {
                if (!NetworkUtils.IsPacketSuccess(ResponseCode))
                    m_ErrorMessage = JsonConvert.DeserializeObject<ErrorResponseArgs>(_Json);
            }
            catch (JsonReaderException)
            {
                Dbg.LogError(ResponseRaw);
                throw;
            }
            
            if (NetworkUtils.IsPacketSuccess(ResponseCode))
                InvokeSuccess();
            else
                InvokeFail();

            IsDone = true;
        }
        
        #endregion
    }
}