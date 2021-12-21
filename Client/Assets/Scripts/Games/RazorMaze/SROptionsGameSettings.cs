using System.ComponentModel;
using Constants;
using Entities;
using Games.RazorMaze;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views;
using Utils;

public partial class SROptions
{
    private const string CategoryMazeItems  = "Maze Items";
    private const string CategoryCharacter  = "Character";
    private const string CategoryCommon     = "Common";
    private const string CategoryLoadLevels = "Load Levels";
    private const string CategoryHaptics    = "Haptics";
    private const string CategoryAds        = "Ads";

    private static IModelGame    _model;
    private static IViewGame     _view;
    private static ModelSettings _modelSettings;
    private static ViewSettings  _viewSettings;
    
    public static void Init(
        IModelGame _Model,
        IViewGame _View)
    {
        _model = _Model;
        _view = _View;
        _modelSettings = _Model.Settings;
        _viewSettings = _View.Settings;

        SRDebug.Instance.PanelVisibilityChanged += OnPanelVisibilityChanged;
    }

    private static void OnPanelVisibilityChanged(bool _Visible)
    {
        if (_Visible)
            _view.CommandsProceeder.LockCommands(new []
            {
                EInputCommand.MoveLeft,
                EInputCommand.MoveRight,
                EInputCommand.MoveUp,
                EInputCommand.MoveDown,
                EInputCommand.ShopMenu,
                EInputCommand.SettingsMenu,
                EInputCommand.RotateClockwise,
                EInputCommand.RotateCounterClockwise
            });
        else 
            _view.CommandsProceeder.UnlockAllCommands();
    }

    #region model settings

    [Category(CategoryCharacter)]
    public float Speed
    {
        get => _modelSettings.CharacterSpeed;
        set => _modelSettings.CharacterSpeed = value;
    }

    [Category(CategoryCharacter)]
    public float MoveThreshold
    {
        get => _viewSettings.MoveSwipeThreshold;
        set => _viewSettings.MoveSwipeThreshold = value;
    }

