using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UICreationSystem;
using Utils;

namespace Network
{
    public class GameClient : MonoBehaviour, ISingleton
    {
        #region singleton
        
        private static GameClient _instance;

        public static GameClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameClient");
                    _instance = go.AddComponent<GameClient>();
                }
                    
                return _instance;
            }
        }
        
        #endregion
        
        #region private fields
        
        private readonly Dictionary<int, IPacket> m_Packets = new Dictionary<int, IPacket>();
        private string m_ServerName;
        private Dictionary<string, string> m_ServerBaseUrls;
        
        
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
        public string DeviceId => SystemInfo.deviceUniqueIdentifier;
        

        #endregion
        
        #region public methods
        
        public void Init(bool _TestMode = false)
        {
#if AZURE
            m_ServerName = "Azure";
#else
            m_ServerName = "Ubuntu1";
#endif
            
            if (_TestMode)
                m_ServerName = "TestRunner";
            
            m_ServerBaseUrls = new Dictionary<string, string>
            {
                {"Ubuntu1", @"http://77.37.152.15:7000"},
                {"Azure", @"https://clickersapi.azurewebsites.net"},
                {"Test", @"http://77.37.152.15:7100"},
                {"TestRunner", @"http://77.37.152.15:7000"}
            };
            
            if (!IsTestRunningMode)
                DontDestroyOnLoad(gameObject);
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
        
        public static T Deserialize<T>(string _Json)
        {
            return JsonConvert.DeserializeObject<T>(_Json);
        }

        public bool IsTestRunningMode => m_ServerName == "TestRunner";
        
        #endregion
        
        #region private methods
        
        private void SendCore(IPacket _Packet)
        {
            StartCoroutine(Coroutines.Action(() => SendRequest(_Packet)));
        }

        private void SendRequest(IPacket _Packet)
        {
            UnityWebRequest request = new UnityWebRequest(_Packet.Url, _Packet.Method);
            request.method = _Packet.Method;
            string json = JsonConvert.SerializeObject(_Packet.Request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw) {contentType = "application/json"};
            request.downloadHandler = new DownloadHandlerBuffer();

            //wait 5 seconds before cancel
            bool stopWaiting = false;
            Task.Run(Utils.Utility.WaitForSecs(5f, () => stopWaiting = true));

            request.SendWebRequest();
            while (!request.isDone && !stopWaiting) {  } //do nothing and wait
            
            
            _Packet.ResponseCode = request.responseCode;
            _Packet.DeserializeResponse(request.downloadHandler.text);
        }
        
        #endregion
    }
}