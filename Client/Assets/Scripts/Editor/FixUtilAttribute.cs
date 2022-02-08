using System;

namespace RMAZOR.Editor
{
    public enum FixUtilColor
    {
        Default, Red, Green, Blue
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class FixUtilAttribute : Attribute
    {
        public FixUtilColor Color { get; }

        public FixUtilAttribute(FixUtilColor _Color = FixUtilColor.Default)
        {
            Color = _Color;
        }
    }
}