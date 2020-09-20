using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Network;
using Network.PacketArgs;
using Network.Packets;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Tests.Editor
{
    public class ServerConnectionTests
    {
        [Test]
        public void SimpleConnectionTest()
        {
            //Arrange
            GameClient.Instance.Start();
            GameClient.Instance.SetTestMode();
            string url = $"{GameClient.Instance.BaseUrl}/timetest";

            var request = new UnityWebRequest(url, "GET");
            request.downloadHandler = new DownloadHandlerBuffer();

            //wait 5 seconds before cancel
            var stopWaiting = new Utils.Bool();
            Task.Run(Utils.CommonUtils.WaitForSecs(5f, stopWaiting));

            //Act
            request.SendWebRequest();
            while (!request.isDone && !stopWaiting)
            {
                //do nothing and wait
            }
            
            //Assert
            Assert.IsTrue(Utility.IsInRange(request.responseCode, 200, 299));
            
            Debug.Log($"Response code: {request.responseCode}");
            Debug.Log(request.downloadHandler.text);
        }

        [UnityTest]
        public IEnumerator LoginByTestGuestUser()
        {
            //Arrange
            GameClient.Instance.Start();
            GameClient.Instance.SetTestMode();
            string url = $"{GameClient.Instance.BaseUrl}/api/accounts/login";
            bool requestSuccess = false;
            
            //Act
            IPacket packet = new LoginUserPacket(
                    new LoginUserPacketRequestArgs
                    {
                        Name = "",
                        PasswordHash = "",
                        DeviceId = "000000"
                    })
                .OnFail(() => requestSuccess = false)
                .OnSuccess(() => requestSuccess = true) as LoginUserPacket;
            GameClient.Instance.Send(packet);
            
            //Assert
            Debug.Log($"response code: {packet.ResponseCode}");
            Debug.Log($"response string: {packet.ResponseRaw}");
            
            Assert.IsTrue(requestSuccess);
            yield break;
        }
    }
}