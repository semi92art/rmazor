using UnityEngine;

namespace Common.Extensions
{
    public static class ColorExtensions
    {
        public static Color SetR(this Color _C, float _R)
        {
            return new Color(_R, _C.g, _C.b, _C.a);
        }

        public static Color SetG(this Color _C, float _G)
        {
            return new Color(_C.r, _G, _C.b, _C.a);
        }

        public static Color SetB(this Color _C, float _B)
        {
            return new Color(_C.r, _C.g, _B, _C.a);
        }
        
        public static Color SetA(this Color _C, float _A)
        {
            return new Color(_C.r, _C.g, _C.b, _A);
        }
    }
}