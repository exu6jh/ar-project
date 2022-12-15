using System;
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
    void Awake()
    {
        _vectorManager = GetComponent<VectorManager>();
    }

    private void OnEnable()
    {
        _vectorManager.activeConstraints.Add(this);
    }

    private void OnDisable()
    {
        _vectorManager.activeConstraints.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        _vectorManager.SetNewStartPoint(from.transform.localPosition);
        _vectorManager.SetNewStandardValue((to.transform.localPosition - from.transform.localPosition) / gridManager.euclideanGridScale);
    }
}
