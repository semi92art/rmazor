namespace Common.Ticker
{
    public interface IUpdateTick
    {
        /// <summary>
        /// Do not call manually!!!
        /// </summary>
        void UpdateTick();
    }

    public interface IFixedUpdateTick
    {
        /// <summary>
        /// Do not call manually!!!
        /// </summary>
        void FixedUpdateTick();
    }

    public interface ILateUpdateTick
    {
        /// <summary>
        /// Do not call manually!!!
        /// </summary>
        void LateUpdateTick();
    }

    public interface IDrawGizmosTick
    {
        /// <summary>
        /// Do not call manually!!!
        /// </summary>
        void DrawGizmosTick();
    }

    public interface IApplicationPause
    {
        /// <summary>
        /// Do not call manually!!!
        /// </summary>
        void OnApplicationPause(bool _Pause);
    }

    public interface IApplicationFocus
    {
        /// <summary>
        /// Do not call manually!!!
        /// </summary>
        void OnApplicationFocus(bool _Focus);
    }

    public interface IDestroy
    {
        /// <summary>
        /// Do not call manually!!!
        /// </summary>
        void OnDestroy();
    }
}