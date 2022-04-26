using Common.Enums;
using Common.Exceptions;
using Common.Utils;
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
            string prefabName = string.Empty;
            switch (_TextType)
            {
                case ETextType.GameUI:
                    switch (_Language)
                    {
                        case ELanguage.Russian:
                        case ELanguage.English:
                        case ELanguage.German:
                        case ELanguage.Spanish:
                        case ELanguage.Portugal:
                            prefabName = "montserrat-ace-regular";
                            break;
                        case ELanguage.Japaneese:
                            prefabName = "japaneese";
                            break;
                        case ELanguage.Korean:
                            prefabName = "korean";
                            break;
                        default:
                            throw new SwitchCaseNotImplementedException(_Language);
                    }
                    break;
                case ETextType.MenuUI:
                    switch (_Language)
                    {
                        case ELanguage.Russian:
                        case ELanguage.English:
                        case ELanguage.German:
                        case ELanguage.Spanish:
                        case ELanguage.Portugal:
                            prefabName = "fulbo-argenta-cirillic";
                            break;
                        case ELanguage.Japaneese:
                            prefabName = "japaneese";
                            break;
                        case ELanguage.Korean:
                            prefabName = "korean";
                            break;
                        default:
                            throw new SwitchCaseNotImplementedException(_Language);
                    }
                    break;
                case ETextType.Currency:
                    prefabName = "segoe-ui";
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_TextType);
            }
            return PrefabSetManager.GetObject<TMP_FontAsset>(PrefabSetName, prefabName);
        }
    }
}