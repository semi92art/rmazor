﻿using System;
using System.Linq;
using Extensions;
using Helpers;
using UI;
using UnityEngine;
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
    public static readonly int AlphaCoeff = Shader.PropertyToID("_AlphaCoeff");
    public RenderTexture Texture { get; set; }

    public AnimationCurve curve;
    public AnimationCurve fastCurve;
    public MeshRenderer transitionRenderer;
    
    private Material m_Material;


    private void Start()
    {
        m_Material = transitionRenderer.sharedMaterial;
        m_Material.SetFloat(AlphaCoeff, -1);
    }
    
    public static CircleTransparentTransitionRenderer Create()
    {
        GameObject instance = GameObject.FindGameObjectWithTag("RenderTransitionCamera");
        if (instance == null)
        {
            instance = PrefabInitializer.InitPrefab(
                null,
                "ui_panel_transition",
                "render_camera"
            );
            instance.transform.position = instance.transform.position.SetX(-50);
        }
        var result = instance.GetComponent<CircleTransparentTransitionRenderer>();
        result.Texture = result.gameObject.GetCompItem<Camera>("camera").targetTexture;
        return result;
    }

    public void StartTransition(bool _Fast = false)
    {
        AnimationCurve ac = _Fast ? fastCurve : curve;
        float startTime = Time.time;
        bool doEvent = false;
        
        Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
        {
            m_Material.SetFloat(AlphaCoeff, -1);
            Coroutines.Run(Coroutines.DoWhile(() =>
                {
                    float dt = Time.time - startTime;
                    m_Material.SetFloat(AlphaCoeff, ac.Evaluate(dt));
                    if (!doEvent && dt > ac.keys.Last().time * 0.5f)
                    {
                        TransitionAction?.Invoke(this, null);
                        doEvent = true;
                    }
                },
                () =>
                {
                    TransitionAction = null;
                    m_Material.SetFloat(AlphaCoeff, ac.keys.Last().value);
                },
                () => Time.time - startTime < ac.keys.Last().time,
                () => true));
        }));
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