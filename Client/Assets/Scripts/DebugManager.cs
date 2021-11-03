using Entities;
using Games.RazorMaze.Views.InputConfigurators;
using UnityEngine.Events;
using Utils;

public interface IDebugManager : IInit
{
    void EnableDebug(bool _Enable);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    DebugConsole.IDebugConsoleController Console { get; }
#endif
}

public class DebugManager : IDebugManager
{
    #region nonpublic members

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public DebugConsole.IDebugConsoleController Console => DebugConsole.DebugConsoleView.Instance.Controller;
#endif
    
#if DEVELOPMENT_BUILD
    private static GameObject _debugReporter;
#endif

    #endregion

    #region inject

    private IViewInput Input { get; }
    
    public DebugManager(IViewInput _Input)
    {
        Input = _Input;
    }

    #endregion

    #region api
    
    public event UnityAction Initialized;
    public void Init()
    {
        InitDebugConsole();
        InitDebugReporter();
        bool doEnable = SaveUtils.GetValue<bool>(SaveKey.DebugUtilsOn);
        EnableDebug(doEnable);
        Initialized?.Invoke();
    }

    public void EnableDebug(bool _Enable)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsole.DebugConsoleView.Instance.enabled = _Enable;
#endif
#if DEVELOPMENT_BUILD
        _debugReporter.SetActive(_Enable);
#endif
    }
    
    #endregion

    #region nonpublic methods

    private void InitDebugConsole()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsole.DebugConsoleView.Instance.Init(Input);
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