    [Category(CategoryMazeItems)]
    public float Moving_Items_Speed
    {
        get => _modelSettings.MovingItemsSpeed;
        set => _modelSettings.MovingItemsSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float Gravity_Block_Speed
    {
        get => _modelSettings.GravityBlockSpeed;
        set => _modelSettings.GravityBlockSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float Gravity_Trap_Speed
    {
        get => _modelSettings.GravityTrapSpeed;
        set => _modelSettings.GravityTrapSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float Moving_Items_Pause
    {
        get => _modelSettings.MovingItemsPause;
        set => _modelSettings.MovingItemsPause = value;
    }

    [Category(CategoryMazeItems)]
    public float Trap_PreReact_Time
    {
        get => _modelSettings.TrapPreReactTime;
        set => _modelSettings.TrapPreReactTime = value;
    }

    [Category(CategoryMazeItems)]
    public float Trap_React_Time
    {
        get => _modelSettings.TrapReactTime;
        set => _modelSettings.TrapReactTime = value;
    }

    [Category(CategoryMazeItems)]
    public float Trap_AfterReact_Time
    {
        get => _modelSettings.TrapAfterReactTime;
        set => _modelSettings.TrapAfterReactTime = value;
    }

    [Category(CategoryMazeItems)]
    public float Trap_IncreasingIdle_Time
    {
        get => _modelSettings.TrapIncreasingIdleTime;
        set => _modelSettings.TrapIncreasingIdleTime = value;
    }

    [Category(CategoryMazeItems)]
    public float Trap_IncreasingIncreased_Time
    {
        get => _modelSettings.TrapIncreasingIncreasedTime;
        set => _modelSettings.TrapIncreasingIncreasedTime = value;
    }

    [Category(CategoryMazeItems)]
    public float Turret_PreShoot_Interval
    {
        get => _modelSettings.TurretPreShootInterval;
        set => _modelSettings.TurretPreShootInterval = value;
    }

    [Category(CategoryMazeItems)]
    public float Turret_Shoot_Interval
    {
        get => _modelSettings.TurretShootInterval;
        set => _modelSettings.TurretShootInterval = value;
    }

    [Category(CategoryMazeItems)]
    public float Turret_Projectile_Speed
    {
        get => _modelSettings.TurretProjectileSpeed;
        set => _modelSettings.TurretProjectileSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float Shredinger_BlockProceed_Time
    {
        get => _modelSettings.ShredingerBlockProceedTime;
        set => _modelSettings.ShredingerBlockProceedTime = value;
    }

    #endregion

    #region view settings

    [Category(CategoryMazeItems)]
    public float Moving_Trap_Rotation_Speed
    {
        get => _viewSettings.MovingTrapRotationSpeed;
        set => _viewSettings.MovingTrapRotationSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float Shredinger_Line_Offset_Speed
    {
        get => _viewSettings.ShredingerLineOffsetSpeed;
        set => _viewSettings.ShredingerLineOffsetSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float Turret_Bullet_Rotation_Speed
    {
        get => _viewSettings.TurretBulletRotationSpeed;
        set => _viewSettings.TurretBulletRotationSpeed = value;
    }

    [Category(CategoryCommon)]
    public float Maze_Rotation_Speed
    {
        get => _viewSettings.MazeRotationSpeed;
        set => _viewSettings.MazeRotationSpeed = value;
    }

    [Category(CategoryCommon)]
    public float Finish_Time_Excellent
    {
        get => _viewSettings.FinishTimeExcellent;
        set => _viewSettings.FinishTimeExcellent = value;
    }

    [Category(CategoryCommon)]
    public float Finish_Time_Good
    {
        get => _viewSettings.FinishTimeGood;
        set => _viewSettings.FinishTimeGood = value;
    }

    [Category(CategoryCommon)]
    public float Proposal_Dialog_Anim_Speed
    {
        get => _viewSettings.ProposalDialogAnimSpeed;
        set => _viewSettings.ProposalDialogAnimSpeed = value;
    }

    [Category(CategoryCommon)]
    public bool AddMoney500
    {
        get => false;
        set
        {
            var moneyEntity = _view.Managers.ScoreManager.GetScore(DataFieldIds.Money, true);
            Coroutines.Run(Coroutines.WaitWhile(
                () => moneyEntity.Result != EEntityResult.Pending,
                () =>
                {
                    if (moneyEntity.Result == EEntityResult.Fail)
                    {
                        Dbg.LogError("Failed to load money score.");
                        return;
                    }
                    var score = moneyEntity.GetFirstScore();
                    if (!score.HasValue)
                    {
                        Dbg.LogError("Money score entity does not contain first score.");
                        return;
                    }
                    _view.Managers.ScoreManager.SetScore(DataFieldIds.Money, score.Value + 500, false);
                }));
        } 
    }

    #endregion

    #region other settings

    [Category(CategoryLoadLevels)]
    public int Level_Index { get; set; }

    [Category(CategoryLoadLevels)]
    public bool Load_By_Index
    {
        get => false;
        set
        {
            if (!value)
                return;
            _view.CommandsProceeder.RaiseCommand(
                EInputCommand.LoadLevelByIndex, 
                new object[] { Level_Index - 1 },
                true);
        }
    }

    [Category(CategoryLoadLevels)]
    public bool Load_Next_Level
    {
        get => false;
        set
        {
            if (!value)
                return;
            _view.CommandsProceeder.RaiseCommand(
                EInputCommand.LoadNextLevel,
                null, 
                true);
        }
    }
    
    [Category(CategoryLoadLevels)]
    public bool Load_Previous_Level
    {
        get => false;
        set
        {
            if (!value)
                return;
            int levelIndex = _model.LevelStaging.LevelIndex;
            _view.CommandsProceeder.RaiseCommand(
                EInputCommand.LoadLevelByIndex, 
                new object[] { levelIndex - 1 },
                true);
        }
    }
    
    [Category(CategoryLoadLevels)]
    public bool Load_Current_Level
    {
        get => false;
        set
        {
            if (!value)
                return;
            _view.CommandsProceeder.RaiseCommand(
                EInputCommand.LoadCurrentLevel,
                null, 
                true);
        }
    }
    
    [Category(CategoryLoadLevels)]
    public bool Load_Random_Level
    {
        get => false;
        set
        {
            if (!value)
                return;
            _view.CommandsProceeder.RaiseCommand(
                EInputCommand.LoadRandomLevel,
                null, 
                true);
        }
    }
    
    [Category(CategoryLoadLevels)]
    public bool Load_Random_Level_With_Rotation
    {
        get => false;
        set
        {
            if (!value)
                return;
            _view.CommandsProceeder.RaiseCommand(
                EInputCommand.LoadRandomLevelWithRotation,
                null, 
                true);
        }
    }
    
    [Category(CategoryLoadLevels)]
    public bool Finish_Current_Level
    {
        get => false;
        set
        {
            if (!value)
                return;
            _view.CommandsProceeder.RaiseCommand(
                EInputCommand.FinishLevel,
                null, 
                true);
        }
    }

    [Category(CategoryHaptics)]
    public float Amplitude { get; set; }
    
    [Category(CategoryHaptics)]
    public float Frequency { get; set; }
    
    [Category(CategoryHaptics)]
    public float Duration { get; set; }
    
    [Category(CategoryHaptics)]
    public bool Play_Constant
    {
        get => false;
        set
        {
            _view.Managers.HapticsManager.Play(Amplitude, Frequency, Duration);
        }
    }

    [Category(CategoryAds)]
    public bool Rewarded_Ad_Ready_State
    {
        get => false;
        set => Dbg.Log($"Rewarded ad ready state: {_view.Managers.AdsManager.RewardedAdReady}.");
    }
    
    [Category(CategoryAds)]
    public bool Show_Ads
    {
        get
        {
            var res = SaveUtils.GetValue(SaveKeys.DisableAds);
            if (res.HasValue) return 
                res.Value;
            SaveUtils.PutValue(SaveKeys.DisableAds, false);
            return false;
        }
        set
        {
            SaveUtils.PutValue(SaveKeys.DisableAds, !value);
            Dbg.Log($"Ads enabled: {value}.");
        }
    }

    [Category(CategoryAds)]
    public bool Show_Rewarded_Ad
    {
        get => false;
        set
        {
            _view.Managers.AdsManager.ShowRewardedAd(
                () => Dbg.Log("Rewarded ad was shown."));
        }
    }
    
    #endregion
    
}
