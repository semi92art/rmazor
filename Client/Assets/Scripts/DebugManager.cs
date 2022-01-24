using Common;
using Managers;
using RMAZOR.Views.InputConfigurators;
using Settings;
using UnityEngine.Events;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using DebugConsole;
#endif

public interface IDebugManager : IInit
{
    void EnableDebug(bool _Enable);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    IDebugConsoleController Console { get; }
    event VisibilityChangedHandler       VisibilityChanged;
#endif
}

public class DebugManager : IDebugManager
{
    #region nonpublic members

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public IDebugConsoleController Console => DebugConsoleView.Instance.Controller;
    public event VisibilityChangedHandler       VisibilityChanged;

#endif

    #endregion

    #region inject

    private IViewInputCommandsProceeder CommandsProceeder { get; }
    private IManagersGetter             Managers          { get; }
    private IDebugSetting               DebugSetting      { get; }

    public DebugManager(
        IViewInputCommandsProceeder _CommandsProceeder,
        IManagersGetter             _Managers,
        IDebugSetting               _DebugSetting)
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Console.VisibilityChanged += _Value => VisibilityChanged?.Invoke(_Value);
#endif
        DebugSetting.OnValueSet = EnableDebug;
        InitDebugConsole();
        EnableDebug(DebugSetting.Get());
        Initialize?.Invoke();
        Initialized = true;
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