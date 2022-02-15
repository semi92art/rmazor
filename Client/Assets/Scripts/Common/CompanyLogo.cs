using Common.Extensions;
using UnityEngine;

public class CompanyLogo : MonoBehaviour
{
    [SerializeField] private SpriteRenderer logoRend;

    public void HideLogo()
    {
        gameObject.DestroySafe();
    }
}