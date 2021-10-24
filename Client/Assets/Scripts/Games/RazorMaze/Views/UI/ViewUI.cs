using System.Collections.Generic;
using Controllers;
using DI.Extensions;
using DialogViewers;
using Entities;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.InputConfigurators;
using Managers;
using Ticker;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUI : ViewUIBase
    {
        #region types

        private class BoolArgs
        {
            public bool Value { get; set; }
        }

        #endregion

        #region nonpublic members

        private bool m_OnStart = true;

        #endregion
        
        
        #region inject

        private IManagersGetter Managers { get; }
        private IUITicker UITicker { get; }
        private IDialogViewer DialogViewer { get; }
        private INotificationViewer NotificationViewer { get; }
        private IDialogPanels DialogPanels { get; }
        private ITransitionRenderer TransitionRenderer { get; }
        private ILoadingController LoadingController { get; }
        private IViewInputConfigurator InputConfigurator { get; }

        public ViewUI(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IDialogViewer _DialogViewer,
            INotificationViewer _NotificationViewer,
            IDialogPanels _DialogPanels,
            ITransitionRenderer _TransitionRenderer,
            ILoadingController _LoadingController,
            IViewUIGameControls _ViewUIGameControls,
            IViewInputConfigurator _InputConfigurator)
            : base(_ViewUIGameControls)
        {
            Managers = _Managers;
            UITicker = _UITicker;
            DialogViewer = _DialogViewer;
            NotificationViewer = _NotificationViewer;
            DialogPanels = _DialogPanels;
            TransitionRenderer = _TransitionRenderer;
            LoadingController = _LoadingController;
            InputConfigurator = _InputConfigurator;
            _UITicker.Register(this);
        }

        #endregion
        
        #region api

        public override void Init()
        {
            InputConfigurator.Command += OnCommand;
            
            DataFieldsMigrator.InitDefaultDataFieldValues();
            CreateCanvas();
            var parent = m_Canvas.RTransform();
            DialogViewer.Init(parent);
            NotificationViewer.Init(parent);
            TransitionRenderer.Init(parent);
            UIGameControls.Init();
            RaiseInitializedEvent();
        }

        private void OnCommand(int _Key, object[] _Args)
        {
            switch (_Key)
            {
                case InputCommands.SettingsMenu:
                    DialogPanels.SettingDialogPanel.Init();
                    DialogViewer.Show(DialogPanels.SettingDialogPanel);
                    break;
                case InputCommands.ShopMenu:
                    DialogPanels.ShopDialogPanel.Init();
                    DialogViewer.Show(DialogPanels.ShopDialogPanel);
                    break;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            UIGameControls.OnLevelStageChanged(_Args);
        }
 

        #endregion

        #region nonpublic methods
        
        // private void OnStart()
        // {
        //     if (!m_OnStart)
        //     {
        //         ShowMenu(false);
        //         return;
        //     }
        //     CreateLoadingPanel();
        //     m_OnStart = false;
        // }

        private void ShowMenu(bool _OnStart)
        {
            if (!_OnStart)
            {
                // TODO
                return;
            }
            
            TransitionRenderer.TransitionAction = (_, _Args) =>
            {
                // TODO
            };
            TransitionRenderer.StartTransition();
        }
        
        private void CreateLoadingPanel()
        {
            var authFinishedArgs = new BoolArgs();
            var loadingPanel = DialogPanels.LoadingDialogPanel;
            loadingPanel.Init();
            DialogViewer.Show(loadingPanel);

            LoadingController.Init(OnLoadingResult, GetLoadingStages(authFinishedArgs));

            LoadingController.StartStage(1, 
                () => AssetBundleManager.BundlesLoaded, 
                () => AssetBundleManager.Errors);
            
            LoadingController.StartStage(2,
                () => authFinishedArgs.Value, () => null);
        }
        
        private void OnLoadingResult(LoadingResult _Result)
        {
            switch (_Result)
            {
                case LoadingResult.Success:
                    ShowMenu(true);
                    break;
                case LoadingResult.Fail:
                    //TODO if failed to load, do something with it
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Result);
            }
        }

        private List<LoadingStage> GetLoadingStages(BoolArgs _AuthenticationFinishedArgs)
        {
            return new List<LoadingStage>
            {
                new LoadingStage(1, "Loading resources", 0.5f, () => { }),
                new LoadingStage(2, "Connecting to server", 0.5f, () =>
                {
                    var authCtrl = new AuthController();
                    authCtrl.Authenticate(_AuthResult =>
                    {
                        bool authorizedAtLeastOnce = SaveUtils.GetValue<bool>(SaveKey.AuthorizedAtLeastOnce);

                        switch (_AuthResult)
                        {
                            case AuthController.AuthResult.LoginSuccess:
                                if (!authorizedAtLeastOnce)
                                {
                                    DataFieldsMigrator.MigrateFromDatabase();
                                    SaveUtils.PutValue(SaveKey.AuthorizedAtLeastOnce, true);
                                }

                                break;
                            case AuthController.AuthResult.RegisterSuccess:
                                DataFieldsMigrator.MigrateFromPrevious();
                                SaveUtils.PutValue(SaveKey.AuthorizedAtLeastOnce, true);
                                break;
                            case AuthController.AuthResult.LoginFailed:
                            case AuthController.AuthResult.RegisterFailed:
                            case AuthController.AuthResult.FailedNoInternet:
                                break;
                            default:
                                throw new SwitchCaseNotImplementedException(_AuthResult);
                        }
                        _AuthenticationFinishedArgs.Value = true;
                    });
                })
            };
        }

        #endregion
    }
}