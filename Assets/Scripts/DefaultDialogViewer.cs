using System;
using System.Collections.Generic;
using System.Linq;
using Extentions;
using Network;
using UICreationSystem;
using UICreationSystem.Factories;
using UICreationSystem.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Object = UnityEngine.Object;

public interface IDialogViewer
{
    RectTransform DialogContainer { get; }
    void Show(IDialogPanel _ItemTo, bool _IsGoBack = false, float _TransitionTime = 0.2f);
    void AddNotDialogItem(RectTransform _Item, UiCategory _Categories);
    void RemoveNotDialogItem(RectTransform _Item);
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

    private Sprite m_IconBack;
    private Sprite m_IconClose;
    private Dictionary<RectTransform, UiCategoriesVis> m_NotDialogs = new Dictionary<RectTransform, UiCategoriesVis>();
    private Button m_CloseButton;
    private Image m_CloseButtonIcon;
    private IDialogPanel m_FromTemp;
    private Dictionary<int, GraphicAlphas> m_GraphicsAlphas = 
        new Dictionary<int, GraphicAlphas>();
    private Stack<IDialogPanel> m_PanelStack = new Stack<IDialogPanel>();
    private Stack<RectTransform> m_NotDialogsToRemove = new Stack<RectTransform>();

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
        m_CloseButton.SetOnClick(GoBack);
        m_GraphicsAlphas.Add(m_CloseButton.RTransform().GetInstanceID(), new GraphicAlphas(m_CloseButton.RTransform()));
        m_CloseButton.RTransform().gameObject.SetActive(false);

        m_IconBack = PrefabInitializer.GetObject<Sprite>("icons", "icon_back");
        m_IconClose = PrefabInitializer.GetObject<Sprite>("icons", "icon_close");
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            GoBack();
    }
    
    #endregion

    #region api
    
    public Action Action { get; set; }
    public RectTransform DialogContainer => dialogContainer;
    
    public void Show(
        IDialogPanel _ItemTo, 
        bool _IsGoBack = false,
        float _TransitionTime = 0.2f)
    {
        IDialogPanel itemFrom;
        if (m_FromTemp != null)
        {
            itemFrom = m_FromTemp;
            m_FromTemp = null;
        }
        else
        {
            itemFrom = m_PanelStack.Any() ? m_PanelStack.Peek() : null;    
        }
        
        if (itemFrom == null && _ItemTo == null)
            return;
        
        UiManager.Instance.CurrentCategory = _ItemTo?.Category ?? UiCategory.MainMenu;
        var cat = UiManager.Instance.CurrentCategory;
        
        if (itemFrom != null)
        {
            int instId = itemFrom.Panel.GetInstanceID();
            if (!m_GraphicsAlphas.ContainsKey(instId))
                m_GraphicsAlphas.Add(instId, new GraphicAlphas(itemFrom.Panel));
            itemFrom.Panel.Set(RtrLites.FullFill);
            StartCoroutine(Coroutines.DoTransparentTransition(
                itemFrom.Panel, m_GraphicsAlphas[instId].Alphas, _TransitionTime, true,
                () =>
                {
                    if (_IsGoBack)
                    {
                        Destroy(itemFrom.Panel.gameObject);
                        var monobeh = itemFrom as MonoBehaviour;
                        if (monobeh != null)
                            Destroy(monobeh.gameObject);
                    }
                })); 
        }

        if (_ItemTo != null)
        {
            int instId = _ItemTo.Panel.GetInstanceID();
            if (!m_GraphicsAlphas.ContainsKey(instId))
                m_GraphicsAlphas.Add(instId, new GraphicAlphas(_ItemTo.Panel));
            _ItemTo.Panel.Set(RtrLites.FullFill);
            StartCoroutine(Coroutines.DoTransparentTransition(
                _ItemTo.Panel, m_GraphicsAlphas[instId].Alphas, _TransitionTime, false, 
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
                        _TransitionTime,
                        !show.Value,
                        () =>
                        {
                            Action?.Invoke();
                            Action = null;
                        }));
                }
            }
        }

        if (m_CloseButton != null && (itemFrom == null || _ItemTo == null))
        {
            StartCoroutine(Coroutines.DoTransparentTransition(m_CloseButton.RTransform(),
                m_GraphicsAlphas[m_CloseButton.RTransform().GetInstanceID()].Alphas, _TransitionTime, _IsGoBack));
        }
            
        
        background.enabled = background.raycastTarget = !(_ItemTo == null && _IsGoBack);
        ClearGraphicsAlphas();

        if (_ItemTo == null)
            ClearPanelStack();
        else
        {
            if (!m_PanelStack.Any())
                m_PanelStack.Push(itemFrom);
            m_PanelStack.Push(_ItemTo);
        }

        SetCloseButtonIcon();
    }

    public void AddNotDialogItem(RectTransform _Item, UiCategory _Categories)
    {
        if (!m_NotDialogs.ContainsKey(_Item))
        {
            m_NotDialogs.Add(_Item, new UiCategoriesVis(_Categories, _Item.gameObject.activeSelf));
            m_GraphicsAlphas.Add(_Item.GetInstanceID(), new GraphicAlphas(_Item));
        }
    }

    public void RemoveNotDialogItem(RectTransform _Item)
    {
        if (m_NotDialogs.ContainsKey(_Item))
        {
            m_NotDialogs.Remove(_Item);
            m_GraphicsAlphas.Remove(_Item.GetInstanceID());
        }
    }

    #endregion

    #region private methods

    private void ClearPanelStack()
    {
        var list = new List<IDialogPanel>();
        while(m_PanelStack.Any())
            list.Add(m_PanelStack.Pop());
        foreach (var item in list)
        {
            var monobeh = item as MonoBehaviour;
            if (monobeh != null)
                Destroy(monobeh);
        }
    }
    
    private void SetCloseButtonIcon()
    {
        IDialogPanel item1, item2 = null;
        if (m_PanelStack.Any())
        {
            item1 = m_PanelStack.Pop();
            if (m_PanelStack.Any())
                item2 = m_PanelStack.Peek();
            m_PanelStack.Push(item1);
        }
        
        if (m_CloseButton != null)
            m_CloseButtonIcon.sprite = item2 == null ? m_IconClose : m_IconBack;
    }
    
    private void ClearGraphicsAlphas()
    {
        foreach (var item in m_GraphicsAlphas.ToArray())
        {
            if (item.Value.Alphas.All(_A => !_A.Key.IsAlive()))
                m_GraphicsAlphas.Remove(item.Key);
        }
    }
    
    private void GoBack()
    {
        if (m_PanelStack.Any())
        {
            m_FromTemp = m_PanelStack.Pop();
            Show(m_PanelStack.Pop(), true);
        }
    }
    
    #endregion
}


