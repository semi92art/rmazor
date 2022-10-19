using System.Runtime.CompilerServices;
using Common.Enums;
using TMPro;

namespace Common.Managers
{
    public enum ETextType
    {
        GameUI,
        MenuUI,
        Currency
    }
    
    public interface IFontProvider
    {
        TMP_FontAsset GetFont(ETextType _TextType, ELanguage _Language);
    }

    public class DefaultFontProvider : IFontProvider
    {
        #region constants

        private const string PrefabSetName = "fonts";

        #endregion
        
        private IPrefabSetManager PrefabSetManager { get; }

        public DefaultFontProvider(IPrefabSetManager _PrefabSetManager)
        {
            PrefabSetManager = _PrefabSetManager;
        }

        public TMP_FontAsset GetFont(ETextType _TextType, ELanguage _Language)
        {
            string prefabName = _TextType switch
            {
                ETextType.GameUI => _Language switch
                {
                    ELanguage.English   => "montserrat_ace_regular",
                    ELanguage.Russian   => "montserrat_ace_regular",
                    ELanguage.German    => "montserrat_ace_regular",
                    ELanguage.Spanish   => "montserrat_ace_regular",
                    ELanguage.Portugal  => "montserrat_ace_regular",
                    ELanguage.Japanese  => "japanese_game",
                    ELanguage.Korean    => "korean_game",
                    _                   => throw new SwitchExpressionException(_Language)
                },
                ETextType.MenuUI => _Language switch
                {
                    ELanguage.English   => "lilita_one_54_regular",
                    ELanguage.Russian   => "efour_pro",
                    ELanguage.German    => "efour_pro",
                    ELanguage.Spanish   => "efour_pro",
                    ELanguage.Portugal  => "efour_pro",
                    ELanguage.Japanese  => "japanese_menu",
                    ELanguage.Korean    => "korean_menu",
                    _                   => throw new SwitchExpressionException(_Language)
                },
                ETextType.Currency => "segoe_ui",
                _                  => throw new SwitchExpressionException(_TextType)
            };
            return PrefabSetManager.GetObject<TMP_FontAsset>(PrefabSetName, prefabName);
        }
    }
}