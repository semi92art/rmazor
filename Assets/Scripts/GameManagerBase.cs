using Constants;
using Helpers;
using Lean.Touch;
using UnityEngine;

public abstract class GameManagerBase : MonoBehaviour, IGameManager
{
    #region api
    
    public abstract void Init();
    
    #endregion
    
    #region protected members

    protected LeanTouch MainTouchSystem;
    
    #endregion

    #region protected methods
    protected abstract void Start_();
    protected abstract void Update_();

    protected virtual void InitTouchSystem()
    {
        var touchSystemObj = new GameObject("Main Touch System");
        MainTouchSystem = touchSystemObj.AddComponent<LeanTouch>();
        var mts = MainTouchSystem;
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
    
    #endregion
    
    #region engine methods
    
    private void Start()
    {
        Start_();
        InitTouchSystem();
    }

    private void Update()
    {
        Update_();
    }

    #endregion
}
