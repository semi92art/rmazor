using System;
using System.ComponentModel;
using System.Reflection;

namespace Exceptions
{
    public class InvalidEnumArgumentExceptionEx : InvalidEnumArgumentException
    {
        public InvalidEnumArgumentExceptionEx(object _Value)
        {
            var type = _Value.GetType();
            var name = Enum.GetName(type, _Value);

            FieldInfo messageInfo = typeof(Exception).GetField("_message",
                BindingFlags.CreateInstance | BindingFlags.NonPublic);
            messageInfo?.SetValue(this, $"No realization for {name} in type {type.Name}");
        }
    }
}