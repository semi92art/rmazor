using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Entities;
using GameHelpers;
using Network.Packets;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TimeProviders;
using Utils;

namespace Network
{
    public class GameClient : IInit
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

        private bool m_FirstRequest = true;
        private bool m_DatabaseConnectionTestStarted;
        private bool m_InternetConnectionTestStarted;

        private bool DatabaseConnection
        {
            get => SaveUtils.GetValue<bool>(SaveKey.LastDatabaseConnectionSucceeded);
            set => SaveUtils.PutValue(SaveKey.LastDatabaseConnectionSucceeded, value);
        }
        
        #endregion
        
        #region api

        public event NoArgsHandler Initialized;
        public readonly List<GameDataField> ExecutingGameFields = new List<GameDataField>();
        
        public void Init()
        {
            if (GameClientUtils.GameId == 0)
                GameClientUtils.GameId = GameClientUtils.DefaultGameId;
            if (!CommonData.Testing)
            {
                Initialized?.Invoke();
                return;
            }
            TestDatabaseConnection();
            TestInternetConnection();
            Initialized?.Invoke();
        }
        
        public void Send(IPacket _Packet, bool _Async = true)
        {
            SendCore(_Packet, _Async);
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
                Coroutines.Run(Coroutines.Action(() => SendRequestToDatabase(_Packet)));
            else
                SendRequestToDatabase(_Packet);
        }

        private void SendRequestToDatabase(IPacket _Packet, float? _WaitingTime = null)
        {
            var request = new UnityWebRequest(_Packet.Url, _Packet.Method);
            request.method = _Packet.Method;
            string json = JsonConvert.SerializeObject(_Packet.Request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw) {contentType = "application/json"};
            request.downloadHandler = new DownloadHandlerBuffer();
            
            bool stopWaiting = false;
            request.SendWebRequest();

            float waitingTime = 2f;
            if (!DatabaseConnection)
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

        private void TestDatabaseConnection()
        {
            var sw = new Stopwatch();
            if (m_DatabaseConnectionTestStarted)
                return;
            m_DatabaseConnectionTestStarted = true;
            Coroutines.Run(Coroutines.Repeat(() =>
            {
                sw.Restart();
                IPacket testPacket = new TestConnectionPacket()
                    .OnSuccess(() => DatabaseConnection = true)
                    .OnFail(() =>
                        {
                            Dbg.LogWarning("No connection to database," +
                                         $" request time: {sw.Elapsed.TotalMilliseconds / 1000D:F2} secs");
                            DatabaseConnection = false;
                        }
                    );
                Coroutines.Run(Coroutines.Action(() => SendRequestToDatabase(testPacket, 2f)));
            }, 5f,
            float.MaxValue,
            UiTimeProvider.Instance,
            () => false));
        }

        private void TestInternetConnection()
        {
            if (m_InternetConnectionTestStarted)
                return;
            m_InternetConnectionTestStarted = true;
            Coroutines.Run(Coroutines.Repeat(
            () =>
            {
                var r = new UnityWebRequest("http://google.com", "GET");
                Coroutines.Run(Coroutines.WaitWhile(() =>
                !r.isDone && !r.isHttpError && !r.isNetworkError,
                () => GameClientUtils.InternetConnection = r.isDone && !r.isHttpError && !r.isNetworkError));
                r.SendWebRequest();
            },
            5f,
            float.MaxValue,
            UiTimeProvider.Instance,
            () => false));
        }

        #endregion
    }
}