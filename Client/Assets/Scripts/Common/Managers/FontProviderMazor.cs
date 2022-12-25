using System.Runtime.CompilerServices;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using TMPro;

namespace Common.Managers
{
    public class FontProviderMazor : IFontProvider
    {
        #region constants

        private const string PrefabSetName = "fonts";

        #endregion
        
        private IPrefabSetManager PrefabSetManager { get; }

        public FontProviderMazor(IPrefabSetManager _PrefabSetManager)
        {
            PrefabSetManager = _PrefabSetManager;
        }

        public TMP_FontAsset GetFont(ETextType _TextType, ELanguage _Language)
        {
            const string defaultFontGameUi = "montserrat_ace_regular";
            const string defaultFontMenuUi = "multiround_pro";
            string prefabName = _TextType switch
            {
                ETextType.GameUI => _Language switch
                {
                    ELanguage.English   => defaultFontGameUi,
                    ELanguage.Russian   => defaultFontGameUi,
                    ELanguage.German    => defaultFontGameUi,
                    ELanguage.Spanish   => defaultFontGameUi,
                    ELanguage.Portugal  => defaultFontGameUi,
                    ELanguage.Japanese  => "japanese_game",
                    ELanguage.Korean    => "korean_game",
                    _                   => throw new SwitchExpressionException(_Language)
                },
                ETextType.MenuUI => _Language switch
                {
                    ELanguage.English  => defaultFontMenuUi,
                    ELanguage.Russian  => defaultFontMenuUi,
                    ELanguage.German   => defaultFontMenuUi,
                    ELanguage.Spanish  => defaultFontMenuUi,
                    ELanguage.Portugal => defaultFontMenuUi,
                    ELanguage.Japanese => "japanese_menu",
                    ELanguage.Korean   => "korean_menu",
                    _                  => throw new SwitchExpressionException(_Language)
                },
                _                  => throw new SwitchExpressionException(_TextType)
            };
            return PrefabSetManager.GetObject<TMP_FontAsset>(PrefabSetName, prefabName);
        }
    }
}