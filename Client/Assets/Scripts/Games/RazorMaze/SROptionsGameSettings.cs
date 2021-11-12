using System.ComponentModel;
using Games.RazorMaze;
using UnityEngine;

public partial class SROptions
{
    private const string CategoryMazeItems = "Maze Items";
    private const string CategoryCharacter = "Character";
    private const string CategoryCommon    = "Common";
    private const string CategoryView      = "View";
        
    private static ModelSettings _modelSettings;
    private static ViewSettings  _viewSettings;
    
    public static void Init(ModelSettings _ModelSettings, ViewSettings _ViewSettings)
    {
        _modelSettings = _ModelSettings;
        _viewSettings = _ViewSettings;
    }

    #region model settings

    [Category(CategoryCharacter)]
    public float CharacterSpeed
    {
        get => _modelSettings.CharacterSpeed;
        set => _modelSettings.CharacterSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float MovingItemsSpeed
    {
        get => _modelSettings.MovingItemsSpeed;
        set => _modelSettings.MovingItemsSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float GravityBlockSpeed
    {
        get => _modelSettings.GravityBlockSpeed;
        set => _modelSettings.GravityBlockSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float GravityTrapSpeed
    {
        get => _modelSettings.GravityTrapSpeed;
        set => _modelSettings.GravityTrapSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float MovingItemsPause
    {
        get => _modelSettings.MovingItemsPause;
        set => _modelSettings.MovingItemsPause = value;
    }

    [Category(CategoryMazeItems)]
    public float TrapPreReactTime
    {
        get => _modelSettings.TrapPreReactTime;
        set => _modelSettings.TrapPreReactTime = value;
    }

    [Category(CategoryMazeItems)]
    public float TrapReactTime
    {
        get => _modelSettings.TrapReactTime;
        set => _modelSettings.TrapReactTime = value;
    }

    [Category(CategoryMazeItems)]
    public float TrapAfterReactTime
    {
        get => _modelSettings.TrapAfterReactTime;
        set => _modelSettings.TrapAfterReactTime = value;
    }

    [Category(CategoryMazeItems)]
    public float TrapIncreasingIdleTime
    {
        get => _modelSettings.TrapIncreasingIdleTime;
        set => _modelSettings.TrapIncreasingIdleTime = value;
    }

    [Category(CategoryMazeItems)]
    public float TrapIncreasingIncreasedTime
    {
        get => _modelSettings.TrapIncreasingIncreasedTime;
        set => _modelSettings.TrapIncreasingIncreasedTime = value;
    }

    [Category(CategoryMazeItems)]
    public float TurretPreShootInterval
    {
        get => _modelSettings.TurretPreShootInterval;
        set => _modelSettings.TurretPreShootInterval = value;
    }

    [Category(CategoryMazeItems)]
    public float TurretShootInterval
    {
        get => _modelSettings.TurretShootInterval;
        set => _modelSettings.TurretShootInterval = value;
    }

    [Category(CategoryMazeItems)]
    public float TurretProjectileSpeed
    {
        get => _modelSettings.TurretProjectileSpeed;
        set => _modelSettings.TurretProjectileSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float ShredingerBlockProceedTime
    {
        get => _modelSettings.ShredingerBlockProceedTime;
        set => _modelSettings.ShredingerBlockProceedTime = value;
    }

    #endregion

    #region view settings

    [Category(CategoryView)]
    public float   LineWidth
    {
        get => _viewSettings.LineWidth;
        set => _viewSettings.LineWidth = value;
    }

    [Category(CategoryView)]
    public float   CornerWidth
    {
        get => _viewSettings.CornerWidth;
        set => _viewSettings.CornerWidth = value;
    }

    [Category(CategoryView)]
    public float   CornerRadius
    {
        get => _viewSettings.CornerRadius;
        set => _viewSettings.CornerRadius = value;
    }
    
    [Category(CategoryMazeItems)]
    public float   MovingTrapRotationSpeed
    {
        get => _viewSettings.MovingTrapRotationSpeed;
        set => _viewSettings.MovingTrapRotationSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float   ShredingerLineOffsetSpeed
    {
        get => _viewSettings.ShredingerLineOffsetSpeed;
        set => _viewSettings.ShredingerLineOffsetSpeed = value;
    }

    [Category(CategoryMazeItems)]
    public float   TurretBulletRotationSpeed
    {
        get => _viewSettings.TurretBulletRotationSpeed;
        set => _viewSettings.TurretBulletRotationSpeed = value;
    }

    [Category(CategoryCommon)]
    public int     BlockItemsCount
    {
        get => _viewSettings.BlockItemsCount;
        set => _viewSettings.BlockItemsCount = value;
    }

    [Category(CategoryCommon)]
    public int     PathItemsCount
    {
        get => _viewSettings.PathItemsCount;
        set => _viewSettings.PathItemsCount = value;
    }

    [Category(CategoryView)]
    public bool    StartPathItemFilledOnStart
    {
        get => _viewSettings.StartPathItemFilledOnStart;
        set => _viewSettings.StartPathItemFilledOnStart = value;
    }

    [Category(CategoryCommon)]
    public float   MazeRotationSpeed
    {
        get => _viewSettings.MazeRotationSpeed;
        set => _viewSettings.MazeRotationSpeed = value;
    }

    [Category(CategoryCommon)]
    public float   FinishTimeExcellent
    {
        get => _viewSettings.FinishTimeExcellent;
        set => _viewSettings.FinishTimeExcellent = value;
    }

    [Category(CategoryCommon)]
    public float   FinishTimeGood
    {
        get => _viewSettings.FinishTimeGood;
        set => _viewSettings.FinishTimeGood = value;
    }

    [Category(CategoryCommon)]
    public float   ProposalDialogAnimSpeed
    {
        get => _viewSettings.ProposalDialogAnimSpeed;
        set => _viewSettings.ProposalDialogAnimSpeed = value;
    }

    // [Category(CategoryView)]
    // public Vector4 ScreenOffsets
    // {
    //     get => _viewSettings.ScreenOffsets;
    //     set => _viewSettings.ScreenOffsets = value;
    // }
    
    #endregion
}
