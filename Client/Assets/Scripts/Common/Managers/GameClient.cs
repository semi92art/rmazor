using System.Diagnostics;
using System.Text;
using Common.Helpers;
using Common.Network;
using Common.Network.Packets;
using Common.Ticker;
using Common.Utils;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Common.Managers
{
    public class GameClient : InitBase, IGameClient
    {
        #region nonpublic members

        private bool m_FirstRequest = true;
        private bool m_DatabaseConnectionTestStarted;

        private bool DatabaseConnection
        {
            get => SaveUtils.GetValue(SaveKeysCommon.LastDbConnectionSuccess);
            set => SaveUtils.PutValue(SaveKeysCommon.LastDbConnectionSuccess, value);
        }
        
        #endregion

        #region inject

        private GlobalGameSettings GameSettings { get; }
        private ICommonTicker      Ticker   { get; }

        public GameClient(GlobalGameSettings _GameSettings, ICommonTicker _Ticker)
        {
            GameSettings = _GameSettings;
            Ticker = _Ticker;
        }

        #endregion
        
        #region api
        
        public override void Init()
        {
            if (CommonData.GameId == 0)
                CommonData.GameId = GameClientUtils.GetDefaultGameId();
            base.Init();
        }
        
        public void Send(IPacket _Packet, bool _Async = true)
        {
            SendCore(_Packet, _Async);
        }

        #endregion
        
        #region nonpublic methods
        
        private void SendCore(IPacket _Packet, bool _Async)
        {
            if (_Async)
                Cor.Run(Cor.Action(() => SendRequestToDatabase(_Packet)));
            else
                SendRequestToDatabase(_Packet);
        }

        private void SendRequestToDatabase(IPacket _Packet, float? _WaitingTime = null)
        {
            var request = new UnityWebRequest(_Packet.Url, _Packet.Method) {method = _Packet.Method};
            string json = JsonConvert.SerializeObject(_Packet.Request);
            var bodyRaw = Encoding.UTF8.GetBytes(json);
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
            Cor.Run(Cor.Delay(waitingTime, Ticker, () => stopWaiting = true));
            Cor.Run(Cor.WaitWhile(
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
            Cor.Run(Cor.Repeat(() =>
            {
                sw.Restart();
                var testPacket = new TestConnectionPacket()
                    .OnSuccess(() => DatabaseConnection = true)
                    .OnFail(() =>
                        {
                            Dbg.LogWarning("No connection to database," +
                                         $" request time: {sw.Elapsed.TotalMilliseconds / 1000D:F2} secs");
                            DatabaseConnection = false;
                        }
                    );
                Cor.Run(Cor.Action(() => SendRequestToDatabase(testPacket, 2f)));
            }, 5f,
            float.MaxValue,
            Ticker,
            () => false));
        }

        #endregion
    }
}