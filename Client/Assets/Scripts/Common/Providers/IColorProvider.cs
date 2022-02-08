using UnityEngine;
using UnityEngine.Events;

namespace Common.Providers
{
    public enum EColorTheme
    {
        Light = 0,
        Dark  = 1
    }
    
    public interface IColorProvider : IInit
    {
        event UnityAction<int, Color>  ColorChanged;
        event UnityAction<EColorTheme> ColorThemeChanged;
        bool                           DarkThemeAvailable { get; set; }
        EColorTheme                    CurrentTheme       { get; }
        Color                          GetColor(int         _Id);
        void                           SetColor(int         _Id, Color _Color);
        void                           SetTheme(EColorTheme _Theme);
    }
}