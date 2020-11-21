using System;

namespace DI
{
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