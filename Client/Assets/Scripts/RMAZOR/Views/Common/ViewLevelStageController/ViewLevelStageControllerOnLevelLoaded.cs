using System.Collections.Generic;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Managers.PlatformGameServices;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Characters;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.UI.Game_Logo;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelLoaded
    {
        void OnLevelLoaded(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems);
    }
    
    public class ViewLevelStageControllerOnLevelLoaded : IViewLevelStageControllerOnLevelLoaded
    {
        #region nonpublic members
        
        private bool m_StartLogoShowing = true;

        #endregion

        #region inject
        
        private ViewSettings                     ViewSettings                { get; }
        private IModelGame                       Model                       { get; }
        private IScoreManager                    ScoreManager                { get; }
        private IViewCharacter                   Character                   { get; }
        private IViewFullscreenTransitioner      FullscreenTransitioner      { get; }
        private IViewMazePathItemsGroup          PathItemsGroup              { get; }
        private IViewCameraEffectsCustomAnimator CameraEffectsCustomAnimator { get; }
        private CompanyLogo                      CompanyLogo                 { get; }
        private IViewUIGameLogo                  GameLogo                    { get; }

        public ViewLevelStageControllerOnLevelLoaded(
            ViewSettings                     _ViewSettings,
            IModelGame                       _Model,
            IScoreManager                    _ScoreManager,
            IViewCharacter                   _Character,
            IViewFullscreenTransitioner      _FullscreenTransitioner,
            IViewMazePathItemsGroup          _PathItemsGroup,
            IViewCameraEffectsCustomAnimator _CameraEffectsCustomAnimator,
            CompanyLogo                      _CompanyLogo,
            IViewUIGameLogo                  _GameLogo)
        {
            ViewSettings                = _ViewSettings;
            Model                       = _Model;
            ScoreManager                = _ScoreManager;
            Character                   = _Character;
            FullscreenTransitioner      = _FullscreenTransitioner;
            PathItemsGroup              = _PathItemsGroup;
            CameraEffectsCustomAnimator = _CameraEffectsCustomAnimator;
            CompanyLogo                 = _CompanyLogo;
            GameLogo                    = _GameLogo;
        }


        #endregion

        #region api
        
        
        public void OnLevelLoaded(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems)
        {
            var savedGameEntity = ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, true);
            Cor.Run(Cor.WaitWhile(
                () => savedGameEntity.Result == EEntityResult.Pending,
                () =>
                {
                    bool castSuccess = savedGameEntity.Value.CastTo(out SavedGame savedGame);
                    if (savedGameEntity.Result == EEntityResult.Fail || !castSuccess)
                    {
                        Dbg.LogWarning("Failed to load saved game: " +
                                       $"_Result: {savedGameEntity.Result}, " +
                                       $"castSuccess: {castSuccess}, " +
                                       $"_Value: {savedGameEntity.Value}");
                        return;
                    }
                    var args = _Args.Args ?? savedGame.Args;
                    long levelIndex = _Args.LevelIndex;
                    string nextLevelType = (string) args.GetSafe(CommonInputCommandArg.KeyNextLevelType, out _);
                    bool nextLevelTypeIsBonus = nextLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
                    if (nextLevelTypeIsBonus)
                    {
                        long bonusLevelIndex = Model.LevelStaging.LevelIndex;
                        levelIndex = RmazorUtils.GetFirstLevelInGroup((int) bonusLevelIndex + 1 + 1);
                    }
                    
                    var newSavedGame = new SavedGame
                    {
                        FileName = CommonData.SavedGameFileName,
                        Money = savedGame.Money,
                        Level = levelIndex,
                        Args = _Args.Args
                    };
                    ScoreManager.SaveGameProgress(
                        newSavedGame, false);
                }));
            if (m_StartLogoShowing)
            {
                CompanyLogo.HideLogo();
                GameLogo.Show();
                m_StartLogoShowing = false;
            }
            Character.Appear(true);
            foreach (var pathItem in PathItemsGroup.PathItems)
            {
                bool collect = Model.PathItemsProceeder.PathProceeds[pathItem.Props.Position];
                pathItem.Collect(collect, true);
            }
            foreach (var mazeItem in _MazeItems)
                mazeItem.Appear(true);
            FullscreenTransitioner.DoTextureTransition(false, ViewSettings.betweenLevelTransitionTime);
            CameraEffectsCustomAnimator.AnimateCameraEffectsOnBetweenLevelTransition(true);
        }

        #endregion
    }
}