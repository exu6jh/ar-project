using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VectorEndpointConstraint : MonoBehaviour
{
    public GameObject from;
    public GameObject to;
    public GridManager gridManager;
    
    private VectorManager _vectorManager;


    // Start is called before the first frame update
    void Start()
    {
        _vectorManager = GetComponent<VectorManager>();
    }

    // Update is called once per frame
    void Update()
    {
        _vectorManager.SetNewStartPoint(from.transform.localPosition);
        _vectorManager.SetNewValue((to.transform.localPosition - from.transform.localPosition) / gridManager.euclideanGridScale);
    }
}
