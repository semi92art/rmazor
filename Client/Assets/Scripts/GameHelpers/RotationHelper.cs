using UnityEngine;

namespace GameHelpers
{
    public class RotationHelper : MonoBehaviour
    {
        private void Update()
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }
}