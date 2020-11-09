using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Helpers;
using UI;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using Utils;

public interface IDialogViewer
{
    RectTransform DialogContainer { get; }
    void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);
    void Back();
    void AddNotDialogItem(RectTransform _Item, UiCategory _Categories);
    void RemoveNotDialogItem(RectTransform _Item);
    IDialogViewer SetTransitionTime(float _TransitionTime);
}

public class DefaultDialogViewer : MonoBehaviour, IDialogViewer, IActionExecuter
{
    #region types
    
    private class GraphicAlphas
    {
        public Dictionary<Graphic, float> Alphas { get; }

        public GraphicAlphas(RectTransform _Item)
        {
            Alphas = _Item.GetComponentsInChildrenEx<Graphic>()
                .Distinct()
                .ToDictionary(_El => _El, _El => _El.color.a);
        }
    }

    private class PanelAndVisibility
    {
        public IDialogPanel DialogPanel { get; }
        public bool IsHidden { get; set; }

        public PanelAndVisibility(IDialogPanel _DialogPanel)
        {
            DialogPanel = _DialogPanel;
        }
    }

    private class UiCategoriesVis
    {
        public UiCategory Categories { get; }
        public bool IsVisible { get; set; }

        public UiCategoriesVis(UiCategory _Categories, bool _IsVisible)
        {
            Categories = _Categories;
            IsVisible = _IsVisible;
        }
    }
    
    #endregion
    
    #region serialized fields
    
    public Image background;
    public RectTransform dialogContainer;
    
    #endregion
    
    #region private members

    private readonly Dictionary<RectTransform, UiCategoriesVis> m_NotDialogs =
        new Dictionary<RectTransform, UiCategoriesVis>();
    private readonly Dictionary<int, GraphicAlphas> m_GraphicsAlphas = 
        new Dictionary<int, GraphicAlphas>();
    private readonly Stack<PanelAndVisibility> m_PanelStack = new Stack<PanelAndVisibility>();
    private Sprite m_IconBack;
    private Sprite m_IconClose;
    private Button m_CloseButton;
    private Image m_CloseButtonIcon;
    private IDialogPanel m_FromTemp;
    private float m_TransitionTime = 0.2f;
    

    #endregion
    
    #region engine methods

    private void Start()
    {
        GameObject go = PrefabInitializer.InitUiPrefab(
            UiFactory.UiRectTransform(
                gameObject.RTransform(),
                UiAnchor.Create(0.5f, 0.5f, 0.5f, 0.5f),
                new Vector2(0, -418f),
                Vector2.one * 0.5f,
                Vector2.one * 60f),
            "main_menu_buttons", "dialog_close_button");
        m_CloseButton = go.GetCompItem<Button>("button");
        m_CloseButtonIcon = go.GetCompItem<Image>("close_icon");
        m_CloseButton.SetOnClick(Back);
        m_GraphicsAlphas.Add(m_CloseButton.RTransform().GetInstanceID(), new GraphicAlphas(m_CloseButton.RTransform()));
        m_CloseButton.RTransform().gameObject.SetActive(false);

        m_IconBack = PrefabInitializer.GetObject<Sprite>("icons", "icon_back");
        m_IconClose = PrefabInitializer.GetObject<Sprite>("icons", "icon_close");
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Back();
    }
    
    #endregion

    #region api
    
    public Action Action { get; set; }
    public RectTransform DialogContainer => dialogContainer;

    public void Show(IDialogPanel _ItemTo, bool _HidePrevious = true)
    {
        var to = new PanelAndVisibility(_ItemTo);
        ShowCore(to, _HidePrevious, false);
    }

    public void Back()
    {
        ShowCore(m_PanelStack.GetItem(1), true, true);
    }

    public IDialogViewer SetTransitionTime(float _TransitionTime)
    {
        m_TransitionTime = _TransitionTime;
        return this;
    }

    public void AddNotDialogItem(RectTransform _Item, UiCategory _Categories)
    {
        if (m_NotDialogs.ContainsKey(_Item))
            return;
        m_NotDialogs.Add(_Item, new UiCategoriesVis(_Categories, _Item.gameObject.activeSelf));
        m_GraphicsAlphas.Add(_Item.GetInstanceID(), new GraphicAlphas(_Item));
    }

    public void RemoveNotDialogItem(RectTransform _Item)
    {
        if (!m_NotDialogs.ContainsKey(_Item)) 
            return;
        m_NotDialogs.Remove(_Item);
        m_GraphicsAlphas.Remove(_Item.GetInstanceID());
    }

