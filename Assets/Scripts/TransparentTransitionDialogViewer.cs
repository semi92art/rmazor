using System;
using System.Collections.Generic;
using System.Linq;
using UICreationSystem;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public interface IDialogItem
{
    void Show(RectTransform _FromItem = null);
    void Hide(RectTransform _ToItem = null);
}

public interface IDialogViewer
{
    RectTransform DialogContainer { get; }
    void Show(RectTransform _ItemFrom, RectTransform _ItemTo, float _TransitionTime = 0.5f, bool _IsGoBack = false);
    void SetNotDialogItems(RectTransform[] _Items);
}

public class TransparentTransitionDialogViewer : MonoBehaviour, IDialogViewer
{
    public Image background;
    public RectTransform dialogContainer;

    public RectTransform DialogContainer => dialogContainer;
    private RectTransform[]  m_NotDialogs;
    
    Dictionary<int, GraphicAlphas> m_GraphicsAlphas = new Dictionary<int, GraphicAlphas>();

    public void Show(
        RectTransform _ItemFrom,
        RectTransform _ItemTo, 
        float _TransitionTime = 0.5f,
        bool _IsGoBack = false)
    {
        if (_ItemFrom == null && _ItemTo == null)
            return;
        
        if (_ItemFrom != null)
        {
            int instId = _ItemFrom.GetInstanceID();
            if (!m_GraphicsAlphas.ContainsKey(instId))
                m_GraphicsAlphas.Add(instId, new GraphicAlphas(_ItemFrom));
            _ItemFrom.Set(RtrLites.FullFill);
            StartCoroutine(Coroutines.DoTransparentTransition(
                _ItemFrom, m_GraphicsAlphas[instId].Alphas, _TransitionTime, true,
                () =>
                {
                    if (_IsGoBack)
                        Destroy(_ItemFrom.gameObject);
                })); 
        }

        if (_ItemTo != null)
        {
            int instId = _ItemTo.GetInstanceID();
            if (!m_GraphicsAlphas.ContainsKey(instId))
                m_GraphicsAlphas.Add(instId, new GraphicAlphas(_ItemTo));
            _ItemTo.Set(RtrLites.FullFill);
            StartCoroutine(Coroutines.DoTransparentTransition(
                _ItemTo, m_GraphicsAlphas[instId].Alphas, _TransitionTime, false, 
                () => background.enabled = true));
        }

        if (m_NotDialogs != null && m_NotDialogs.Any())
        {
            if (_ItemFrom == null && !_IsGoBack || _ItemTo == null && _IsGoBack)
                foreach (var item in m_NotDialogs)
                    StartCoroutine(Coroutines.DoTransparentTransition(
                        item, m_GraphicsAlphas[item.GetInstanceID()].Alphas, _TransitionTime, !_IsGoBack));
        }
        
        background.enabled = background.raycastTarget = !(_ItemTo == null && _IsGoBack);
        ClearGraphicsAlphas();
    }

    public void SetNotDialogItems(RectTransform[]  _Items)
    {
        m_NotDialogs = _Items;
        foreach (var item in _Items)
        {
            if (!m_GraphicsAlphas.ContainsKey(item.GetInstanceID()))
                m_GraphicsAlphas.Add(item.GetInstanceID(), new GraphicAlphas(item));
        }
    }

    private void ClearGraphicsAlphas()
    {
        foreach (var item in m_GraphicsAlphas.ToArray())
        {
            if (item.Value.Alphas.All(_A => !_A.Key.IsAlive()))
                m_GraphicsAlphas.Remove(item.Key);
        }
    }
}

public class GraphicAlphas
{
    public Dictionary<Graphic, float> Alphas { get; }

    public GraphicAlphas(RectTransform _Item)
    {
        Alphas = _Item.GetComponentsInChildrenEx<Graphic>()
             .Distinct()
             .ToDictionary(_El => _El, _El => _El.color.a);
    }
}
