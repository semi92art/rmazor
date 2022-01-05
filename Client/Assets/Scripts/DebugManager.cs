using DebugConsole;
using Entities;
using Games.RazorMaze.Views.InputConfigurators;
using Settings;
using UnityEngine.Events;

public interface IDebugManager : IInit
{
    void EnableDebug(bool _Enable);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    IDebugConsoleController Console { get; }
#endif
}

public class DebugManager : IDebugManager
{
    #region nonpublic members

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public IDebugConsoleController Console => DebugConsoleView.Instance.Controller;
#endif

    #endregion

    #region inject

    private IViewInputCommandsProceeder CommandsProceeder { get; }
    private IManagersGetter             Managers          { get; }
    private IDebugSetting               DebugSetting      { get; }

    public DebugManager(
        IViewInputCommandsProceeder _CommandsProceeder,
        IManagersGetter _Managers,
        IDebugSetting _DebugSetting)
    {
        CommandsProceeder = _CommandsProceeder;
        Managers = _Managers;
        DebugSetting = _DebugSetting;
    }

    #endregion

    #region api

    public bool              Initialized { get; private set; }
    public event UnityAction Initialize;
    public void Init()
    {
        DebugSetting.OnValueSet = EnableDebug;
        InitDebugConsole();
        EnableDebug(DebugSetting.Get());
        Initialize?.Invoke();
        Initialized = true;
    }

    public void EnableDebug(bool _Enable)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsoleView.Instance.enabled = _Enable;
#endif
    }
    
    #endregion

    #region nonpublic methods

    private void InitDebugConsole()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsoleView.Instance.Init(CommandsProceeder, Managers);
#endif
    }

    #endregion
}