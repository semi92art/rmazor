using System;
using Extensions;
using GameHelpers;
using UI.Entities;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class PrototypingUiDebugPanel : MonoBehaviour
    {
        private const string ContentPanelName = "Prototyping UI Debug Content";
        
        [SerializeField] private GameObject panelPrefab;
        [SerializeField] private GameObject buttonPrefab;
        [Space, Header("Next button:")]
        [SerializeField] private string title;
        [SerializeField] private Button.ButtonClickedEvent @event;

        [HideInInspector] [SerializeField] private RectTransform content;

        private void Start()
        {
            Application.targetFrameRate = 120;
            GameSettings.PlayMode = true;
        }

        public void AddButton()
        {
            content = GetContentPanel();
            var clone = Instantiate(buttonPrefab, content.transform);
            var prot = clone.GetComponent<PrototypingUiDebugItem>();
            prot.Init(title, @event);
        }

        public void ShowPanel()
        {
            content = GetContentPanel();
            content.SetGoActive(!content.gameObject.activeSelf);
        }

        private RectTransform GetContentPanel()
        {
            if (content != null)
                return content;
            
            var go = GameObject.Find(ContentPanelName);
            if (go != null)
                return go.RTransform();
            
            go = Instantiate(panelPrefab);
            go.name = ContentPanelName;
            var rTr = go.RTransform();
            rTr.SetParent(transform.parent);
            rTr.localScale = Vector3.one;
            rTr.Set(
                UiAnchor.Create(0, 0, 1, 1), 
                Vector2.zero,
                Vector2.one * 0.5f, 
                Vector2.zero);
            return rTr;
        }
    }
}