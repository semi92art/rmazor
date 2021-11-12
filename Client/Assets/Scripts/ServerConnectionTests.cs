using System.Collections;
using System.Threading.Tasks;
using GameHelpers;
using Network;
using Network.Packets;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using Utils;

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
        CommonData.Testing = true;
        GameClient.Instance.Init();
        string url = $"{GameClientUtils.ServerApiUrl}/timetest";

        var request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        //wait 5 seconds before cancel
        bool stopWaiting = false;
        Task.Run(() => CommonUtils.WaitForSecs(5f, () => stopWaiting = true));

        //Act
        request.SendWebRequest();
        while (!request.isDone && !stopWaiting)
            yield return new WaitForEndOfFrame();
            
        //Assert
        Assert.IsTrue(NetworkUtils.IsPacketSuccess(request.responseCode));
            
        Dbg.Log($"Response code: {request.responseCode}");
        Dbg.Log(request.downloadHandler.text);
    }
    
    [UnityTest]
    public IEnumerator LoginByTestGuestUser()
    {
        //Arrange
        InitGameObjects();
        CommonData.Testing = true;
        GameClient.Instance.Init();
        bool requestSuccess = false;
        
        //Act
        IPacket packet = new LoginUserPacket(
            new LoginUserPacketRequestArgs
            {
                Name = "test_user_do_not_delete",
                PasswordHash = ""
            })
            .OnFail(() => requestSuccess = false)
            .OnSuccess(() => requestSuccess = true) as LoginUserPacket;
        GameClient.Instance.Send(packet);
        while (!packet.IsDone) 
            yield return new WaitForEndOfFrame();
        
        //Assert
        Dbg.Log($"response code: {packet.ResponseCode}");
        Dbg.Log($"response string: {packet.ResponseRaw}");
        
        Assert.IsTrue(requestSuccess);
    }

    [UnityTest]
    public IEnumerator RegisterByTestGuestUser()
    {
        //Arrange
        InitGameObjects();
        CommonData.Testing = true;
        GameClient.Instance.Init();
        bool requestSuccess = false;
        
        //Act
        IPacket packet = new RegisterUserPacket(
            new RegisterUserPacketRequestArgs
            {
                Name = $"test_{CommonUtils.GetUniqueId()}",
                PasswordHash = "",
                GameId = 1
            })
            .OnSuccess(() => requestSuccess = true);
        GameClient.Instance.Send(packet);
        while (!packet.IsDone) 
            yield return new WaitForEndOfFrame();
        
        //Assert
        Dbg.Log($"response code: {packet.ResponseCode}");
        Dbg.Log($"response string: {packet.ResponseRaw}");
        
        Assert.IsTrue(requestSuccess);
    }
}
