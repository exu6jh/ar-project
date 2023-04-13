using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Serialization;


// This class manages Vectors. It updates its standardValue (with standard basis) and value (according to the current
// gridManager tMatrix). Since vectors are made up of cylinder bodies and cone tips, each component has to be transformed
// separetely to avoid deformation of the cone head when the vector has different magnitudes.
[ExecuteInEditMode]
public class VectorManager : MonoBehaviour
{
    public GridManager gridManager;

    public GameObject TransformCylinder;
    public GameObject TransformCone;

    public readonly HashSet<MonoBehaviour> activeConstraints = new HashSet<MonoBehaviour>();
    
    
    public Vector3 standardValue = Vector3.right;
    public Vector3 value;
    public float length = 1;
    public Vector3 euler = new Vector3(0, 0, -90);
    public float zOffset = -0.2f;

    [HideInInspector] public Vector3 zOffsetVector;

    // Necessary for quiz...
    public Vector3 canonicalEuler = new Vector3(0, 0, 0);

    private void Start()
    {
        zOffsetVector = new Vector3(0, 0, zOffset);
    }

    private void Update()
    {
        length = TransformCylinder.transform.localScale.y;
        euler = transform.localRotation.eulerAngles;
        standardValue = Quaternion.Euler(euler) * (Vector3.up * length);
        value = gridManager.tMatrix.inverse * standardValue;
        // MixedRealityPose pose;
        // HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out pose);
        // Vector3 position = pose.Position;
        // Quaternion rotation = pose.Rotation;
    }

    public void SetNewStandardValue(Vector3 newStandardValue)
    {
        standardValue = newStandardValue;
        value = gridManager.tMatrix.inverse * standardValue;
        
        // Length changes
        length = newStandardValue.magnitude;
        
        Vector3 newLocalScale = TransformCylinder.transform.localScale;
        newLocalScale.y = length;
        TransformCylinder.transform.localScale = newLocalScale;
        
        Transform cone = TransformCone.transform.GetChild(0);
        Vector3 newLocalPosition = cone.localPosition;
        newLocalPosition.y = length;
        cone.localPosition = newLocalPosition;
        
        // Rotation changes
        transform.localRotation = Quaternion.FromToRotation(Vector3.up, newStandardValue);
        euler = transform.localRotation.eulerAngles;
    }

    public void SetNewLengthEuler(float length, Vector3 euler)
    {   
        // Length changes
        Vector3 newLocalScale = TransformCylinder.transform.localScale;
        newLocalScale.y = length;
        TransformCylinder.transform.localScale = newLocalScale;
            
        Transform cone = TransformCone.transform.GetChild(0);
        Vector3 newLocalPosition = cone.localPosition;
        newLocalPosition.y = length;
        cone.localPosition = newLocalPosition;
            
        // Rotation changes
        transform.localRotation = Quaternion.Euler(euler);
            
        // Update values
        standardValue = Quaternion.Euler(euler) * (Vector3.up * length);
        value = gridManager.tMatrix.inverse * standardValue;
    }
    
    public void SetNewLength(float length)
    {
        // Length changes
        Vector3 newLocalScale = TransformCylinder.transform.localScale;
        newLocalScale.y = length;
        TransformCylinder.transform.localScale = newLocalScale;
            
        Transform cone = TransformCone.transform.GetChild(0);
        Vector3 newLocalPosition = cone.localPosition;
        newLocalPosition.y = length;
        cone.localPosition = newLocalPosition;
        
        // Update values
        standardValue = Quaternion.Euler(euler) * (Vector3.up * length);
        value = gridManager.tMatrix.inverse * standardValue;
    }

    // public void SetNewLength(SliderEventData data) => SetNewLength(data.NewValue * 10 - 5);
    public void SetNewLength(SliderEventData data)
    {

        if (data.NewValue * 10 < 5)
        {
            SetNewLengthEuler(5 - data.NewValue * 10, (Quaternion.Euler(canonicalEuler) * Quaternion.Euler(0, 0, 180)).eulerAngles);
        }
        else
        {
            SetNewLengthEuler(data.NewValue * 10 - 5, canonicalEuler);
        }
    }

    public void SetNewValue(Vector3 newValue) => SetNewStandardValue(gridManager.tMatrix * newValue);

    public void SetNewStartPoint(Vector3 newStart)
    {
        transform.localPosition = newStart + zOffsetVector;
    }

    public Vector3 GetCurStartPoint()
    {
        return transform.localPosition - zOffsetVector;
    }
}