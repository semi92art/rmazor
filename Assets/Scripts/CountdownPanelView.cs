using Constants;
using UnityEngine;
using UnityEngine.Events;

public class CountdownPanelView : MonoBehaviour
{
    #region serialized fields

    [SerializeField] private Animator animator;
    
    #endregion

    #region api

    private UnityAction m_CountdownFinish;

    public void StartCountdown(UnityAction _CountdownFinish)
    {
        m_CountdownFinish = _CountdownFinish;
        animator.SetTrigger(AnimKeys.Anim);
    }

    #endregion
    
    #region for animator

    public void StartGame()
    {
        m_CountdownFinish?.Invoke();
    }

    #endregion
    
    #region engine methods

    private void Update()
    {
        animator.speed = GameTimeProvider.Instance.Pause ? 0 : 1;
    }
    
    #endregion
}
