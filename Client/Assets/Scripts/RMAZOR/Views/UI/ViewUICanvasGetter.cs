using Common.Helpers;
using Common.UI;
using Common.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.Views.UI
{
    public class ViewUICanvasGetter : InitBase, IViewUICanvasGetter
    {
        #region nonpublic members

        private Canvas m_Canvas;
        
        #endregion

        #region api
        
        public override void Init()
        {
            CreateCanvas();
            base.Init();
        }

        public Canvas GetCanvas()
        {
            return m_Canvas;
        }

        #endregion

        #region nonpublic methods
        
        private void CreateCanvas()
        {
            m_Canvas = UIUtils.UiCanvas(
                GetType().Name + " Canvas",
                RenderMode.ScreenSpaceOverlay,
                false,
                0,
                AdditionalCanvasShaderChannels.None,
                CanvasScaler.ScaleMode.ScaleWithScreenSize,
                new Vector2Int(1920, 1080),
                CanvasScaler.ScreenMatchMode.Shrink,
                0f,
                100,
                true,
                GraphicRaycaster.BlockingObjects.None);
            m_Canvas.enabled = false;
        }

        #endregion
    }

    public class ViewUICanvasGetterFake : InitBase, IViewUICanvasGetter
    {
        public Canvas GetCanvas() => null;
    }
}