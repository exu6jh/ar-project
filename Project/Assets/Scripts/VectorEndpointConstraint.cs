using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VectorEndpointConstraint : MonoBehaviour
{
    public GameObject from;
    public GameObject to;
    public GridManager gridManager;
    
    private BasisVectorManager _basisVectorManager;


    // Start is called before the first frame update
    void Start()
    {
        _basisVectorManager = GetComponent<BasisVectorManager>();
    }

    // Update is called once per frame
    void Update()
    {
        _basisVectorManager.SetNewStartPoint(from.transform.localPosition);
        _basisVectorManager.SetNewValue((to.transform.localPosition - from.transform.localPosition) / gridManager.euclideanGridScale);
    }
}
