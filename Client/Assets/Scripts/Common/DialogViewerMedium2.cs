using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;

namespace Common
{
    public interface IDialogViewerMedium2 : IDialogViewerMedium { }
    
    public class DialogViewerMedium2Fake : DialogViewerMediumFake, IDialogViewerMedium2 { }

    public class DialogViewerMedium2 : DialogViewerCommonBase, IDialogViewerMedium2
    {
        protected override string PrefabName => "medium_2";

        public DialogViewerMedium2(
            IViewUICanvasGetter _CanvasGetter,
            IUITicker           _Ticker,
            ICameraProvider     _CameraProvider,
            IPrefabSetManager   _PrefabSetManager)
            : base(
                _CanvasGetter,
                _Ticker,
                _CameraProvider,
                _PrefabSetManager) { }

        public override int    Id         => MazorCommonDialogViewerIds.Medium2;
        public override string CanvasName => CommonCanvasNames.CommonScreenSpace;
    }
}