    #endregion

    #region private methods
    
    private void ShowCore(
        PanelAndVisibility _ItemTo, 
        bool _HidePrevious,
        bool _GoBack)
    {
        PanelAndVisibility itemFrom = m_FromTemp != null || !m_PanelStack.Any() ? null : m_PanelStack.Peek();
        m_FromTemp = null;
        
        if (itemFrom == null && _ItemTo == null)
            return;

        var fromPanel = itemFrom?.DialogPanel.Panel;
        var toPanel = _ItemTo?.DialogPanel.Panel;
        
        UiManager.Instance.CurrentCategory = _ItemTo?.DialogPanel?.Category ?? UiCategory.MainMenu;
        var cat = UiManager.Instance.CurrentCategory;
        
        if (itemFrom != null && _HidePrevious)
        {
            int instId = fromPanel.GetInstanceID();
            if (!m_GraphicsAlphas.ContainsKey(instId))
                m_GraphicsAlphas.Add(instId, new GraphicAlphas(fromPanel));
            StartCoroutine(Coroutines.DoTransparentTransition(
                fromPanel, m_GraphicsAlphas[instId].Alphas, m_TransitionTime, true,
                () =>
                {
                    if (!_GoBack)
                        return;
                    Destroy(fromPanel.gameObject);
                    var monobeh = itemFrom.DialogPanel as MonoBehaviour;
                    if (monobeh != null)
                        Destroy(monobeh.gameObject);
                }));
            itemFrom.IsHidden = true;
        }

        if (toPanel != null)
        {
            int instId = toPanel.GetInstanceID();
            if (!m_GraphicsAlphas.ContainsKey(instId))
                m_GraphicsAlphas.Add(instId, new GraphicAlphas(toPanel));
            toPanel.Set(RtrLites.FullFill);
            StartCoroutine(Coroutines.DoTransparentTransition(
                toPanel, m_GraphicsAlphas[instId].Alphas, m_TransitionTime, false, 
                () => background.enabled = true));
        }

        if (m_NotDialogs != null && m_NotDialogs.Any())
        {
            foreach (var item in m_NotDialogs)
            {
                bool? show = null;
                if (!item.Value.IsVisible && (cat & item.Value.Categories) != 0)
                    show = true;
                else if (item.Value.IsVisible && (cat & item.Value.Categories) == 0)
                    show = false;

                if (show.HasValue)
                {
                    m_NotDialogs[item.Key].IsVisible = show.Value;
                    StartCoroutine(Coroutines.DoTransparentTransition(
                        item.Key,
                        m_GraphicsAlphas[item.Key.GetInstanceID()].Alphas, 
                        m_TransitionTime,
                        !show.Value,
                        () =>
                        {
                            Action?.Invoke();
                            Action = null;
                        }));
                }
            }
        }

        if (m_CloseButton != null && (itemFrom == null || toPanel == null))
        {
            StartCoroutine(Coroutines.DoTransparentTransition(m_CloseButton.RTransform(),
                m_GraphicsAlphas[m_CloseButton.RTransform().GetInstanceID()].Alphas, m_TransitionTime, _GoBack));
        }
        
        background.enabled = background.raycastTarget = !(toPanel == null && _GoBack);
        ClearGraphicsAlphas();
        
        if (toPanel == null)
            ClearPanelStack();
        else
        {
            if (!m_PanelStack.Any())
                m_PanelStack.Push(itemFrom);
            if (m_PanelStack.Any() && _GoBack)
                m_PanelStack.Pop();
            if (!_GoBack)
                m_PanelStack.Push(_ItemTo);
        }

        SetCloseButtonIcon();
    }

    private void ClearPanelStack()
    {
        var list = new List<PanelAndVisibility>();
        while(m_PanelStack.Any())
            list.Add(m_PanelStack.Pop());
        foreach (var monobeh in from item in list 
            where item != null select item.DialogPanel as MonoBehaviour)
            Destroy(monobeh);
    }
    
    private void SetCloseButtonIcon()
    {
        if (m_CloseButton == null)
            return;
        m_CloseButtonIcon.sprite = m_PanelStack.GetItem(1) == null ? 
            m_IconClose : m_IconBack;
    }
    
    private void ClearGraphicsAlphas()
    {
        foreach (var item in m_GraphicsAlphas.ToArray())
        {
            if (item.Value.Alphas.All(_A => !_A.Key.IsAlive()))
                m_GraphicsAlphas.Remove(item.Key);
        }
    }

    #endregion
}


