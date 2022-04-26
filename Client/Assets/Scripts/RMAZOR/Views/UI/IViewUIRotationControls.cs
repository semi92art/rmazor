namespace RMAZOR.Views.UI
{
    public interface IViewUIRotationControls :
        IOnLevelStageChanged,
        IInitViewUIItem,
        IViewUIGetRenderers
    {
        bool HasButtons { get; }
        void OnTutorialStarted(ETutorialType  _Type);
        void OnTutorialFinished(ETutorialType _Type);
    }
}