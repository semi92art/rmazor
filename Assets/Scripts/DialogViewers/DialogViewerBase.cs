using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Extensions;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace DialogViewers
{
    public abstract class DialogViewerBase : MonoBehaviour, IDialogViewer, IActionExecutor
    {
        #region constants

        protected const int MenuUiCategoryType = 0;
        protected const int GameUiCategoryType = 1;
        
        #endregion
        
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
            public MenuUiCategory MenuCategories { get; }
            public GameUiCategory GameCategories { get; }
            public bool IsVisible { get; set; }

            public VisibleInCategories(
                MenuUiCategory _MenuCategories,
                bool _IsVisible)
            {
                MenuCategories = _MenuCategories;
                IsVisible = _IsVisible;
            }
            
            public VisibleInCategories(
                GameUiCategory _GameCategories,
                bool _IsVisible)
            {
                GameCategories = _GameCategories;
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
        
        #region serialized fields
    
        [SerializeField] protected Image background;
        [SerializeField] protected RectTransform dialogContainer;
    
        #endregion
        
        #region nonpublic members
        
        protected readonly Dictionary<RectTransform, VisibleInCategories> NotDialogs =
            new Dictionary<RectTransform, VisibleInCategories>();
        protected readonly Stack<Panel> PanelStack = new Stack<Panel>();
        
        protected readonly Dictionary<int, GraphicAlphas> GraphicsAlphas = 
            new Dictionary<int, GraphicAlphas>();

        protected virtual float TransitionTime => 0.2f;
        
        #endregion

        #region api
        
        public RectTransform Container => dialogContainer;
        public virtual void Back()
        {
            int uiCategoryType = MenuUiCategoryType;
            if (this as GameDialogViewer != null)
                uiCategoryType = GameUiCategoryType;
            ShowCore(PanelStack.GetItem(1), true, true, uiCategoryType);
        }

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
            int uiCategoryType = lastPanel.DialogPanel is IMenuUiCategory ? MenuUiCategoryType : GameUiCategoryType;
            var panelsToDestroy = new List<Panel>();
            while (PanelStack.Count > 0)
                panelsToDestroy.Add(PanelStack.Pop());
            
            foreach (var pan in panelsToDestroy
                .Where(_Panel => _Panel != null)
                .Select(_Panel => _Panel.DialogPanel))
            {
                Destroy(pan.Panel.gameObject);
            }
            
            PanelStack.Push(lastPanel);
            ShowCore(null, true, true, uiCategoryType);
        }

        public UnityAction Action { get; set; }
        
        #endregion
        
        #region engine methods
        
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Back();
        }
        
        #endregion

        protected void ShowCore(
            Panel _ItemTo,
            bool _HidePrevious,
            bool _GoBack,
            int _UiCategoryType)
        {
            Panel itemFrom = !PanelStack.Any() ? null : PanelStack.Peek();
            if (itemFrom == null && _ItemTo == null)
                return;

            RectTransform fromPanel = itemFrom?.DialogPanel.Panel;
            RectTransform toPanel = _ItemTo?.DialogPanel.Panel;
            var menuCat = MenuUiCategory.Nothing;
            var gameCat = GameUiCategory.Nothing;
            switch (_UiCategoryType)
            {
                case MenuUiCategoryType:
                    UiManager.Instance.CurrentMenuCategory = menuCat = 
                        ((IMenuUiCategory)_ItemTo?.DialogPanel)?.Category ?? MenuUiCategory.MainMenu;
                    break;
                case GameUiCategoryType:
                    UiManager.Instance.CurrentGameCategory = gameCat =
                        ((IGameUiCategory)_ItemTo?.DialogPanel)?.Category ?? GameUiCategory.Game;
                    break;
                default:
                    throw new InvalidEnumArgumentExceptionEx(_UiCategoryType);
            }

            if (itemFrom != null && fromPanel != null && _HidePrevious)
            {
                int instId = fromPanel.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(fromPanel));
                Coroutines.Run(Coroutines.DoTransparentTransition(
                    fromPanel, GraphicsAlphas[instId].Alphas, TransitionTime, true,
                    () =>
                    {
                        if (!_GoBack)
                            return;
                        Destroy(fromPanel.gameObject);
                        var monobeh = itemFrom.DialogPanel as MonoBehaviour;
                        if (monobeh != null)
                            Destroy(monobeh.gameObject);
                    }, true));
            }

            if (toPanel != null)
            {
                int instId = toPanel.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(toPanel));
                StartCoroutine(Coroutines.DoTransparentTransition(
                    toPanel, GraphicsAlphas[instId].Alphas, TransitionTime, false, 
                    () => background.enabled = true, true));
                _ItemTo.DialogPanel.OnDialogEnable();
            }

            if (NotDialogs != null && NotDialogs.Any())
            {
                foreach (var item in NotDialogs)
                {
                    bool? show = null;
                    if (_UiCategoryType == MenuUiCategoryType)
                    {
                        if (!item.Value.IsVisible && (menuCat & item.Value.MenuCategories) != 0)
                            show = true;
                        else if (item.Value.IsVisible && (menuCat & item.Value.MenuCategories) == 0)
                            show = false;
                    }
                    else if (_UiCategoryType == GameUiCategoryType)
                    {
                        if (!item.Value.IsVisible && (gameCat & item.Value.GameCategories) != 0)
                            show = true;
                        else if (item.Value.IsVisible && (gameCat & item.Value.GameCategories) == 0)
                            show = false;
                    }

                    if (!show.HasValue) 
                        continue;
                    NotDialogs[item.Key].IsVisible = show.Value;
                    Coroutines.Run(Coroutines.DoTransparentTransition(
                        item.Key,
                        GraphicsAlphas[item.Key.GetInstanceID()].Alphas, 
                        TransitionTime,
                        !show.Value,
                        () =>
                        {
                            Action?.Invoke();
                            Action = null;
                        }, true));
                }
            }

            FinishShowing(itemFrom, _ItemTo, _GoBack, toPanel, _UiCategoryType);
        }

        protected virtual void FinishShowing(
            Panel _ItemFrom,
            Panel _ItemTo,
            bool _GoBack,
            RectTransform _PanelTo,
            int _UiCategoryType)
        {
            background.enabled = background.raycastTarget = !(_PanelTo == null && _GoBack);
            ClearGraphicsAlphas();
        
            if (_PanelTo == null)
                ClearPanelStack(_UiCategoryType);
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
                if (item.Value.Alphas.All(_A => !_A.Key.IsAlive()))
                    GraphicsAlphas.Remove(item.Key);
            }
        }
        
        protected void ClearPanelStack(int _UiCategoryType)
        {
            var list = new List<Panel>();
            while(PanelStack.Any())
                list.Add(PanelStack.Pop());
            foreach (var monobeh in from item in list
                where item != null
                select item.DialogPanel as MonoBehaviour)
            {
                Destroy(monobeh);
            }
        }
    }
}