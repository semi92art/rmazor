using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Entities;
using GameHelpers;
using Network.Packets;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using Utils;
using Debug = UnityEngine.Debug;

namespace Network
{
    public class GameClient : ISingleton
    {
        #region singleton
        
        private static GameClient _instance;

        public static GameClient Instance
        {
            get
            {
                if (_instance != null) 
                    return _instance;
                
                _instance = new GameClient(); 
                _instance.Init();
                return _instance;
            }
        }
        
        #endregion
        
        #region nonpublic members
        
        private readonly Dictionary<string, IPacket> m_Packets = new Dictionary<string, IPacket>();
        
        private bool m_FirstRequest = true;
        private bool m_ConnectionTestStarted;
        
        private bool LastConnectionSucceeded
        {
            get => SaveUtils.GetValue<bool>(SaveKey.LastConnectionSucceeded);
            set => SaveUtils.PutValue(SaveKey.LastConnectionSucceeded, value);
        }
        
        #endregion
        
        #region api

       

        public void Init(bool _TestMode = false)
        {


            if (GameClientUtils.GameId == 0)
                GameClientUtils.GameId = GameClientUtils.DefaultGameId;

            if (_TestMode)
                CommonUtils.UnitTesting = true;
            if (!_TestMode)
                StartTestingConnection();
            GameSettings.PlayMode = !_TestMode;
        }

        public void Send(IPacket _Packet, bool _Async = true)
        {
            if (m_Packets.ContainsKey(_Packet.Id))
            {
                if (m_Packets[_Packet.Id].OnlyOne)
                    _Packet.ResponseCode = 300;
                else
                {
                    m_Packets[_Packet.Id] = _Packet;
                    SendCore(_Packet, _Async);
                }
            }
            else
            {
                m_Packets.Add(_Packet.Id, _Packet);
                SendCore(_Packet, _Async);
            }
        }
        
        public T Deserialize<T>(string _Json)
        {
            return JsonConvert.DeserializeObject<T>(_Json);
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void SendCore(IPacket _Packet, bool _Async)
        {
            if (_Async)
                Coroutines.Run(Coroutines.Action(() => SendRequest(_Packet)));
            else
                SendRequest(_Packet);
        }

        private void SendRequest(IPacket _Packet, float? _WaitingTime = null)
        {
            UnityWebRequest request = new UnityWebRequest(_Packet.Url, _Packet.Method);
            request.method = _Packet.Method;
            string json = JsonConvert.SerializeObject(_Packet.Request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw) {contentType = "application/json"};
            request.downloadHandler = new DownloadHandlerBuffer();
            
            bool stopWaiting = false;
            request.SendWebRequest();

            float waitingTime = 2f;
            if (!LastConnectionSucceeded)
                waitingTime = 0f;
            if (m_FirstRequest)
                waitingTime = 5f;
            if (_WaitingTime.HasValue)
                waitingTime = _WaitingTime.Value;
            
            Coroutines.Run(Coroutines.Delay(
                () => stopWaiting = true, waitingTime));
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => !request.isDone && !stopWaiting,
                () =>
            {
                _Packet.ResponseCode = request.responseCode;
                _Packet.DeserializeResponse(request.downloadHandler.text);
                m_FirstRequest = false;
            }));
        }

        private void StartTestingConnection()
        {
            var sw = new Stopwatch();
            if (m_ConnectionTestStarted)
                return;
            Coroutines.Run(Coroutines.Repeat(() =>
            {
                sw.Restart();
                IPacket testPacket = new TestConnectionPacket()
                    .OnSuccess(() => LastConnectionSucceeded = true)
                    .OnFail(() =>
                        {
                            Debug.LogError($"No connection to server, request time: {(sw.Elapsed.TotalMilliseconds / 1000D):F2} secs");
                            LastConnectionSucceeded = false;
                        }
                    );
                Coroutines.Run(Coroutines.Action(() => SendRequest(testPacket, 2f)));
            }, 5f,
                float.MaxValue,
                UiTimeProvider.Instance,
                () => false));
            m_ConnectionTestStarted = true;
        }

        #endregion
    }
}