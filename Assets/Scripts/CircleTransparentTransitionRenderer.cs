using System;
using System.Linq;
using Extentions;
using UnityEngine;
using UICreationSystem;
using UICreationSystem.Factories;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface ITransitionRenderer
{
    void StartTransition(bool _Fast = false);
    EventHandler TransitionAction { set; }
    RenderTexture Texture { get; }
}

public class CircleTransparentTransitionRenderer : MonoBehaviour, ITransitionRenderer
{
    public EventHandler TransitionAction { get; set; }
    public RenderTexture Texture { get; set; }

    public AnimationCurve curve;
    public AnimationCurve fastCurve;
    public MeshRenderer transitionRenderer;
    
    private Material m_Material;
    
    
    private void Start()
    {
        m_Material = transitionRenderer.sharedMaterial;
    }
    
    public static CircleTransparentTransitionRenderer Create()
    {
        GameObject instance = GameObject.FindGameObjectWithTag("RenderTransitionCamera");
        if (instance == null)
        {
            instance = PrefabInitializer.InitPrefab(
                new GameObject().transform, 
                "ui_panel_transition",
                "render_camera"
            );
        }
        var result = instance.GetComponent<CircleTransparentTransitionRenderer>();
        result.Texture = result.gameObject.GetComponentItem<Camera>("camera").targetTexture;
        return result;
    }

    public void StartTransition(bool _Fast = false)
    {
        AnimationCurve ac = _Fast ? fastCurve : curve;
        
        float startTime = Time.time;
        bool doEvent = false;

        StartCoroutine(Coroutines.DoWhile(() =>
            {
                float dt = Time.time - startTime;
                m_Material.SetFloat("_AlphaCoeff", ac.Evaluate(dt));
                if (!doEvent && dt > ac.keys.Last().time * 0.5f)
                {
                    TransitionAction?.Invoke(this, null);
                    doEvent = true;
                }
            },
            () =>
            {
                TransitionAction = null;
                m_Material.SetFloat("_AlphaCoeff", ac.keys.Last().value);
            },
            () => Time.time - startTime < ac.keys.Last().time,
            () => true));
    }
}

#region Editor

#if UNITY_EDITOR
[CustomEditor(typeof(CircleTransparentTransitionRenderer))]
public class CircleTransparentTransitionPanelEditor : Editor
{
    private CircleTransparentTransitionRenderer o;

    private void OnEnable()
    {
        o = target as CircleTransparentTransitionRenderer;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Start Transition"))
            o.StartTransition();
    }
}
#endif

#endregion