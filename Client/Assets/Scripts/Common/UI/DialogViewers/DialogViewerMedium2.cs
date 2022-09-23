using Common.CameraProviders;
using Common.Managers;
using Common.Ticker;

namespace Common.UI.DialogViewers
{
    public interface IDialogViewerMedium2 : IDialogViewerMedium { }
    
    public class DialogViewerMedium2Fake : DialogViewerMediumFake, IDialogViewerMedium2 { }
    
    public class DialogViewerMedium2 : DialogViewerMediumBase, IDialogViewerMedium2
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
    }
}