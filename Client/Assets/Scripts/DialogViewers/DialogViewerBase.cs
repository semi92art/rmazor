using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Ticker;
using UI;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace DialogViewers
{
    public interface IDialogViewer : IDialogViewerBase
    {
        void AddNotDialogItem(RectTransform _Item, EUiCategory _Categories);
        void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);
        void RemoveNotDialogItem(RectTransform _Item);
        void CloseAll();
    }
    
    public abstract class DialogViewerBase : IDialogViewer, IAction, IUpdateTick
    {
        #region types
        
        protected class GraphicAlphas
        {
            public Dictionary<Graphic, float> Alphas { get; }

            public GraphicAlphas(RectTransform _Item)
            {
                Alphas = _Item.GetComponentsInChildrenEx<Graphic>()
                    .Distinct()
                    .ToDictionary(_El => _El, _El => _El.color.a);
            }
        }
        
        protected class VisibleInCategories
        {
            public EUiCategory Categories { get; }
            public bool IsVisible { get; set; }

            public VisibleInCategories(
                EUiCategory _Categories,
                bool _IsVisible)
            {
                Categories = _Categories;
                IsVisible = _IsVisible;
            }
        }
        
        protected class Panel
        {
            public IDialogPanel DialogPanel { get; }
            public Panel(IDialogPanel _DialogPanel)
            {
                DialogPanel = _DialogPanel;
            }
        }
        
        #endregion

        #region nonpublic members

        protected Image m_Background;
        protected RectTransform m_DialogContainer;
        protected readonly Dictionary<RectTransform, VisibleInCategories> NotDialogs =
            new Dictionary<RectTransform, VisibleInCategories>();
        protected readonly Stack<Panel> PanelStack = new Stack<Panel>();
        
        protected readonly Dictionary<int, GraphicAlphas> GraphicsAlphas = 
            new Dictionary<int, GraphicAlphas>();

        protected virtual float TransitionTime => 0.2f;
        
        #endregion

        #region inject

        protected IManagersGetter Managers { get; }
        protected IUITicker Ticker { get; }
        
        protected DialogViewerBase(IManagersGetter _Managers, IUITicker _Ticker)
        {
            Managers = _Managers;
            Ticker = _Ticker;
            _Ticker.Register(this);
        }
        
        #endregion

        #region api
        
        public RectTransform Container => m_DialogContainer;
        public event UnityAction Initialized;
        public UnityAction Action { get; set; }
        
        public virtual void Init(RectTransform _Parent)
        {
            Initialized?.Invoke();
        }

        public virtual void Back()
        {
            ShowCore(PanelStack.GetItem(1), true, true);
        }

        public abstract void AddNotDialogItem(RectTransform _Item, EUiCategory _Categories);
        public abstract void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);

        public virtual void RemoveNotDialogItem(RectTransform _Item)
        {
            if (!NotDialogs.ContainsKey(_Item)) 
                return;
            NotDialogs.Remove(_Item);
            GraphicsAlphas.Remove(_Item.GetInstanceID());
        }

        public void CloseAll()
        {
            if (!PanelStack.Any())
                return;
            var lastPanel = PanelStack.Pop();
            var panelsToDestroy = new List<Panel>();
            while (PanelStack.Count > 0)
                panelsToDestroy.Add(PanelStack.Pop());
            
            foreach (var pan in panelsToDestroy
                .Where(_Panel => _Panel != null)
                .Select(_Panel => _Panel.DialogPanel))
            {
                Object.Destroy(pan.Panel.gameObject);
            }
            
            PanelStack.Push(lastPanel);
            ShowCore(null, true, true);
        }
        
        public virtual void UpdateTick()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Back();
        }
        
        #endregion

        #region nonpublic methods

        protected void ShowCore(
            Panel _ItemTo,
            bool _HidePrevious,
            bool _GoBack)
        {
            Panel itemFrom = !PanelStack.Any() ? null : PanelStack.Peek();
            if (itemFrom == null && _ItemTo == null)
                return;

            RectTransform fromPanel = itemFrom?.DialogPanel.Panel;
            RectTransform toPanel = _ItemTo?.DialogPanel.Panel;
            EUiCategory menuCat = _ItemTo?.DialogPanel?.Category ?? EUiCategory.MainMenu;


            if (itemFrom != null && fromPanel != null && _HidePrevious)
            {
                int instId = fromPanel.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(fromPanel));
                Coroutines.Run(Coroutines.DoTransparentTransition(
                    fromPanel, GraphicsAlphas[instId].Alphas, TransitionTime,
                    Ticker,
                    true,
                    () =>
                    {
                        if (!_GoBack)
                            return;
                        Object.Destroy(fromPanel.gameObject);
                        var monobeh = itemFrom.DialogPanel as MonoBehaviour;
                        if (monobeh != null)
                            Object.Destroy(monobeh.gameObject);
                    }));
            }

            if (toPanel != null)
            {
                int instId = toPanel.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(toPanel));
                Coroutines.Run(Coroutines.DoTransparentTransition(
                    toPanel, GraphicsAlphas[instId].Alphas, TransitionTime,
                    Ticker,
                    false, 
                    () => m_Background.enabled = true));
                _ItemTo.DialogPanel.OnDialogEnable();
            }

            if (NotDialogs != null && NotDialogs.Any())
            {
                foreach (var item in NotDialogs)
                {
                    bool? show = null;
                    if (!item.Value.IsVisible && (menuCat & item.Value.Categories) != 0)
                        show = true;
                    else if (item.Value.IsVisible && (menuCat & item.Value.Categories) == 0)
                        show = false;
                    if (!show.HasValue) 
                        continue;
                    NotDialogs[item.Key].IsVisible = show.Value;
                    Coroutines.Run(Coroutines.DoTransparentTransition(
                        item.Key,
                        GraphicsAlphas[item.Key.GetInstanceID()].Alphas, 
                        TransitionTime,
                        Ticker,
                        !show.Value,
                        () =>
                        {
                            Action?.Invoke();
                            Action = null;
                        }));
                }
            }

            FinishShowing(itemFrom, _ItemTo, _GoBack, toPanel);
        }

        protected virtual void FinishShowing(
            Panel _ItemFrom,
            Panel _ItemTo,
            bool _GoBack,
            RectTransform _PanelTo)
        {
            m_Background.enabled = m_Background.raycastTarget = !(_PanelTo == null && _GoBack);
            ClearGraphicsAlphas();
        
            if (_PanelTo == null)
                ClearPanelStack();
            else
            {
                if (!PanelStack.Any())
                    PanelStack.Push(_ItemFrom);
                if (PanelStack.Any() && _GoBack)
                    PanelStack.Pop();
                if (!_GoBack)
                    PanelStack.Push(_ItemTo);
            }
        }
        
        protected void ClearGraphicsAlphas()
        {
            foreach (var item in GraphicsAlphas.ToArray())
            {
                if (item.Value.Alphas.All(_A => _A.Key.IsNull()))
                    GraphicsAlphas.Remove(item.Key);
            }
        }
        
        protected void ClearPanelStack()
        {
            var list = new List<Panel>();
            while(PanelStack.Any())
                list.Add(PanelStack.Pop());
            foreach (var monobeh in from item in list
                where item != null
                select item.DialogPanel as MonoBehaviour)
            {
                Object.Destroy(monobeh);
            }
        }

        #endregion
    }
}