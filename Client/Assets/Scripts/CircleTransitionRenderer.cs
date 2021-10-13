using System;
using System.Linq;
using Constants;
using DI.Extensions;
using GameHelpers;
using Ticker;
using UI;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public interface ITransitionRenderer
{
    void Init(RectTransform _Parent);
    void StartTransition(bool _Fast = false);
    EventHandler TransitionAction { set; }
    RenderTexture Texture { get; }
}

public class CircleTransitionRenderer : ITransitionRenderer
{

    #region nonpublic members
    
    private static readonly int AlphaCoeff = Shader.PropertyToID("_AlphaCoeff");
    private static readonly int Tint = Shader.PropertyToID("_Color");
    private AnimationCurvesScriptableObject m_CurvesObject;
    private AnimationCurve m_NormalCurve;
    private AnimationCurve m_FastCurve;
    private MeshRenderer m_Renderer;
    private Material m_Material;
    
    #endregion

    #region inject
    
    private IUITicker Ticker { get; }

    public CircleTransitionRenderer(IUITicker _Ticker)
    {
        Ticker = _Ticker;
    }

    #endregion
    
    public EventHandler TransitionAction { get; set; }
    public RenderTexture Texture { get; set; }

    public void Init(RectTransform _Parent)
    {
        string name = "Transparent Transition Renderer Camera";
        var cameras = GameObject.FindGameObjectsWithTag("TextureCamera");
        GameObject instance = cameras.FirstOrDefault(_Obj => _Obj.name == name);
        if (instance == null)
        {
            instance = PrefabUtilsEx.InitPrefab(
                null,
                "ui_panel_transition",
                "render_camera"
            );
            instance.name = name;
            instance.transform.position = instance.transform.position.SetX(-50);
        }
        Texture = instance.GetCompItem<Camera>("camera").targetTexture;
        m_Renderer = instance.GetCompItem<MeshRenderer>("renderer");
        m_CurvesObject = PrefabUtilsEx.GetObject<AnimationCurvesScriptableObject>(
            "other", "animation_curves");
        m_NormalCurve = m_CurvesObject.GetCurve("normal_curve");
        m_FastCurve = m_CurvesObject.GetCurve("fast_curve");
        
        var transPanel = PrefabUtilsEx.InitUiPrefab(
            UiFactory.UiRectTransform(_Parent, RtrLites.FullFill),
            "ui_panel_transition", "transition_panel");

        var rImage = transPanel.GetCompItem<RawImage>("raw_image");
        rImage.texture = Texture;
        
        Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
        {
            m_Material = m_Renderer.sharedMaterial;
            m_Material.SetFloat(AlphaCoeff, -1);
            var backColor = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiMainBackground);
            m_Material.SetColor(Tint, backColor);  
        }));
    }

    public void StartTransition(bool _Fast = false)
    {
        AnimationCurve ac = _Fast ? m_FastCurve : m_NormalCurve;
        float startTime = Ticker.Time;
        bool doEvent = false;
        
        Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
        {
            m_Material.SetFloat(AlphaCoeff, -1);
            Coroutines.Run(Coroutines.DoWhile(
                () => Ticker.Time - startTime < ac.keys.Last().time,
                () =>
                {
                    float dt = Ticker.Time - startTime;
                    m_Material.SetFloat(AlphaCoeff, ac.Evaluate(dt));
                    if (doEvent || !(dt > ac.keys.Last().time * 0.5f)) 
                        return;
                    TransitionAction?.Invoke(this, null);
                    doEvent = true;
                },
                () =>
                {
                    TransitionAction = null;
                    m_Material.SetFloat(AlphaCoeff, ac.keys.Last().value);
                }));
        }));
    }
}
