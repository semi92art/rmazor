using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Providers
{
    public interface IColorProvider : IInit
    {
        event UnityAction<int, Color>  ColorChanged;
        void                           AddIgnorableForThemeSwitchColor(int    _ColorId);
        void                           RemoveIgnorableForThemeSwitchColor(int _ColorId);
        Color                          GetColor(int                           _Id);
        void                           SetColor(int                           _Id, Color _Color);
    }
}