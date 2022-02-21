using System.Collections;
using System.Threading.Tasks;
using Common;
using Common.Helpers;
using Common.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.TestTools;

public class ServerConnectionTests
{
    private static void InitGameObjects()
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
        string url = $"{GameClientUtils.ServerApiUrl}/timetest";
        var request = new UnityWebRequest(url, "GET")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };
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
}