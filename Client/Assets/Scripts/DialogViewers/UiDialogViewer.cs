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
        void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);
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

        #endregion

        #region nonpublic members

        protected Image m_Background;
        protected RectTransform m_DialogContainer;
        protected readonly Stack<IDialogPanel> PanelStack = new Stack<IDialogPanel>();
        
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
        public UnityAction Action { get; set; }

        public abstract void Init(RectTransform _Parent);
        public abstract void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);
        
        public void CloseAll()
        {
            if (!PanelStack.Any())
                return;
            var lastPanel = PanelStack.Pop();
            var panelsToDestroy = new List<IDialogPanel>();
            while (PanelStack.Count > 0)
                panelsToDestroy.Add(PanelStack.Pop());
            
            foreach (var pan in panelsToDestroy
                .Where(_Panel => _Panel != null))
            {
                Object.Destroy(pan.Panel.gameObject);
            }
            
            PanelStack.Push(lastPanel);
            ShowCore(null, true, true);
        }
        
        public virtual void UpdateTick()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseAll();
        }
        
        #endregion

        #region nonpublic methods

        protected void ShowCore(
            IDialogPanel _ItemTo,
            bool _HidePrevious,
            bool _GoBack)
        {
            var itemFrom = !PanelStack.Any() ? null : PanelStack.Peek();
            if (itemFrom == null && _ItemTo == null)
                return;

            var fromPanel = itemFrom?.Panel;
            var toPanel = _ItemTo?.Panel;
            EUiCategory menuCat = _ItemTo?.Category ?? EUiCategory.MainMenu;
            
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
                        var monobeh = itemFrom as MonoBehaviour;
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
                _ItemTo.OnDialogEnable();
            }

            FinishShowing(itemFrom, _ItemTo, _GoBack, toPanel);
        }

        protected virtual void FinishShowing(
            IDialogPanel _ItemFrom,
            IDialogPanel _ItemTo,
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
            var list = new List<IDialogPanel>();
            while(PanelStack.Any())
                list.Add(PanelStack.Pop());
            foreach (var monobeh in from item in list
                where item != null
                select item as MonoBehaviour)
            {
                Object.Destroy(monobeh);
            }
        }

        #endregion
    }
}