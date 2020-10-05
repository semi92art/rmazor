using System;
using System.Linq;
using UnityEngine;
using UICreationSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface ITransitionRenderer
{
    void StartTransition();
    EventHandler OnTransitionMoment { get; set; }
}

public class CircleTransparentTransitionRenderer : MonoBehaviour, ITransitionRenderer
{
    public EventHandler OnTransitionMoment { get; set; }
    
    public AnimationCurve curve;
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
    
    public void StartTransition()
    {
        float startTime = Time.time;
        bool doEvent = false;

        StartCoroutine(Coroutines.DoWhile(() =>
            {
                float dt = Time.time - startTime;
                m_Material.SetFloat("_AlphaCoeff", curve.Evaluate(dt));
                if (!doEvent && dt > curve.keys.Last().time * 0.5f)
                {
                    OnTransitionMoment?.Invoke(this, null);
                    doEvent = true;
                }
            },
            () =>
            {
                OnTransitionMoment = null;
                m_Material.SetFloat("_AlphaCoeff", curve.keys.Last().value);
            },
            () => Time.time - startTime < curve.keys.Last().time,
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