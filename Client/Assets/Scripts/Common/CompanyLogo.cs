using Common;
using Common.Extensions;
using UnityEngine;

public class CompanyLogo : MonoBehaviour
{
    [SerializeField] private SpriteRenderer logoRend;

    public void ShowLogo()
    {
        logoRend.enabled = true;
    }
    
    public void HideLogo()
    {
        Dbg.Log(nameof(HideLogo));
        logoRend.enabled = false;
        gameObject.DestroySafe();
    }
}