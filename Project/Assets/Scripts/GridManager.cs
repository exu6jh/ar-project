using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


// UGLY TRASH
// DO NOT TOUCH
// YOU'LL BREAK THE MAGIC
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}

public record Transformable
{
    public record MVectorManager(VectorManager VectorManager) : Transformable();
    public record MPointManager(PointManager PointManager) : Transformable();
    public record MPointSnapConstraint(PointSnapConstraint PointSnapConstraint) : Transformable();

    public static readonly Type[] validTypes = new[] {typeof(VectorManager), typeof(PointManager), typeof(PointSnapConstraint)};
    
    private Transformable(){}
}

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    public GameObject GridlineParent;
    public GameObject VectorParent;
    public GameObject origin;
    public GameObject[] bases; 
    public bool transformable = false;
    public bool linearMap = false;
    public GameObject[] toTransform;
    
    public Matrix4x4 tMatrix;
    [HideInInspector] public float gridLength = 100;
    [HideInInspector] public float euclideanGridScale = 2.5f;

    private VectorManager[] _vectorManagers;
    private Transformable[] _transformables;
    private int dimensions = 2;

    private void Start()
    {
        Reset();
    }

    private static List<Transformable> createTransformable(GameObject gameObject)
    {
        // MethodInfo methodInfo = typeof(GameObject).GetMethod("GetComponent");
        // foreach (Type type in Transformable.validTypes)
        // {
        //     MethodInfo GetComponentType = methodInfo.MakeGenericMethod(type);
        //     dynamic typeInstance = GetComponentType.Invoke(gameObject, null);
        //     if (typeInstance != null)
        //     {
        //         // ???
        //     }
        // }
        List<Transformable> transformables = new List<Transformable>();
        VectorManager vectorManager = gameObject.GetComponent<VectorManager>();
        if (vectorManager != null)
        {
            transformables.Add(new Transformable.MVectorManager(vectorManager));
        }

        PointManager pointManager = gameObject.GetComponent<PointManager>();
        if (pointManager != null)
        {
            transformables.Add(new Transformable.MPointManager(pointManager));
        }

        PointSnapConstraint PointSnapConstraint = gameObject.GetComponent<PointSnapConstraint>();
        if (pointManager != null)
        {
            transformables.Add(new Transformable.MPointSnapConstraint(PointSnapConstraint));
        }

        if (transformables.Count == 0)
        { 
            throw new Exception("Bruh. No valid types.");  
        }
        return transformables;
    }

    public void Reset()
    {
        foreach (VectorManager vectorManager in GetComponentsInChildren<VectorManager>())
        {
            vectorManager.gridManager = this;
        }
        foreach (PointManager pointManager in GetComponentsInChildren<PointManager>())
        {
            pointManager.gridManager = this;
        }
        foreach (MoverManager moverManager in GetComponentsInChildren<MoverManager>())
        {
            moverManager.ResetZAndRotation();
        }
        
        _vectorManagers = (from basis in bases 
            select basis.GetComponent<VectorManager>()).ToArray();
        _transformables = (from mappable in toTransform
            from transformables in createTransformable(mappable)
            select transformables).ToArray();
        Debug.Log("All Transformables:");
        foreach (Transformable transformable in _transformables)
        {
            Debug.Log(transformable);
        }
        tMatrix = Matrix4x4.identity;
        transformable = !transformable;
        ToggleTransformableStatus();
    }

    public void ToggleTransformableStatus()
    {
        transformable = !transformable;
        foreach (PointSnapConstraint pointSnapConstraint in GetComponentsInChildren<PointSnapConstraint>())
        {
            pointSnapConstraint.disableSnapping = transformable;
        }

        foreach (GameObject gameObject in bases)
        {
            gameObject.GetComponent<VectorEndpointConstraint>().to.SetActive(transformable);
        }
    }

    public void ToggleLinearMapStatus()
    {
        linearMap = !linearMap;
    }

    public void SetGridlinePosition(Transform gridline)
    {
        
    }

    private void Update()
    {
        if (transformable)
        {
            for(int i = 0; i < dimensions; i++)
            {
                Vector3 basisValue = _vectorManagers[i].standardValue;
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
                Vector3 pointingDimVec = _vectorManagers[gridlineManager.pointingDim].standardValue;
                gridline.localRotation = Quaternion.FromToRotation(Vector3.up, pointingDimVec);
                
                // Scale
                Vector3 localScale = gridline.localScale;
                localScale.y = gridLength / 2 * pointingDimVec.magnitude;
                gridline.localScale = localScale;
            }

            if (linearMap)
            {
                foreach (Transformable transformable in _transformables)
                {
                    switch (transformable)
                    {
                        case Transformable.MVectorManager mVectorManager:
                            VectorManager vectorManager = mVectorManager.VectorManager;
                            if (vectorManager.activeConstraints.Count == 0)
                            {
                                vectorManager.SetNewValue(vectorManager.value);
                            }
                            break;
                        case Transformable.MPointManager mPointManager:
                            PointManager pointManager = mPointManager.PointManager;
                            if (pointManager.activeConstraints.Count == 0)
                            {
                                Debug.Log("Point active!");
                                pointManager.SetNewValue(pointManager.value);   
                            }
                            break;
                        case Transformable.MPointSnapConstraint mPointSnapConstraint:
                            PointSnapConstraint pointSnapConstraint = mPointSnapConstraint.PointSnapConstraint;
                            pointSnapConstraint.SetFollowValue(pointSnapConstraint.followValue);
                            break;
                    }
                }
            }
        }
    }
    
    // public void 
}