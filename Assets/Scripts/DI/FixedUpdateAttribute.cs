using System;

namespace DI
{
    /// <summary>
    /// Imitates running method on Monobehaviour FixedUpdate
    /// Use this attribute for methods of not Monobehaviour objects
    /// Warning: do non destruct object before scene change
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class FixedUpdateAttribute : Attribute, IOrder
    {
        public int Order { get; }
        
        public FixedUpdateAttribute(int _Order = 0)
        {
            Order = _Order;
        }
    }
}