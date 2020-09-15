using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class GameClient : MonoBehaviour
    {
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

        private Dictionary<int, Packet> m_Packets = new Dictionary<int, Packet>();

        public void Send(Packet _Packet)
        {
            if (m_Packets.ContainsKey(_Packet.Id))
            {
                if (m_Packets[_Packet.Id].OnlyOne)
                    _Packet.InvokeCancel();
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

        private void SendCore(Packet _Packet)
        {
            StartCoroutine(SendRequest(_Packet));
        }

        private IEnumerator<UnityWebRequestAsyncOperation> SendRequest(Packet _Packet)
        {
            UnityWebRequest request = new UnityWebRequest(_Packet.Url, _Packet.Method);
            string json = JsonUtility.ToJson(_Packet);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (Utility.IsInRange(request.responseCode, 100, 299))
                _Packet.InvokeSuccess();
            else if (Utility.IsInRange(request.responseCode, 400, 599))
                _Packet.InvokeFail();
            
            yield return request.SendWebRequest();
        }
    }
}