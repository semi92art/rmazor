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
        private string m_ServerName;
        private Dictionary<string, string> m_ServerBaseUrls;
        private bool m_FirstRequest = true;
        private bool m_ConnectionTestStarted;
        
        private bool LastConnectionSucceeded
        {
            get => SaveUtils.GetValue<bool>(SaveKey.LastConnectionSucceeded);
            set => SaveUtils.PutValue(SaveKey.LastConnectionSucceeded, value);
        }
        
        #endregion
        
        #region api

        public string ServerApiUrl => m_ServerBaseUrls[m_ServerName];

        public int AccountId
        {
            get => SaveUtils.GetValue<int>(SaveKey.AccountId);
            set => SaveUtils.PutValue(SaveKey.AccountId, value);
        }

        public string Login
        {
            get => SaveUtils.GetValue<string>(SaveKey.Login);
            set => SaveUtils.PutValue(SaveKey.Login, value);
        }

        public string PasswordHash
        {
            get => SaveUtils.GetValue<string>(SaveKey.PasswordHash);
            set => SaveUtils.PutValue(SaveKey.PasswordHash, value);
        }

        public int GameId
        {
            get => SaveUtils.GetValue<int>(SaveKey.GameId);
            set => SaveUtils.PutValue(SaveKey.GameId, value);
        }

        public int DefaultGameId
        {
            get
            {
#if GAME_1
                return 1;
#elif GAME_2
                return 2;
#elif GAME_3
                return 3;
#elif GAME_4
                return 4;
#elif GAME_5
                return 5;
#endif
                return 1;
            }
        }

        public string DeviceId => $"test_{SystemInfo.deviceUniqueIdentifier}";
        

        public void Init(bool _TestMode = false)
        {
#if UNITY_EDITOR
            m_ServerName = "Debug";
#else
            m_ServerName = "Ubuntu1";
#endif

            if (GameId == 0)
                GameId = DefaultGameId;
            m_ServerBaseUrls = new Dictionary<string, string>
            {
                {"Ubuntu1", @"http://77.37.152.15:7000"},
#if UNITY_EDITOR
                {"Debug", SaveUtils.GetValue<string>(SaveKeyDebug.ServerUrl)},
                {"TestRunner", SaveUtils.GetValue<string>(SaveKeyDebug.ServerUrl)}
#endif
            };
            
            if (_TestMode)
                m_ServerName = "TestRunner";
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