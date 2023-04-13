using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;


// This class constraints a vector to represent a part of the linear combination for a target Vector.
[ExecuteInEditMode]
public class VectorLinCombConstraint : MonoBehaviour
{
    public GameObject target;
    public int dim;
    public GridManager gridManager;
    
    private VectorManager _thisVectorManager;
    private VectorManager _targetVectorManager;


    // Start is called before the first frame update
    void Awake()
    {
        _thisVectorManager = GetComponent<VectorManager>();
        _targetVectorManager = target.GetComponent<VectorManager>();
    }

    private void OnEnable()
    {
        _thisVectorManager.activeConstraints.Add(this);
    }

    private void OnDisable()
    {
        _thisVectorManager.activeConstraints.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        float scalar = _targetVectorManager.standardValue[dim];
        Vector3 start = _targetVectorManager.GetCurStartPoint();
        Vector3 value = Vector3.zero;
        value[dim] = scalar;
        if (dim == 1)
        {
            start += new Vector3(_targetVectorManager.standardValue[0], 0, 0) * gridManager.euclideanGridScale;
        }
        _thisVectorManager.SetNewStartPoint(start);
        _thisVectorManager.SetNewStandardValue(value);
    }
}
