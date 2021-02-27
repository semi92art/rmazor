using System.Collections;
using System.Threading.Tasks;
using Constants;
using GameHelpers;
using Network;
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
                Name = "test_user_do_not_delete",
                PasswordHash = ""
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
                Name = $"test_{CommonUtils.GetUniqueId()}",
                PasswordHash = "",
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
