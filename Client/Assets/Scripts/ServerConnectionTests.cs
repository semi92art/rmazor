using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Helpers;
using Common.Utils;
using StansAssets.Foundation.Extensions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.TestTools;

public class ServerConnectionTests
{
    private static void InitGameObjects()
    {
        
        var corRunner = GameObject.Find("CoroutinesRunner");
        if (corRunner.IsNotNull()) 
            return;
        corRunner = new GameObject("CoroutinesRunner");
        corRunner.AddComponent<DontDestroyOnLoad>();
    }
    
    private static UnityAction WaitForSecs(float _Seconds, UnityAction _OnFinish)
    {
        return () =>
        {
            int millisecs = Mathf.RoundToInt(_Seconds * 1000);
            Thread.Sleep(millisecs);
            _OnFinish?.Invoke();
        };
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
        Task.Run(() => WaitForSecs(5f, () => stopWaiting = true));
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