using System.Collections;
using System.Threading.Tasks;
using Constants;
using GameHelpers;
using Network;
using Network.PacketArgs;
using Network.Packets;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using Utils;
using Assert = UnityEngine.Assertions.Assert;

public class ServerConnectionTests
{
    private void InitGameObjects()
    {
        var coroutinesRunnerObj = GameObject.Find("CoroutinesRunner");
        if (coroutinesRunnerObj != null) 
            return;
        coroutinesRunnerObj = new GameObject("CoroutinesRunner");
        coroutinesRunnerObj.AddComponent<DontDestroyOnLoad>();
    }
    
    [UnityTest]
    public IEnumerator SimpleConnection()
    {
        //Arrange
        InitGameObjects();
        GameClient.Instance.Init(true);
        string url = $"{GameClient.Instance.BaseUrl}/timetest";

        var request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        //wait 5 seconds before cancel
        bool stopWaiting = false;
        Task.Run(CommonUtils.WaitForSecs(5f, () => stopWaiting = true));

        //Act
        request.SendWebRequest();
        while (!request.isDone && !stopWaiting)
            yield return new WaitForEndOfFrame();
            
        //Assert
        Assert.IsTrue(CommonUtils.IsInRange(request.responseCode, 200, 299));
            
        Debug.Log($"Response code: {request.responseCode}");
        Debug.Log(request.downloadHandler.text);
    }
    
    [UnityTest]
    public IEnumerator LoginByTestGuestUser()
    {
        //Arrange
        InitGameObjects();
        GameClient.Instance.Init(true);
        bool requestSuccess = false;
        
        //Act
        IPacket packet = new LoginUserPacket(
            new LoginUserPacketRequestArgs
            {
                Name = "",
                PasswordHash = "",
                DeviceId = "test_user_do_not_delete"
            })
            .OnFail(() => requestSuccess = false)
            .OnSuccess(() => requestSuccess = true) as LoginUserPacket;
        GameClient.Instance.Send(packet);
        while (!packet.IsDone) 
            yield return new WaitForEndOfFrame();
        
        //Assert
        Debug.Log($"response code: {packet.ResponseCode}");
        Debug.Log($"response string: {packet.ResponseRaw}");
        
        Assert.IsTrue(requestSuccess);
    }

    [UnityTest]
    public IEnumerator RegisterByTestGuestUser()
    {
        //Arrange
        InitGameObjects();
        GameClient.Instance.Init(true);
        bool requestSuccess = false;
        
        //Act
        IPacket packet = new RegisterUserPacket(
            new RegisterUserPacketRequestArgs
            {
                Name = "",
                PasswordHash = "",
                DeviceId = $"test_{CommonUtils.GetUniqueId()}",
                GameId = 1
            })
            .OnSuccess(() => requestSuccess = true);
        GameClient.Instance.Send(packet);
        while (!packet.IsDone) 
            yield return new WaitForEndOfFrame();
        
        //Assert
        Debug.Log($"response code: {packet.ResponseCode}");
        Debug.Log($"response string: {packet.ResponseRaw}");
        
        Assert.IsTrue(requestSuccess);
    }

    [UnityTest]
    public IEnumerator GetScore()
    {
        //Arrange
        InitGameObjects();
        GameClient.Instance.Init(true);
        bool requestSuccess = false;
        
        //Act
        IPacket packet = new GetScorePacket(
                new GetScoreRequestArgs
                {
                    AccountId = 1,
                    GameId = 1,
                    Type = ScoreTypes.MaxScore
                })
            .OnSuccess(() => requestSuccess = true);
        GameClient.Instance.Send(packet);
        while (!packet.IsDone) 
            yield return new WaitForEndOfFrame();
        
        //Assert
        Debug.Log($"response code: {packet.ResponseCode}");
        Debug.Log($"response string: {packet.ResponseRaw}");
        
        Assert.IsTrue(requestSuccess);
    }

    [UnityTest]
    public IEnumerator GetScores()
    {
        //Arrange
        InitGameObjects();
        GameClient.Instance.Init(true);
        bool requestSuccess = false;
        
        //Act
        IPacket packet = new GetScoresPacket(
                new AccIdGameId
                {
                    AccountId = 1,
                    GameId = 1
                })
            .OnSuccess(() => requestSuccess = true);
        GameClient.Instance.Send(packet);
        while (!packet.IsDone) 
            yield return new WaitForEndOfFrame();
        
        //Assert
        Debug.Log($"response code: {packet.ResponseCode}");
        Debug.Log($"response string: {packet.ResponseRaw}");
        
        Assert.IsTrue(requestSuccess);
    }
        
    [UnityTest]
    public IEnumerator SetScore()
    {
        //Arrange
        InitGameObjects();
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
                    Type = ScoreTypes.MaxScore
                })
            .OnSuccess(() => requestSuccess = true);
        GameClient.Instance.Send(packet);
        while (!packet.IsDone) 
            yield return new WaitForEndOfFrame();
        
        //Assert
        Debug.Log($"response code: {packet.ResponseCode}");
        Debug.Log($"response string: {packet.ResponseRaw}");
        
        Assert.IsTrue(requestSuccess);
    }

    [UnityTest]
    public IEnumerator GetProfile()
    {
        //Arrange
        InitGameObjects();
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
        while (!packet.IsDone)
            yield return new WaitForEndOfFrame();
        
        //Assert
        Debug.Log($"response code: {packet.ResponseCode}");
        Debug.Log($"response string: {packet.ResponseRaw}");
        
        Assert.IsTrue(requestSuccess);
    }
    
    [UnityTest]
    public IEnumerator SetProfile()
    {
        //Arrange
        InitGameObjects();
        GameClient.Instance.Init(true);
        bool requestSuccess = false;
            
        //Act
        IPacket packet = new SetProfilePacket(
                new SetProfileRequestArgs
                {
                    AccountId = 1
                })
            .OnSuccess(() => requestSuccess = true);
        GameClient.Instance.Send(packet);
        while (!packet.IsDone) 
            yield return new WaitForEndOfFrame();
        
        //Assert
        Debug.Log($"response code: {packet.ResponseCode}");
        Debug.Log($"response string: {packet.ResponseRaw}");
            
        Assert.IsTrue(requestSuccess);
    }
    
    [UnityTest]
    public IEnumerator GetFullAccountData()
    {
        //Arrange
        InitGameObjects();
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
        while (!packet.IsDone) 
            yield return new WaitForEndOfFrame();
            
        //Assert
        Debug.Log($"response code: {packet.ResponseCode}");
        Debug.Log($"response string: {packet.ResponseRaw}");
            
        Assert.IsTrue(requestSuccess);
    }
}
