using System.Linq;
using Exceptions;
using PygmyMonkey.ColorPalette;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;

namespace Helpers
{
    [ExecuteInEditMode]
    public class UiPalettePainter : MonoBehaviour
    {
        public enum UiGraphicType
        {
            MainBackground,
            DialogBackground,
            DialogItem,
            Button,
            Text,
            TextError,
            TextPlaceHolder
        }

        [SerializeField] private UiGraphicType graphicType;
        
        private void OnEnable()
        {
            ColorPaletteData cpd = PrefabInitializer
                .GetObject<ColorPaletteData>("color_palettes", "color_palettes");
            //string colorSchemeName = SaveUtils.GetValue<string>()
            ColorPalette cp = cpd.colorPaletteList
                .FirstOrDefault(_Cp => _Cp.name == UiManager.Instance.ColorScheme);
            if (cp == null)
            {
                Debug.LogError($"Color scheme with name {UiManager.Instance.ColorScheme} was not found");
                return;
            }
            
            var button = GetComponent<Button>();
            if (button != null)
            {
                graphicType = UiGraphicType.Button;
                button.transition = Selectable.Transition.ColorTint;
                button.colors = new ColorBlock
                {
                    disabledColor = cp.getColorFromName("Button Disabled").color,
                    normalColor = cp.getColorFromName("Button Normal").color,
                    pressedColor = cp.getColorFromName("Button Pressed").color
                };
                return;
            }

            var image = GetComponent<Image>();
            if (image != null)
            {
                switch (graphicType)
                {
                    case UiGraphicType.MainBackground:
                        image.color = cp.getColorFromName("Image Main Background").color;
                        break;
                    case UiGraphicType.DialogBackground:
                        image.color = cp.getColorFromName("Image Dialog Background").color;
                        break;
                    case UiGraphicType.DialogItem:
                        image.color = cp.getColorFromName("Image Dialog Item").color;
                        break;
                    default:
                        throw new InvalidEnumArgumentExceptionEx(graphicType);
                }

                return;
            }

            var text = GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                switch (graphicType)
                {
                    case UiGraphicType.Text:
                        text.color = cp.getColorFromName("Text").color;
                        break;
                    case UiGraphicType.TextError:
                        text.color = cp.getColorFromName("Text Error").color;
                        break;
                    case UiGraphicType.TextPlaceHolder:
                        text.color = cp.getColorFromName("Text PlaceHolder").color;
                        break;
                    default:
                        throw new InvalidEnumArgumentExceptionEx(graphicType);
                }
            }
        }
    }
}