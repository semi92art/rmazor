using Constants;
using DialogViewers;
using Extensions;
using Helpers;
using TMPro;
using UI.Entities;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public interface ILifesPanel : IMiniPanel
    {
        void MinusLife();
        void PlusLife();
        void SetLifes(int _Count);
    }
    
    public class LifesPanel : ILifesPanel
    {
        #region protected members
        
        protected readonly IDialogViewer DialogViewer;
        protected readonly RectTransform Panel;
        protected readonly Image LifeIcon;
        protected readonly TextMeshProUGUI LifesCountText;
        protected readonly Animator LifeBrokenAnimator;
        protected readonly Sprite LifeEnabled;
        protected readonly Sprite LifeDisabled;
        
        protected int LifesCount
        {
            get => m_LifesCount;
            set
            {
                m_LifesCount = value;
                SetLifesCountTextAndIcon();
            }
        }
        
        #endregion
        
        #region private members
        
        private int m_LifesCount;
        private int AkBrokenLife => AnimKeys.Anim;
        
        #endregion
        
        #region constructor
        
        protected LifesPanel(RectTransform _Parent, IDialogViewer _DialogViewer)
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

        public static ILifesPanel Create(RectTransform _Parent, IDialogViewer _DialogViewer)
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

        public void MinusLife()
        {
            LifesCount--;
            LifeBrokenAnimator.SetTrigger(AkBrokenLife);
        }

        public void PlusLife()
        {
            LifesCount++;
        }

        public void SetLifes(int _Count)
        {
            LifesCount = _Count;
        }
        
        #endregion

        #region nonpublic methods
        
        private void SetLifesCountTextAndIcon()
        {
            LifesCountText.text = $"{m_LifesCount}";
            LifeIcon.sprite = LifesCount > 0 ? LifeEnabled : LifeDisabled;
        }
        
        #endregion
    }
}