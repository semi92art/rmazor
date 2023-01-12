using System.Collections.Generic;
using Common;
using Common.Entities;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Characters;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelLoaded
    {
        void OnLevelLoaded(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems);
    }
    
    public class ViewLevelStageControllerOnLevelLoaded : IViewLevelStageControllerOnLevelLoaded
    {
        #region inject
        
        private ViewSettings                     ViewSettings                { get; }
        private IModelGame                       Model                       { get; }
        private IScoreManager                    ScoreManager                { get; }
        private IViewCharacter                   Character                   { get; }
        private IViewFullscreenTransitioner      FullscreenTransitioner      { get; }
        private IViewMazePathItemsGroup          PathItemsGroup              { get; }
        private IViewCameraEffectsCustomAnimator CameraEffectsCustomAnimator { get; }

        public ViewLevelStageControllerOnLevelLoaded(
            ViewSettings                     _ViewSettings,
            IModelGame                       _Model,
            IScoreManager                    _ScoreManager,
            IViewCharacter                   _Character,
            IViewFullscreenTransitioner      _FullscreenTransitioner,
            IViewMazePathItemsGroup          _PathItemsGroup,
            IViewCameraEffectsCustomAnimator _CameraEffectsCustomAnimator)
        {
            ViewSettings                = _ViewSettings;
            Model                       = _Model;
            ScoreManager                = _ScoreManager;
            Character                   = _Character;
            FullscreenTransitioner      = _FullscreenTransitioner;
            PathItemsGroup              = _PathItemsGroup;
            CameraEffectsCustomAnimator = _CameraEffectsCustomAnimator;
        }


        #endregion

        #region api
        
        public void OnLevelLoaded(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems)
        {
            SaveGame(_Args);
            Character.Appear(true);
            foreach (var pathItem in PathItemsGroup.PathItems)
            {
                bool collect = Model.PathItemsProceeder.PathProceeds[pathItem.Props.Position];
                pathItem.Collect(collect, true);
            }
            foreach (var mazeItem in _MazeItems)
                mazeItem.Appear(true);
            FullscreenTransitioner.Enabled = true;
            FullscreenTransitioner.DoTextureTransition(false, ViewSettings.betweenLevelTransitionTime);
            CameraEffectsCustomAnimator.AnimateCameraEffectsOnBetweenLevelTransition(true);
        }
        
        #endregion

        #region nonpublic methods

        private void SaveGame(LevelStageArgs _Args)
        {
            var savedGameEntity = ScoreManager.GetSavedGameProgress(
                MazorCommonData.SavedGameFileName, true);
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
                    var newSavedGame = new SavedGame
                    {
                        FileName = MazorCommonData.SavedGameFileName,
                        Money = savedGame.Money,
                        Level = _Args.LevelIndex,
                        Args = _Args.Args
                    };
                    ScoreManager.SaveGameProgress(
                        newSavedGame, false);
                }));
        }
        
        #endregion
    }
}