using System;

namespace Exceptions
{
    public class SwitchCaseNotImplementedException : ArgumentOutOfRangeException
    {
        public SwitchCaseNotImplementedException(object _Value) 
            : base( $"No realization for {Enum.GetName(_Value.GetType(), _Value)} in type {_Value.GetType().Name}") 
        { }
    }
}