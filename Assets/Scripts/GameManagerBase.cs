using Constants;
using Helpers;
using Lean.Touch;
using PointsTapper;
using UI;
using UnityEngine;

public abstract class GameManagerBase : MonoBehaviour, IGameManager, ISingleton
{
    #region nonpublic members
    
    private LeanTouch m_MainTouchSystem;
    
    #endregion
    
    #region api
    
    public ILevelController LevelController { get; protected set; }
    public IGameMenuUi GameMenuUi { get; protected set; }

    public virtual void Init(int _Level)
    {
        LevelController = new LevelController();
        LevelController.Level = _Level;
        LevelController.OnLevelStarted += OnLevelStarted;
        LevelController.OnLevelFinished += OnLevelFinished;
    }
    
    #endregion
    
    #region protected methods
    
    protected virtual void InitTouchSystem()
    {
        var touchSystemObj = new GameObject("Main Touch System");
        m_MainTouchSystem = touchSystemObj.AddComponent<LeanTouch>();
        var mts = m_MainTouchSystem;
        mts.TapThreshold = 0.5f;
        mts.SwipeThreshold = 50f;
        mts.ReferenceDpi = 200;
        mts.GuiLayers = LayerMask.GetMask(LayerNames.Touchable);
        mts.RecordFingers = true;
        mts.RecordThreshold = 5f;
        mts.RecordLimit = 0f;
        mts.SimulateMultiFingers = true;
        mts.PinchTwistKey = KeyCode.LeftControl;
        mts.MovePivotKey = KeyCode.LeftAlt;
        mts.MultiDragKey = KeyCode.LeftAlt;
        mts.FingerTexture = PrefabInitializer.GetObject<Texture2D>("icons", "finger_texture");
    }

    protected abstract void OnLevelStarted(LevelChangedArgs _Args);
    protected abstract void OnLevelFinished(LevelChangedArgs _Args);
    
    #endregion
    
    #region engine methods
    
    protected virtual void Start()
    {
        InitTouchSystem();
    }
    
    #endregion
}
