using System;
using System.Collections.Generic;
using LeTai.Effects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LeTai.TrueShadow
{
[DisallowMultipleComponent]
[HelpURL("https://leloctai.com/trueshadow/docs/articles/customize.html")]
[ExecuteAlways]
public partial class TrueShadow : UIBehaviour, IMeshModifier, ICanvasElement
{
    static readonly Color DEFAULT_COLOR = new Color(0, 0, 0, .15f);

    [Tooltip("Size of the shadow")]
    [SerializeField] float size = 32;

    [Tooltip("Direction to offset the shadow toward")]
    [Knob] [SerializeField] float offsetAngle = 90;

    [Tooltip("How far to offset the shadow")]
    [SerializeField] float offsetDistance = 4;

    [SerializeField] Vector2 offset = Vector2.zero;

    [Tooltip("Tint the shadow")]
    [SerializeField] Color color = DEFAULT_COLOR;

    [Tooltip("Blend mode of the shadow")]
    [SerializeField] BlendMode blendMode;

    [Tooltip("Position the shadow GameObject as previous sibling of the UI element")]
    [SerializeField] bool shadowAsSibling;

    [Tooltip("Cut the source image from the shadow to avoid shadow showing behind semi-transparent UI")]
    [SerializeField] bool cutout;

    [Tooltip(
        "Bake the shadow to a sprite to reduce CPU and GPU usage at runtime, at the cost of storage, memory and flexibility")]
    [SerializeField] bool baked;

    [Tooltip(
        "How to obtain the color of the area outside of the source image. Automatically set based on Blend Mode. You should only change this setting if you are using some very custom UI that require it")]
    [SerializeField] ColorBleedMode colorBleedMode;

    [SerializeField] bool modifiedFromInspector = false;

    public float Size
    {
        get => size;
        set
        {
            var newSize = Mathf.Max(0, value);
            if (modifiedFromInspector || !Mathf.Approximately(size, newSize))
            {
                modifiedFromInspector = false;

                SetLayoutDirty();
                SetTextureDirty();
            }

            size = newSize;
        }
    }

    public float OffsetAngle
    {
        get => offsetAngle;
        set
        {
            var newValue = (value + 360f) % 360f;
            if (modifiedFromInspector || !Mathf.Approximately(offsetAngle, newValue))
            {
                modifiedFromInspector = false;

                SetLayoutDirty();
                if (Cutout)
                    SetTextureDirty();
            }

            offsetAngle = newValue;
            offset      = Math.AngleDistanceVector(offsetAngle, offset.magnitude, Vector2.right);
        }
    }

    public float OffsetDistance
    {
        get => offsetDistance;
        set
        {
            var newValue = Mathf.Max(0, value);
            if (modifiedFromInspector || !Mathf.Approximately(offsetDistance, newValue))
            {
                modifiedFromInspector = false;

                SetLayoutDirty();
                if (Cutout)
                    SetTextureDirty();
            }

            offsetDistance = newValue;
            offset = offset.sqrMagnitude < 1e-6f
                         ? Math.AngleDistanceVector(offsetAngle, offsetDistance, Vector2.right)
                         : offset.normalized * offsetDistance;
        }
    }

    public Color Color
    {
        get => color;
        set
        {
            if (modifiedFromInspector || value != color)
            {
                modifiedFromInspector = false;

                SetLayoutDirty();
            }

            color = value;
        }
    }

    public BlendMode BlendMode
    {
        get => blendMode;
        set
        {
            // Work around for Unity bug causing references loss
            if (!Graphic || !CanvasRenderer)
                return;

            blendMode = value;
            shadowRenderer.UpdateMaterial();

            switch (blendMode)
            {
                case BlendMode.Normal:
                    ColorBleedMode = ColorBleedMode.ImageColor;
                    break;
                case BlendMode.Addictive:
                    ColorBleedMode = ColorBleedMode.Black;
                    break;
                case BlendMode.Multiply:
                    ColorBleedMode = ColorBleedMode.White;
                    break;
                default:
                    ColorBleedMode = ColorBleedMode.ImageColor;
                    throw new ArgumentOutOfRangeException(nameof(blendMode), blendMode, null);
            }
        }
    }

    public ColorBleedMode ColorBleedMode
    {
        get => colorBleedMode;
        set
        {
            if (modifiedFromInspector || colorBleedMode != value)
            {
                modifiedFromInspector = false;

                colorBleedMode = value;
                SetTextureDirty();
            }
        }
    }

    public Color ClearColor
    {
        get
        {
            switch (colorBleedMode)
            {
                case ColorBleedMode.ImageColor:
                    return Graphic.color.WithA(0);
                case ColorBleedMode.ShadowColor:
                    return Color.WithA(0);
                case ColorBleedMode.Black:
                    return Color.clear;
                case ColorBleedMode.White:
                    return new Color(1, 1, 1, 0);
                case ColorBleedMode.Plugin:
                    return casterClearColorProvider?.GetTrueShadowCasterClearColor() ?? Color.clear;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public bool ShadowAsSibling
    {
        get => shadowAsSibling;
        set
        {
            shadowAsSibling = value;
            ShadowRenderer.ClearMaskMaterialCache();
            if (shadowAsSibling)
            {
                ShadowSorter.Instance.Register(this);
            }
            else
            {
                ShadowSorter.Instance.UnRegister(this);
                var rendererTransform = shadowRenderer.transform;
                rendererTransform.SetParent(transform, true);
                rendererTransform.SetSiblingIndex(0);
            }
        }
    }

    public bool Cutout
    {
        get => !shadowAsSibling || cutout;
        set => cutout = value;
    }

    public bool Baked
    {
        get => baked;
        set
        {
            baked = value;

            if (baked)
            {
                BakeShadows();
            }
            else
            {
                RemoveBakedShadow();
                SetTextureDirty();
            }
        }
    }

    [SerializeField] List<Sprite> bakedShadows;

    public Vector2 Offset => offset;

    internal ShadowRenderer shadowRenderer;

    internal ScalableBlur       blurProcessor = new ScalableBlur();
    internal ScalableBlurConfig blurConfig;

    internal Mesh           SpriteMesh     { get; set; }
    internal Graphic        Graphic        { get; set; }
    internal CanvasRenderer CanvasRenderer { get; set; }
    internal RectTransform  RectTransform  { get; private set; }

    internal Texture Content
    {
        get
        {
            switch (Graphic)
            {
                case Image image:
                    var sprite = image.overrideSprite;
                    return sprite ? sprite.texture : null;
                case RawImage rawImage: return rawImage.texture;
                default:                return null;
            }
        }
    }

    int textureRevision;

    internal int ContentHash
    {
        get
        {
            switch (Graphic)
            {
                case Image image:
                    int spriteHash = 0;
                    if (image.sprite)
                        spriteHash = image.sprite.GetHashCode();

                    var hash = HashUtils.CombineHashCodes(
                        textureRevision,
                        Cutout ? 1 : 0,
                        // Hack until we have separated cutout cache, or proper sibling mode
                        Mathf.RoundToInt(Offset.x * 100),
                        Mathf.RoundToInt(Offset.y * 100),
                        (int) ColorBleedMode,
                        spriteHash,
                        (int) image.type,
                        Mathf.RoundToInt(image.color.r * 255),
                        Mathf.RoundToInt(image.color.g * 255),
                        Mathf.RoundToInt(image.color.b * 255),
                        Mathf.RoundToInt(image.color.a * 255),
                        Mathf.RoundToInt(image.fillAmount * 360 * 20),
                        (int) image.fillMethod,
                        image.fillOrigin,
                        image.fillClockwise ? 1 : 0
                    );

                    var rotationHash = Cutout ? transform.rotation.GetHashCode() : 0;

                    hash = HashUtils.CombineHashCodes(hash, rotationHash);

                    return hash;
                case RawImage rawImage: return (rawImage.texture, textureRevision).GetHashCode();
                default:                return GetHashCode();
            }
        }
    }

#if LETAI_TRUESHADOW_DEBUG
    public bool alwaysRender;
#endif

    ShadowContainer shadowContainer;

    bool          textureDirty;
    bool          layoutDirty;
    internal bool hierachyDirty;

    protected override void Awake()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.undoRedoPerformed += ApplySerializedData;
#endif
        if (ShadowAsSibling)
            ShadowSorter.Instance.Register(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        RectTransform  = GetComponent<RectTransform>();
        Graphic        = GetComponent<Graphic>();
        CanvasRenderer = GetComponent<CanvasRenderer>();
        if (!SpriteMesh) SpriteMesh = new Mesh();

        InitializePlugins();

        if (bakedShadows == null)
            bakedShadows = new List<Sprite>(4);

        ShaderProperties.Init(8);
        if (!blurConfig)
        {
            blurConfig           = ScriptableObject.CreateInstance<ScalableBlurConfig>();
            blurConfig.hideFlags = HideFlags.HideAndDontSave;
            blurConfig.Strength  = Size;
        }

        blurProcessor.Init(blurConfig);

        InitInvalidator();

        ShadowRenderer.Initialize(this, ref shadowRenderer);

        Canvas.willRenderCanvases += OnWillRenderCanvas;

        if (Graphic)
            Graphic.SetVerticesDirty();

#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
#endif
    }

    public void ApplySerializedData()
    {
        Size            = size;
        OffsetAngle     = offsetAngle;
        OffsetDistance  = offsetDistance;
        BlendMode       = blendMode;
        ShadowAsSibling = shadowAsSibling;

        SetLayoutDirty();
        SetHierachyDirty();
    }

    protected override void OnDisable()
    {
        Canvas.willRenderCanvases -= OnWillRenderCanvas;
        TerminateInvalidator();

        if (shadowRenderer) shadowRenderer.gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        ShadowSorter.Instance.UnRegister(this);
        if (shadowRenderer) shadowRenderer.Dispose();

        ShadowFactory.Instance.ReleaseContainer(shadowContainer);

#if UNITY_EDITOR
        UnityEditor.Undo.undoRedoPerformed -= ApplySerializedData;
#endif
    }

    bool ShouldPerformWorks()
    {
        bool isBothRendererCulled = CanvasRenderer && CanvasRenderer.cull &&
                                    shadowRenderer.CanvasRenderer && shadowRenderer.CanvasRenderer.cull;
        return isActiveAndEnabled && !isBothRendererCulled;
    }

    void LateUpdate()
    {
        CheckTransformDirtied();

        if (!shadowAsSibling)
        {
            CheckTransformDirtied();
            if (hierachyDirty)
            {
                shadowRenderer.transform.SetParent(RectTransform, false);
                shadowRenderer.transform.SetSiblingIndex(0);
            }
        }
    }

    public void Rebuild(CanvasUpdate executing)
    {
        if (!ShouldPerformWorks()) return;

        if (executing == CanvasUpdate.PostLayout)
        {
            if (layoutDirty)
            {
                shadowRenderer.ReLayout();
                layoutDirty = false;
            }
        }
    }

    void OnWillRenderCanvas()
    {
#if LETAI_TRUESHADOW_DEBUG
        if (alwaysRender) textureDirty = true;
#endif

        if (!ShouldPerformWorks()) return;

        if (textureDirty)
        {
            blurConfig.Strength = Size;

            if (!Baked)
            {
                ShadowFactory.Instance.Get(new ShadowRenderingRequest(this), ref shadowContainer);
                shadowRenderer.SetTexture(shadowContainer?.Texture);
            }
            else
            {
                if (bakedShadows != null && bakedShadows.Count > 0)
                    shadowRenderer.SetSprite(bakedShadows[0]);
            }

            textureDirty = false;
        }
    }

    public void LayoutComplete() { }

    public void GraphicUpdateComplete() { }

    public void SetTextureDirty()
    {
        textureDirty = true;
        unchecked
        {
            textureRevision++;
        }
    }

    public void SetLayoutDirty()
    {
        layoutDirty = true;
        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

    public void SetHierachyDirty()
    {
        hierachyDirty = true;
        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

    internal void UnSetHierachyDirty()
    {
        hierachyDirty = false;
    }
}
}
