using System;
using System.Collections.Generic;
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

public interface ILoadingPanel : IDialogItem
{
    bool DoLoading { get; set; }
}

public class LoadingPanel : MonoBehaviour, ILoadingPanel
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
    
    private IDialogViewer m_DialogViewer;
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
    
    public static ILoadingPanel Create(
        IDialogViewer _DialogViewer)
    {
        RectTransform rTr = UiFactory.UiRectTransform(
            _DialogViewer.DialogContainer, "Loading Panel", RtrLites.FullFill);
        GameObject prefab = PrefabInitializer.InitUiPrefab(rTr, "loading_panel", "loading_panel");
        LoadingPanel lp = prefab.GetComponent<LoadingPanel>();
        lp.gameObject.SetActive(false);
        lp.m_DialogViewer = _DialogViewer;
        return lp;
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
    
    #region public methods

    public void Show(RectTransform _FromItem = null)
    {
        m_DialogViewer.Show(_FromItem, gameObject.RTransform());
    }

    public void Hide(RectTransform _ToItem = null)
    {
        m_DialogViewer.Show(gameObject.RTransform(), _ToItem, 0.1f, true);
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
