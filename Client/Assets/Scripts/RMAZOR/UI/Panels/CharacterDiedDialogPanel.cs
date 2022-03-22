using Common.CameraProviders;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.UI.Panels
{
    public class CharacterDiedDialogPanel : CharacterDiedDialogPanelBase
    {
        #region nonpublic members
        
        protected override string PrefabName => "character_died_panel";

        #endregion

        #region inject

        public CharacterDiedDialogPanel(
            CommonGameSettings          _CommonGameSettings,
            IModelGame                  _Model,
            IBigDialogViewer            _DialogViewer,
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IProposalDialogViewer       _ProposalDialogViewer,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _CommonGameSettings,
                _Model,
                _DialogViewer,
                _Managers, 
                _UITicker, 
                _CameraProvider, 
                _ColorProvider, 
                _ProposalDialogViewer,
                _CommandsProceeder) { }
        
        #endregion
    }
}