using System.Collections.Generic;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.Views.UI
{
    public class ViewUICanvasGetterFake : InitBase, IViewUICanvasGetter
    {
        public Canvas GetCanvas(string _Name) => null;
    }
    
    public class ViewUICanvasGetter : InitBase, IViewUICanvasGetter
    {
        #region nonpublic members

        private readonly Dictionary<string, Canvas> m_Canvases
            = new Dictionary<string, Canvas>();
        
        #endregion

        #region inject
        
        private ICameraProvider CameraProvider { get; }

        public ViewUICanvasGetter(ICameraProvider _CameraProvider)
        {
            CameraProvider = _CameraProvider;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            base.Init();
        }

        public Canvas GetCanvas(string _Name)
        {
            if (m_Canvases.ContainsKey(_Name))
                return m_Canvases[_Name];
            return CreateCanvas(_Name);
        }

        #endregion

        #region nonpublic methods
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            if (m_Canvases.ContainsKey(CommonCanvasNames.CommonCameraSpace))
                m_Canvases[CommonCanvasNames.CommonCameraSpace].worldCamera = _Camera;
        }
        
        private Canvas CreateCanvas(string _Name)
        {
            switch (_Name)
            {
                case CommonCanvasNames.CommonScreenSpace:
                    return CreateCommonScreenSpaceCanvas();
                case CommonCanvasNames.CommonCameraSpace:
                    return CreateCommonCameraSpaceCanvas();
                default: throw new SwitchCaseNotImplementedException(_Name);
            }
        }

        private Canvas CreateCommonScreenSpaceCanvas()
        {
            var canvas = UIUtils.UiCanvas(
                 "Canvas Common Screen Space",
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
            canvas.enabled = false;
            m_Canvases.SetSafe(CommonCanvasNames.CommonScreenSpace, canvas);
            return canvas;
        }

        private Canvas CreateCommonCameraSpaceCanvas()
        {
            var canvas = UIUtils.UiCanvas(
                "Canvas Common Camera Space",
                RenderMode.ScreenSpaceCamera,
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
            canvas.worldCamera = CameraProvider.Camera;
            canvas.enabled = false;
            m_Canvases.SetSafe(CommonCanvasNames.CommonCameraSpace, canvas);
            return canvas;
        }

        #endregion
    }
}