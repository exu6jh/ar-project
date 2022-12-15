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
    
    public float gridLength = 100;
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
                
                // Position
                Vector3 position = Vector3.zero;
                // Gave up, added pointing dim
                // bool[] dimMove = new bool[dimensions];
                foreach (GridMovement gridMovement in gridlineManager.movement)
                {
                    position[gridMovement.dimension] = gridMovement.offset * euclideanGridScale;
                    // dimMove[gridMovement.dimension] = true;
                }

                position = tMatrix * position;

                gridline.localPosition = position;
                
                // Rotation
                Vector3 pointingDimVec = _vectorManagers[gridlineManager.pointingDim].value;
                gridline.localRotation = Quaternion.FromToRotation(Vector3.up, pointingDimVec);
                
                // Scale
                Vector3 localScale = gridline.localScale;
                localScale.y = gridLength / 2 * pointingDimVec.magnitude;
                gridline.localScale = localScale;
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