using System;

namespace DI
{
    public abstract class UpdateBaseAttribute : Attribute, IOrder, IDoNotDestroyOnLoad
    {
        public int Order { get; }
        public bool DoNotDestroyOnLoad { get; }
        
        protected UpdateBaseAttribute(int _Order = 0, bool _DoNotDestroyOnLoad = false)
        {
            Order = _Order;
            DoNotDestroyOnLoad = _DoNotDestroyOnLoad;
        }
    }
    
    /// <summary>
    /// Imitates running method on Monobehaviour Update
    /// Use this attribute for methods of not Monobehaviour objects
    /// Warning: do non destruct object before scene change
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class UpdateAttribute : UpdateBaseAttribute
    {
        public UpdateAttribute(int _Order = 0, bool _DoNotDestroyOnLoad = false) : base(_Order, _DoNotDestroyOnLoad)
        { }
    }
    
    /// <summary>
    /// Imitates running method on Monobehaviour FixedUpdate
    /// Use this attribute for methods of not Monobehaviour objects
    /// Warning: do non destruct object before scene change
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class FixedUpdateAttribute : UpdateBaseAttribute
    {
        public FixedUpdateAttribute(int _Order = 0, bool _DoNotDestroyOnLoad = false) : base(_Order, _DoNotDestroyOnLoad)
        { }
    }
    
    /// <summary>
    /// Imitates running method on Monobehaviour LateUpdate
    /// Use this attribute for methods of not Monobehaviour objects
    /// Warning: do non destruct object before scene change
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LateUpdateAttribute : UpdateBaseAttribute
    {
        public LateUpdateAttribute(int _Order = 0, bool _DoNotDestroyOnLoad = false) : base(_Order, _DoNotDestroyOnLoad)
        { }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class DrawGizmosAttribute : UpdateBaseAttribute
    {
        public DrawGizmosAttribute(int _Order = 0, bool _DoNotDestroyOnLoad = false) : base(_Order, _DoNotDestroyOnLoad)
        { }
    }
}