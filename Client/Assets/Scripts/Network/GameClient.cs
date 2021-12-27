using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Entities;
using GameHelpers;
using Network.Packets;
using Newtonsoft.Json;
using Ticker;
using UnityEngine.Events;
using UnityEngine.Networking;
using Utils;

namespace Network
{
    public interface IGameClient : IInit
    {
        List<GameDataField> ExecutingGameFields { get; }
        void                Send(IPacket _Packet, bool _Async = true);
    }
    
    public class GameClient : IGameClient
    {
        #region nonpublic members

        private bool m_FirstRequest = true;
        private bool m_DatabaseConnectionTestStarted;

        private bool DatabaseConnection
        {
            get => SaveUtils.GetValue(SaveKeys.LastDbConnectionSuccess);
            set => SaveUtils.PutValue(SaveKeys.LastDbConnectionSuccess, value);
        }
        
        #endregion
        
        #region api

        public bool                Initialized { get; private set; }
        public event UnityAction   Initialize;
        public List<GameDataField> ExecutingGameFields { get; } = new List<GameDataField>();
        
        public void Init()
        {
            if (GameClientUtils.GameId == 0)
                GameClientUtils.GameId = GameClientUtils.DefaultGameId;
            Initialize?.Invoke();
            Initialized = true;
            // if (!CommonData.Testing)
            //     return;
            // TestDatabaseConnection();
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
            
            Coroutines.Run(Coroutines.Delay(waitingTime, () => stopWaiting = true));
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
            CommonTicker.Instance,
            () => false));
        }

        #endregion
    }
}