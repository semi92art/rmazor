using Constants;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CountdownPanelView : MonoBehaviour
{
    #region serialized fields
    
    [SerializeField] private TextMeshProUGUI text;
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

    public void SetThree()
    {
        text.text = "3";
    }

    public void SetTwo()
    {
        text.text = "2";
    }

    public void SetOne()
    {
        text.text = "1";
    }

    public void StartGame()
    {
        SetEmpty();
        m_CountdownFinish?.Invoke();
    }

    public void SetEmpty()
    {
        text.text = string.Empty;
    }
    
    #endregion
}
