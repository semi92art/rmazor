using System.Threading.Tasks;
using Network;
using Network.PacketArgs;
using Network.Packets;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using Utility = Utils.Utility;

namespace Tests.Editor
{
    public class ServerConnectionTests
    {
        [Test]
        public void SimpleConnection()
        {
            //Arrange
            GameClient.Instance.Init(true);
            string url = $"{GameClient.Instance.BaseUrl}/timetest";

            var request = new UnityWebRequest(url, "GET");
            request.downloadHandler = new DownloadHandlerBuffer();

            //wait 5 seconds before cancel
            bool stopWaiting = false;
            Task.Run(Utility.WaitForSecs(5f, () => stopWaiting = true));

            //Act
            request.SendWebRequest();
            while (!request.isDone && !stopWaiting) { } //do nothing and wait
            
            //Assert
            Assert.IsTrue(Utility.IsInRange(request.responseCode, 200, 299));
            
            Debug.Log($"Response code: {request.responseCode}");
            Debug.Log(request.downloadHandler.text);
        }

        [Test]
        public void LoginByTestGuestUser()
        {
            //Arrange
            GameClient.Instance.Init(true);
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
        }

        [Test]
        public void RegisterByTestGuestUser()
        {
            //Arrange
            GameClient.Instance.Init(true);
            bool requestSuccess = false;
            
            //Act
            IPacket packet = new RegisterUserPacket(
                new RegisterUserUserPacketRequestArgs
                {
                    Name = "",
                    PasswordHash = "",
                    DeviceId = $"test_{Utility.GetUniqueID()}"
                })
                .OnSuccess(() => requestSuccess = true);
            GameClient.Instance.Send(packet);
            
            //Assert
            Debug.Log($"response code: {packet.ResponseCode}");
            Debug.Log($"response string: {packet.ResponseRaw}");
            
            Assert.IsTrue(requestSuccess);
        }

        [Test]
        public void GetScore()
        {
            //Arrange
            GameClient.Instance.Init(true);
            bool requestSuccess = false;
            
            //Act
            IPacket packet = new GetScorePacket(
                    new GetScoreRequestArgs
                    {
                        AccountId = 1,
                        GameId = 1,
                        Type = 1
                    })
                .OnSuccess(() => requestSuccess = true);
            GameClient.Instance.Send(packet);
            
            //Assert
            Debug.Log($"response code: {packet.ResponseCode}");
            Debug.Log($"response string: {packet.ResponseRaw}");
            
            Assert.IsTrue(requestSuccess);
        }

        [Test]
        public void GetScores()
        {
            //Arrange
            GameClient.Instance.Init(true);
            bool requestSuccess = false;
            
            //Act
            IPacket packet = new GetScoresPacket(
                    new AccIdGameId
                    {
                        AccountId = 1,
                        GameId = 1,
                    })
                .OnSuccess(() => requestSuccess = true);
            GameClient.Instance.Send(packet);
            
            //Assert
            Debug.Log($"response code: {packet.ResponseCode}");
            Debug.Log($"response string: {packet.ResponseRaw}");
            
            Assert.IsTrue(requestSuccess);
        }
        
        [Test]
        public void SetScore()
        {
            //Arrange
            GameClient.Instance.Init(true);
            bool requestSuccess = false;
            Mathf.RoundToInt(Random.value * 100);
            
            //Act
            IPacket packet = new SetScorePacket(
                    new SetScoreRequestArgs
                    {
                        AccountId = 1,
                        GameId = 1,
                        Points = Mathf.RoundToInt(Random.value * 100),
                        Type = 1
                    })
                .OnSuccess(() => requestSuccess = true);
            GameClient.Instance.Send(packet);
            
            //Assert
            Debug.Log($"response code: {packet.ResponseCode}");
            Debug.Log($"response string: {packet.ResponseRaw}");
            
            Assert.IsTrue(requestSuccess);
        }

        [Test]
        public void GetProfile()
        {
            //Arrange
            GameClient.Instance.Init(true);
            bool requestSuccess = false;
            
            //Act
            IPacket packet = new GetProfilePacket(
                    new AccIdGameId
                    {
                        AccountId = 1,
                        GameId = 1
                    })
                .OnSuccess(() => requestSuccess = true);
            GameClient.Instance.Send(packet);
            
            //Assert
            Debug.Log($"response code: {packet.ResponseCode}");
            Debug.Log($"response string: {packet.ResponseRaw}");
            
            Assert.IsTrue(requestSuccess);
        }

        [Test]
        public void SetProfile()
        {
            //Arrange
            GameClient.Instance.Init(true);
            bool requestSuccess = false;
            
            //Act
            var options = ProfileOption.CreateList();
            options[3] = new ProfileOption {Available = true, Option = 4};

            IPacket packet = new SetProfilePacket(
                    new SetProfileRequestArgs
                    {
                        AccountId = 1,
                        Options = options
                    })
                .OnSuccess(() => requestSuccess = true);
            GameClient.Instance.Send(packet);

            //Assert
            Debug.Log($"response code: {packet.ResponseCode}");
            Debug.Log($"response string: {packet.ResponseRaw}");
            
            Assert.IsTrue(requestSuccess);
        }

        [Test]
        public void GetFullAccountData()
        {
            //Arrange
            GameClient.Instance.Init(true);
            bool requestSuccess = false;
            
            //Act
            IPacket packet = new GetFullAccountDataPacket(
                    new AccIdGameId
                    {
                        AccountId = 1,
                        GameId = 1
                    })
                .OnSuccess(() => requestSuccess = true);
            GameClient.Instance.Send(packet);
            
            //Assert
            Debug.Log($"response code: {packet.ResponseCode}");
            Debug.Log($"response string: {packet.ResponseRaw}");
            
            Assert.IsTrue(requestSuccess);
        }
    }
}