using System;

namespace DI
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UpdateAttribute : Attribute, IOrder
    {
        public int Order { get; }
        
        public UpdateAttribute(int _Order = 0)
        {
            Order = _Order;
        }
    }
}