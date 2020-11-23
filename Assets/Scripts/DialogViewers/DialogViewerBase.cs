using System.Collections.Generic;
using System.Linq;
using Extensions;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace DialogViewers
{
    public abstract class DialogViewerBase : MonoBehaviour, IDialogViewer, IActionExecuter
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
            public IMenuDialogPanel MenuDialogPanel { get; }
            public IGameDialogPanel GameDialogPanel { get; }

            public Panel(IMenuDialogPanel _DialogPanel)
            {
                MenuDialogPanel = _DialogPanel;
            }
            
            public Panel(IGameDialogPanel _DialogPanel)
            {
                GameDialogPanel = _DialogPanel;
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
        protected abstract float TransitionTime { get; }
        
        #endregion

        #region api
        
        public RectTransform DialogContainer => dialogContainer;
        public virtual void Back()
        {
            int uiCategoryType = MenuUiCategoryType;
            if (this as GameDialogViewer != null)
                uiCategoryType = GameUiCategoryType;
            ShowCore(PanelStack.GetItem(1), true, true, uiCategoryType);
        }

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
            int uiCategoryType = lastPanel.GameDialogPanel != null ? GameUiCategoryType : MenuUiCategoryType;
            var panelsToDestroy = new List<Panel>();
            while (PanelStack.Count > 0)
                panelsToDestroy.Add(PanelStack.Pop());
            
            foreach (var pan in panelsToDestroy
                .Where(_Panel => _Panel != null)
                .Select(_Panel => (IDialogPanel)_Panel.GameDialogPanel ?? _Panel.MenuDialogPanel)
                .Where(_Panel => _Panel != null))
            {
                Destroy(pan.Panel.gameObject);
            }
            
            ShowCore(null, true, true, uiCategoryType);
        }

        public System.Action Action { get; set; }
        
        #endregion
        
        #region engine methods
        
        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Back();
        }
        
        #endregion

        protected virtual void ShowCore(
            Panel _ItemTo,
            bool _HidePrevious,
            bool _GoBack,
            int _UiCategoryType)
        {
            Panel itemFrom = !PanelStack.Any() ? null : PanelStack.Peek();
   
            if (itemFrom == null && _ItemTo == null)
                return;
            
            RectTransform fromPanel = null;
            RectTransform toPanel = null;
            MenuUiCategory menuCat = MenuUiCategory.Nothing;
            GameUiCategory gameCat = GameUiCategory.Nothing;
            if (_UiCategoryType == MenuUiCategoryType)
            {
                fromPanel = itemFrom?.MenuDialogPanel.Panel;
                toPanel = _ItemTo?.MenuDialogPanel.Panel;
                UiManager.Instance.CurrentMenuCategory = menuCat = 
                    _ItemTo?.MenuDialogPanel?.Category ?? MenuUiCategory.MainMenu;
            }
            else if (_UiCategoryType == GameUiCategoryType)
            {
                fromPanel = itemFrom?.GameDialogPanel.Panel;
                toPanel = _ItemTo?.GameDialogPanel.Panel;
                UiManager.Instance.CurrentGameCategory = gameCat =
                    _ItemTo?.GameDialogPanel?.Category ?? GameUiCategory.Game;
            }

            if (itemFrom != null && fromPanel != null && _HidePrevious)
            {
                int instId = fromPanel.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(fromPanel));
                StartCoroutine(Coroutines.DoTransparentTransition(
                    fromPanel, GraphicsAlphas[instId].Alphas, TransitionTime, true,
                    () =>
                    {
                        if (!_GoBack)
                            return;
                        Destroy(fromPanel.gameObject);
                        var monobeh = itemFrom.MenuDialogPanel as MonoBehaviour;
                        if (monobeh != null)
                            Destroy(monobeh.gameObject);
                    }));
            }

            if (toPanel != null)
            {
                int instId = toPanel.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(toPanel));
                StartCoroutine(Coroutines.DoTransparentTransition(
                    toPanel, GraphicsAlphas[instId].Alphas, TransitionTime, false, 
                    () => background.enabled = true));
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
                    
                    if (show.HasValue)
                    {
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
                            }));
                    }
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
                select
                    (_UiCategoryType == MenuUiCategoryType
                        ? (IDialogPanel) item.MenuDialogPanel
                        : (IDialogPanel) item.GameDialogPanel) as MonoBehaviour)
            {
                Destroy(monobeh);
            }
        }
    }
}