using System;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    public class Packet
    {
        public PacketData Request { get; }
        public PacketData Response { get; set; }
        public int Id { get; }
        public string Url { get; set; }
        public string Method { get; set; }
        public bool OnlyOne { get; set; }
        public bool Resend { get; set; }
        
        protected Action Success;
        protected Action Fail;
        protected Action Cancel;

        #region constructors
        
        protected Packet(int _Id, PacketData _Request)
        {
            Id = _Id;
            Request = _Request;
        }
        
        #endregion
        
        #region public methods

        public Packet OnSuccess(Action _Action)
        {
            Success += _Action;
            return this;
        }

        public Packet OnFail(Action _Action)
        {
            Fail += _Action;
            return this;
        }

        public Packet OnCancel(Action _Action)
        {
            Cancel = _Action;
            return this;
        }

        public void InvokeSuccess()
        {
            try
            {
                Success?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnSuccess error: {e.Message}");
            }
        }

        public void InvokeFail()
        {
            try
            {
                Fail?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnFail error: {e.Message}");
            }
        }

        public void InvokeCancel()
        {
            try
            {
                Cancel?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"OnCancel error: {e.Message}");
            }
        }
        
        #endregion
    }
}