using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class VectorLinCombConstraint : MonoBehaviour
{
    public GameObject target;
    public int dim;
    public GridManager gridManager;
    
    private VectorManager _thisVectorManager;
    private VectorManager _targetVectorManager;


    // Start is called before the first frame update
    void Start()
    {
        _thisVectorManager = GetComponent<VectorManager>();
        _targetVectorManager = target.GetComponent<VectorManager>();
    }

    // Update is called once per frame
    void Update()
    {
        float scalar = _targetVectorManager.value[dim];
        Vector3 start = _targetVectorManager.GetCurStartPoint();
        Vector3 value = Vector3.zero;
        value[dim] = scalar;
        if (dim == 1)
        {
            start += new Vector3(_targetVectorManager.value[0], 0, 0) * gridManager.euclideanGridScale;
        }
        _thisVectorManager.SetNewStartPoint(start);
        _thisVectorManager.SetNewValue(value);
    }
}
