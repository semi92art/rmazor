using System;

namespace Common.Exceptions
{
    public class SwitchCaseNotImplementedException : ArgumentOutOfRangeException
    {
        public SwitchCaseNotImplementedException(object _Value)
            : base("No realization for " +
                   $"{(_Value.GetType().IsEnum ? Enum.GetName(_Value.GetType(), _Value) : _Value)} " +
                   $"in type {_Value.GetType().Name}") { }
    }
}