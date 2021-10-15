using Entities;
using UnityEngine.Events;
using Utils;

public interface IDebugManager : IInit
{
    void EnableDebug(bool _Enable);
}

public class DebugManager : IDebugManager
{
    #region nonpublic members

#if DEVELOPMENT_BUILD
    private static GameObject _debugReporter;
#endif

    #endregion

    #region api
    
    public event UnityAction Initialized;
    public void Init()
    {
        InitDebugConsole();
        InitDebugReporter();
        Initialized?.Invoke();
    }

    public void EnableDebug(bool _Enable)
    {
        DebugConsole.DebugConsoleView.Instance.SetGoActive(_Enable);
#if DEVELOPMENT_BUILD
        _debugReporter.SetActive(_Enable);
#endif
    }
    
    #endregion

    #region nonpublic methods

    private static void InitDebugConsole()
    {
        bool debugOn = SaveUtils.GetValue<bool>(SaveKeyDebug.DebugUtilsOn);
        // SaveUtils.PutValue(SaveKeyDebug.DebugUtilsOn, debugOn); // FIXME
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsole.DebugConsoleView.Instance.Init();
        DebugConsole.DebugConsoleView.Instance.SetGoActive(debugOn);
#endif
    }
    
    private static void InitDebugReporter()
    {
#if DEVELOPMENT_BUILD
        _debugReporter = GameHelpers.PrefabUtilsEx.InitPrefab(
                null,
                "debug_console",
                "reporter");
        _debugReporter.SetActive(debugOn);
#endif
    }

    #endregion
}