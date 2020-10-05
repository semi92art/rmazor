using System;
using System.Linq;
using TMPro;
using UICreationSystem;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadingPanel : MonoBehaviour
{
    #region serialized fields
    
    public Image indicator;
    public Image indicator2;
    public Animator animator;
    public TextMeshProUGUI loading;
    public float speed = 50f;
    
    #endregion

    #region public properties

    public bool DoLoading
    {
        get => m_DoLoading;
        set
        {
            indicator.enabled = value;
            indicator2.enabled = value;
            animator.enabled = value;
            m_DoLoading = value;
        }
    }
    
    #endregion
    
    #region private fields
    
    private Transform m_Indicator;
    private Transform m_Indicator2;
    private string m_LoadingText = "Loading";
    private int m_PointsState;
    private int m_TimePrev;
    private bool m_DoLoading;
    private bool m_IsConnectionProblem;

    #endregion

    #region engine methods

    private void Start()
    {
        m_Indicator = indicator.transform;
        m_Indicator2 = indicator2.transform;
        DoLoading = true;
    }
    
    private void Update()
    {
        IndicateLoading();
    }

    #endregion

    #region factory methods
    
    public static LoadingPanel Create(
        RectTransform _Parent, 
        string _Name,
        UiAnchor _Anchor,
        Vector2 _AnchoredPosition,
        Vector2 _Pivot,
        Vector2 _SizeDelta)
    {
        RectTransform rTr = UiFactory.UiRectTransform(
            _Parent, _Name, _Anchor, _AnchoredPosition, _Pivot, _SizeDelta);

        GameObject prefab = PrefabInitializer.InitUiPrefab(rTr, "loading_panel", "loading_panel");
        
        return prefab.GetComponent<LoadingPanel>();
    }
    
    #endregion

    #region private methods

    private void IndicateLoading()
    {
        if (!DoLoading)
            return;
        
        m_Indicator.Rotate(Vector3.back, Time.deltaTime * speed);
        m_Indicator2.Rotate(Vector3.forward, Time.deltaTime * speed);

        int time = Mathf.FloorToInt(Time.time * 5f);
        if (time % 2 == 0 &&  time != m_TimePrev)
        {
            CommonUtils.IncWithOverflow(ref m_PointsState, 4);
            loading.text = m_LoadingText + string.Concat(Enumerable.Repeat(".", m_PointsState));
        }

        m_TimePrev = time;
    }
    
    #endregion
}

#region Editor

#if UNITY_EDITOR

[CustomEditor(typeof(LoadingPanel))]
public class LoadingPanelEditor : Editor
{
    private LoadingPanel m_LoadingPanel;

    private void OnEnable()
    {
        m_LoadingPanel = target as LoadingPanel;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        string buttonText = m_LoadingPanel.DoLoading ? "Disable loading" : "Enable loading";
        if (GUILayout.Button(buttonText))
            m_LoadingPanel.DoLoading = !m_LoadingPanel.DoLoading;
    }
}

#endif

#endregion
