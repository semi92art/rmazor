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
                    ELanguage.English   => "montserrat-ace-regular",
                    ELanguage.Russian   => "montserrat-ace-regular",
                    ELanguage.German    => "montserrat-ace-regular",
                    ELanguage.Spanish   => "montserrat-ace-regular",
                    ELanguage.Portugal  => "montserrat-ace-regular",
                    ELanguage.Japaneese => "japaneese",
                    ELanguage.Korean    => "korean",
                    _                   => throw new SwitchExpressionException(_Language)
                },
                ETextType.MenuUI => _Language switch
                {
                    ELanguage.English   => "lilita-one-outline-54",
                    ELanguage.Russian   => "fulbo-argenta-cirillic",
                    ELanguage.German    => "fulbo-argenta-cirillic",
                    ELanguage.Spanish   => "fulbo-argenta-cirillic",
                    ELanguage.Portugal  => "fulbo-argenta-cirillic",
                    ELanguage.Japaneese => "japaneese",
                    ELanguage.Korean    => "korean",
                    _                   => throw new SwitchExpressionException(_Language)
                },
                ETextType.Currency => "segoe-ui",
                _                  => throw new SwitchExpressionException(_TextType)
            };
            return PrefabSetManager.GetObject<TMP_FontAsset>(PrefabSetName, prefabName);
        }
    }
}