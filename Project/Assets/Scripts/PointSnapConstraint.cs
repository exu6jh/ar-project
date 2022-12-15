using System;
using UnityEngine;

// [ExecuteInEditMode]
public class PointSnapConstraint : MonoBehaviour
{
    public GameObject origin;
    public GameObject follow;
     

    private Transform followTransform;
    private Transform originTransform;

    public bool disableSnapping = false;
    
    public Vector3 followValue;
    
    private PointManager _pointManager;
    private GridManager _gridManager;
    private Vector3 _followStandardValue;
    // private Vector3 _followValue;

    private void Awake()
    {
        followTransform = follow.transform;
        originTransform = origin.transform;
        _pointManager = GetComponent<PointManager>();
        _gridManager = _pointManager.gridManager;

        followTransform.parent = transform.parent;
    }

    private void OnEnable()
    {
        _pointManager.activeConstraints.Add(this);
    }

    private void OnDisable()
    {
        _pointManager.activeConstraints.Remove(this);
    }

    private void Update()
    {
        // Vector3 pos = originTransform.InverseTransformPoint(followTransform.position);
        _followStandardValue = followTransform.localPosition / _gridManager.euclideanGridScale;
        followValue = _gridManager.tMatrix.inverse * _followStandardValue;
        float closestCoordX;
        float closestCoordY;
        if (disableSnapping)
        {
            closestCoordX = followValue.x;
            closestCoordY = followValue.y;
        }
        else
        {
            closestCoordX = (float) Math.Round(followValue.x);
            closestCoordY = (float) Math.Round(followValue.y);
        }
        // float closestCoordZ = (float) Math.Round(pos.z / gridManager.euclideanGridScale);

        _pointManager.SetNewValue(new Vector3(closestCoordX, closestCoordY, 0));
    }

    public void ResetFollowPosition()
    {
        // followTransform.localPosition = Vector3.zero;
        Vector3 localPosition = transform.localPosition;
        localPosition.z = 0;
        
        transform.localPosition = localPosition;
        followTransform.localPosition = localPosition;
        followTransform.localRotation = Quaternion.identity;
    }

    public void SetFollowValue(Vector3 newValue)
    {
        followTransform.transform.localPosition = _gridManager.tMatrix * newValue * _gridManager.euclideanGridScale;
    }
}