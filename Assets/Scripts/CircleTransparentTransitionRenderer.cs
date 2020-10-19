using System;
using System.Linq;
using UnityEngine;
using UICreationSystem;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface ITransitionRenderer
{
    void StartTransition(bool _Fast = false);
    EventHandler TransitionAction { set; }
    RectTransform TransitionPanel { get; set; }
}

public class CircleTransparentTransitionRenderer : MonoBehaviour, ITransitionRenderer
{
    public EventHandler TransitionAction { get; set; }
    public RectTransform TransitionPanel { get; set; }
    
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

        return instance.GetComponent<CircleTransparentTransitionRenderer>();
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