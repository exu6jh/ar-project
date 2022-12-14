using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    // [FormerlySerializedAs("BasisVectorParent")]
    public GameObject GridlineParent;
    public GameObject VectorParent;
    public GameObject origin;
    public GameObject[] bases; 
    public bool transformable = false;
    public bool linearMap = false;
    public Matrix4x4 tMatrix;

    [HideInInspector] public float euclideanGridScale = 2.5f;

    private VectorManager[] _vectorManagers;
    private int dimensions = 2;

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        _vectorManagers = (from basis in bases
            select basis.GetComponent<VectorManager>()).ToArray();
        tMatrix = Matrix4x4.identity;
    }

    private void Update()
    {
        if (transformable)
        {
            for(int i = 0; i < dimensions; i++)
            {
                Vector3 basisValue = _vectorManagers[i].value;
                tMatrix[0, i] = basisValue[0];
                tMatrix[1, i] = basisValue[1];
                tMatrix[2, i] = basisValue[2];
            }

            foreach (Transform gridline in GridlineParent.transform)
            {
                GridlineManager gridlineManager = gridline.GetComponent<GridlineManager>();
                Vector3 position = Vector3.zero;
                foreach (GridMovement gridMovement in gridlineManager.movement)
                {
                    position[gridMovement.dimension] = gridMovement.offset * euclideanGridScale;
                }

                position = tMatrix * position;

                gridline.localPosition = position;
                
                // Still need code for rotation...
            }

            if (linearMap)
            {
                foreach (Transform vector in VectorParent.transform)
                {
                    Debug.Log("????");
                }
            }
        }
    }
    
    // public void 
}