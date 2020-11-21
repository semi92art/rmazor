using Boo.Lang;
using Constants;
using DialogViewers;
using Extensions;
using Helpers;
using TMPro;
using UI.Entities;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public interface ILifesPanel : IMiniPanel
    {
        void SetLifes(long _Count);
    }
    
    public class LifesPanel : ILifesPanel
    {
        #region protected members
        
        protected readonly IGameDialogViewer DialogViewer;
        protected readonly RectTransform Panel;
        protected readonly Image LifeIcon;
        protected readonly TextMeshProUGUI LifesCountText;
        protected readonly Animator LifeBrokenAnimator;
        protected readonly Sprite LifeEnabled;
        protected readonly Sprite LifeDisabled;
        
        protected long LifesCount
        {
            get => m_LifesCount;
            set
            {
                long diff = m_LifesCount - value; 
                m_LifesCount = value;
                SetLifesCountTextAndIcon();
                SetPanelWidth();
                if (diff <= 0)
                    return;
                AnimateBrokenLifes(diff);
            }
        }
        
        #endregion
        
        #region private members
        
        private long m_LifesCount;
        private int AkBrokenLife => AnimKeys.Anim;
        
        #endregion
        
        #region constructor
        
        protected LifesPanel(RectTransform _Parent, IGameDialogViewer _DialogViewer)
        {
            DialogViewer = _DialogViewer;
            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    UiAnchor.Create(1, 1, 1, 1),
                    Vector2.one * 4f, 
                    Vector2.one,
                    new Vector2(112f, 59.2f)),
                "game_menu", "lifes_panel");
            Panel = go.RTransform();
            LifeIcon = go.GetCompItem<Image>("life_icon");
            LifesCountText = go.GetCompItem<TextMeshProUGUI>("lifes_count_text");
            LifeBrokenAnimator = go.GetCompItem<Animator>("life_broken_animator");
            LifeEnabled = PrefabInitializer.GetObject<Sprite>("icons", "icon_life_enabled");
            LifeDisabled = PrefabInitializer.GetObject<Sprite>("icons", "icon_life_disabled");
        }
        
        #endregion

        #region api

        public static ILifesPanel Create(RectTransform _Parent, IGameDialogViewer _DialogViewer)
        {
            return new LifesPanel(_Parent, _DialogViewer);
        }
        
        public void Show()
        {
            Panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            Panel.gameObject.SetActive(false);
        }

        public void SetLifes(long _Count)
        {
            LifesCount = _Count;
        }
        
        #endregion

        #region nonpublic methods
        
        private void SetLifesCountTextAndIcon()
        {
            LifesCountText.text = m_LifesCount.ToNumeric();
            LifeIcon.sprite = LifesCount > 0 ? LifeEnabled : LifeDisabled;
        }

        private void AnimateBrokenLifes(long _BrokenLifesCount)
        {
            Coroutines.Run(Coroutines.Repeat(
                () =>
                {
                    GameObject go = Object.Instantiate(
                        LifeBrokenAnimator.gameObject,
                        LifeBrokenAnimator.transform.parent);
                    Animator anim = go.GetComponent<Animator>();
                    anim.SetTrigger(AkBrokenLife);
                },
                0.1f,
                _BrokenLifesCount));
        }

        private void SetPanelWidth()
        {
            float textWidth = (Utility.SymbolWidth + 4) * m_LifesCount.ToNumeric().Length;
            var textRtr = LifesCountText.RTransform();
            textRtr.sizeDelta = textRtr.sizeDelta.SetX(textWidth);
            Panel.sizeDelta = Panel.sizeDelta.SetX(textWidth + LifeIcon.RTransform().sizeDelta.x + 40);
        }
        
        #endregion
    }
}