using System;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

[ExecuteInEditMode]
public class BasisVectorManager : MonoBehaviour
{

    public GameObject TransformCylinder;
    public GameObject TransformCone;
    
    
    public Vector3 value = Vector3.right;
    public float length = 1;
    public Vector3 euler = new Vector3(0, 0, -90);
    public float zOffset = -0.2f;

    [HideInInspector] public Vector3 zOffsetVector;

    private void Start()
    {
        zOffsetVector = new Vector3(0, 0, zOffset);
    }

    private void Update()
    {
        length = TransformCylinder.transform.localScale.y;
        euler = transform.localRotation.eulerAngles;
        value = Quaternion.Euler(euler) * (Vector3.up * length);
        // MixedRealityPose pose;
        // HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out pose);
        // Vector3 position = pose.Position;
        // Quaternion rotation = pose.Rotation;
    }

    public void SetNewValue(Vector3 newValue)
    {
        value = newValue;
        
        // Length changes
        length = newValue.magnitude;
            
        Vector3 newLocalScale = TransformCylinder.transform.localScale;
        newLocalScale.y = length;
        TransformCylinder.transform.localScale = newLocalScale;
            
        Transform cone = TransformCone.transform.GetChild(0);
        Vector3 newLocalPosition = cone.localPosition;
        newLocalPosition.y = length;
        cone.localPosition = newLocalPosition;
            
        // Rotation changes
        transform.localRotation = Quaternion.FromToRotation(Vector3.up, newValue);
        euler = transform.localRotation.eulerAngles;
    }

    public void SetNewLengthEuler(float length, Vector3 euler)
    {
        
    }

    public void SetNewStartPoint(Vector3 newStart)
    {
        transform.localPosition = newStart + zOffsetVector;
    }
}