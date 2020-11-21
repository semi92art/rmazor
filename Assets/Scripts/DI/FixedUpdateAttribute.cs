using System;

namespace DI
{
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