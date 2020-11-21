using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Network.Packets;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Utils;

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
        
        #region private members
        
        private readonly Dictionary<int, IPacket> m_Packets = new Dictionary<int, IPacket>();
        private string m_ServerName;
        private Dictionary<string, string> m_ServerBaseUrls;
        private bool m_FirstRequest = true;
        private bool m_ConnectionTestStarted;
        
        
        #endregion
        
        #region public properties

        public string BaseUrl => m_ServerBaseUrls[m_ServerName];

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

        public string CountryKey
        {
            get => SaveUtils.GetValue<string>(SaveKey.CountryKey);
            set => SaveUtils.PutValue(SaveKey.CountryKey, value);
        }

        public int GameId
        {
            get => SaveUtils.GetValue<int>(SaveKey.GameId);
            set => SaveUtils.PutValue(SaveKey.GameId, value);
        }
        
        public string DeviceId => $"test_{SystemInfo.deviceUniqueIdentifier}";

        public bool LastConnectionSucceeded
        {
            get => SaveUtils.GetValue<bool>(SaveKey.LastConnectionSucceeded);
            private set => SaveUtils.PutValue(SaveKey.LastConnectionSucceeded, value);
        }
        

        #endregion
        
        #region api
        
        public void Init(bool _TestMode = false)
        {
#if AZURE
            m_ServerName = "Azure";
#elif RELEASE
            m_ServerName = "Ubuntu1";
#elif DEBUG
            m_ServerName = "Debug";
#endif
            
            m_ServerBaseUrls = new Dictionary<string, string>
            {
                {"Ubuntu1", @"http://77.37.152.15:7000"},
                {"Azure", @"https://clickersapi.azurewebsites.net"},
                {"Debug", SaveUtils.GetValue<string>(SaveKey.DebugServerUrl)},
                {"TestRunner", SaveUtils.GetValue<string>(SaveKey.DebugServerUrl)}
            };
            
            if (_TestMode)
                m_ServerName = "TestRunner";
            if (!_TestMode)
                StartTestingConnection();
        }

        public void Send(IPacket _Packet)
        {
            if (m_Packets.ContainsKey(_Packet.Id))
            {
                if (m_Packets[_Packet.Id].OnlyOne)
                    _Packet.ResponseCode = 300;
                else
                {
                    m_Packets[_Packet.Id] = _Packet;
                    SendCore(_Packet);
                }
            }
            else
            {
                m_Packets.Add(_Packet.Id, _Packet);
                SendCore(_Packet);
            }
        }
        
        public T Deserialize<T>(string _Json)
        {
            return JsonConvert.DeserializeObject<T>(_Json);
        }

        #endregion
        
        #region private methods
        
        private void SendCore(IPacket _Packet)
        {
            Coroutines.Run(Coroutines.Action(() => SendRequest(_Packet)));
        }

        private void SendRequest(IPacket _Packet)
        {
            UnityWebRequest request = new UnityWebRequest(_Packet.Url, _Packet.Method);
            request.method = _Packet.Method;
            string json = JsonConvert.SerializeObject(_Packet.Request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw) {contentType = "application/json"};
            request.downloadHandler = new DownloadHandlerBuffer();
            
            bool stopWaiting = false;
            request.SendWebRequest();
            Coroutines.Run(Coroutines.Delay(() => stopWaiting = true, m_FirstRequest ? 5f : 2f));
            Coroutines.Run(Coroutines.WaitWhile(() =>
            {
                _Packet.ResponseCode = request.responseCode;
                _Packet.DeserializeResponse(request.downloadHandler.text);
                m_FirstRequest = false;
            }, () => !request.isDone && !stopWaiting));
        }

        private void StartTestingConnection()
        {
            if (m_ConnectionTestStarted)
                return;
            Coroutines.Run(Coroutines.Repeat(() =>
            {
                IPacket testPacket = new TestConnectionPacket()
                    .OnSuccess(() => LastConnectionSucceeded = true)
                    .OnFail(() =>
                        {
                            Debug.LogError("No connection to server");
                            LastConnectionSucceeded = false;
                        }
                    );
                Send(testPacket);
            }, 5f,
                float.MaxValue,
                () => false));
            m_ConnectionTestStarted = true;
        }

        #endregion
    }
}