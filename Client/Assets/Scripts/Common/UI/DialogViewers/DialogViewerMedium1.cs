using Common.CameraProviders;
using Common.Managers;
using Common.Ticker;

namespace Common.UI.DialogViewers
{
    public interface IDialogViewerMedium1 : IDialogViewerMedium { }
    
    public class DialogViewerMedium1Fake : DialogViewerMediumFake, IDialogViewerMedium1 { }
    
    public class DialogViewerMedium1 : DialogViewerMediumBase, IDialogViewerMedium1
    {
        protected override string PrefabName => "medium_1";
        
        public DialogViewerMedium1(
            IViewUICanvasGetter _CanvasGetter,
            IUITicker           _Ticker,
            ICameraProvider     _CameraProvider,
            IPrefabSetManager   _PrefabSetManager)
            : base(
                _CanvasGetter,
                _Ticker, 
                _CameraProvider, 
                _PrefabSetManager) { }
    }
}