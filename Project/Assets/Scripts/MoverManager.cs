using UnityEngine;

// This class manages the "hidden" NearInterationGrabbable that allows
// basis vectors to be manipulated by pinching. Due to the nature of NearInterationGrabbable,
// even with a Z-axis move constraint, the rotation of the object will still change as the rotation
// of the camera changes. Therefore, at the end of interaction, a reset position and rotation callback must
// be called.
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