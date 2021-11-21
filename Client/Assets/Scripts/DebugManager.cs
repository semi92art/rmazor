using Entities;
using Games.RazorMaze.Views.InputConfigurators;
using Settings;
using UnityEngine.Events;

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
    
    public event UnityAction Initialized;
    public void Init()
    {
        DebugSetting.OnValueSet = EnableDebug;
        InitDebugConsole();
        EnableDebug(DebugSetting.Get());
        Initialized?.Invoke();
    }

    public void EnableDebug(bool _Enable)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsole.DebugConsoleView.Instance.enabled = _Enable;
#endif
    }
    
    #endregion

    #region nonpublic methods

    private void InitDebugConsole()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsole.DebugConsoleView.Instance.Init(CommandsProceeder, Managers);
#endif
    }

    #endregion
}