using System;
using UnityEngine;

// [ExecuteInEditMode]
public class PointSnapConstraint : MonoBehaviour
{
    public GameObject origin;
    public GameObject follow;
     

    private Transform followTransform;
    private Transform originTransform;
    
    private PointManager _pointManager;
    private GridManager _gridManager;

    private void Start()
    {
        followTransform = follow.transform;
        originTransform = origin.transform;
        _pointManager = GetComponent<PointManager>();
        _gridManager = _pointManager.gridManager;

        followTransform.parent = transform.parent;
    }

    private void Update()
    {
        Vector3 pos = originTransform.InverseTransformPoint(followTransform.position);
        float closestCoordX = (float) Math.Round(pos.x / _gridManager.euclideanGridScale);
        float closestCoordY = (float) Math.Round(pos.y / _gridManager.euclideanGridScale);
        // float closestCoordZ = (float) Math.Round(pos.z / gridManager.euclideanGridScale);

        _pointManager.SetNewPosition(new Vector3(closestCoordX, closestCoordY, 0));
    }

    public void ResetFollowPosition()
    {
        // followTransform.localPosition = Vector3.zero;
        followTransform.position = transform.position;
        followTransform.localRotation = Quaternion.identity;
    }
}