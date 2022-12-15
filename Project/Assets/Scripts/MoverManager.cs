using UnityEngine;

public class MoverManager : MonoBehaviour
{
    public void ResetZAndRotation()
    {
        Vector3 localPosition = transform.localPosition;
        localPosition.z = 0;
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.identity;
    }
}