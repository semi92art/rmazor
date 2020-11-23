using System;

namespace DI
{
    /// <summary>
    /// Imitates running method on Monobehaviour LateUpdate
    /// Use this attribute for methods of not Monobehaviour objects
    /// Warning: do non destruct object before scene change
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LateUpdateAttribute : Attribute, IOrder
    {
        public int Order { get; }
        
        public LateUpdateAttribute(int _Order = 0)
        {
            Order = _Order;
        }
    }
}