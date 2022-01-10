namespace Ticker
{
    public interface IUpdateTick { void UpdateTick(); }
    public interface IFixedUpdateTick {void FixedUpdateTick(); }
    public interface ILateUpdateTick { void LateUpdateTick(); }
    public interface IDrawGizmosTick { void DrawGizmosTick(); }
    public interface IApplicationPause { void OnApplicationPause(bool _Pause); }
}