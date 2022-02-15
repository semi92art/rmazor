using Common.CameraProviders;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace RMAZOR.UI.Panels
{
    public class CharacterDiedDialogPanel2 : CharacterDiedDialogPanelBase
    {
        #region nonpublic members
        protected override string             PrefabName => "character_died_panel_2";
        
        #endregion

        #region inject

        public CharacterDiedDialogPanel2(
            IModelGame _Model,
            IBigDialogViewer            _DialogViewer,
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IProposalDialogViewer       _ProposalDialogViewer,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
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