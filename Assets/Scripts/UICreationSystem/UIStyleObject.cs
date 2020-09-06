using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UICreationSystem
{
    [CreateAssetMenu(fileName = "new_style", menuName = "Style", order = 1)]
    public class UIStyleObject : ScriptableObject
    {
        #region public fields

        [Header("Button:")]
        public bool interactable;
        public Selectable.Transition transition;
        public UiState normalState;
        public UiState highlightedState;
        public UiState pressedState;
        public UiState selectedState;
        public UiState disabledState;
        [Range(1f, 5f)] 
        public float colorMultiplyer = 1f;
        public float fadeDuration;

        [Header("Image:")]
        public Sprite sprite;
        public Color imageColor = Color.white;
        public bool raycastImageTarget = true;
        public Image.Type imageType = Image.Type.Simple;
        public bool useSpriteMesh;
        public bool preserveAspect = true;
        public float pixelsPerUnityMultyply;
        public Image.FillMethod fillMethod;
        [Range(0, 3)] 
        public int fillOrigin;
        public bool fillClockwise = true;

        [Header("Text:")]
        public Color textColor = Color.white;
        public bool raycastTextTarget;
        public string textID;
        public Font font;
        public FontStyle fontStyle = FontStyle.Normal;
        public int fontSize = 10;
        public TextAnchor alignment = TextAnchor.MiddleCenter;
        public HorizontalWrapMode horizontalOverflow = HorizontalWrapMode.Wrap;
        public VerticalWrapMode verticalOverflow = VerticalWrapMode.Truncate;
        public bool richText;
        public bool alignByGeometry;
        public float lineSpacing = 1f;
        public bool bestFit;
        public bool isShadow;
        public Color shadowEffectColor = Color.black;
        public Vector2 shadowEffectDistance;
        public bool shadowUseGraphicsAlpha;

        [Header("TMP Text")] public GameObject text;
        [Header("TMP Button")] public GameObject button;
        [Header("TMP Input Field:")] public GameObject inputField;
        [Header("Image")] public GameObject image;
        [Header("Toggle")] public GameObject toggle;
        [Header("Slider")] public GameObject slider;
        [Header("Scrollbar")] public GameObject scrollbar;

        #endregion
    }
}


