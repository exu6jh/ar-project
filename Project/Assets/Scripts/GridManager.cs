using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;


// UGLY TRASH
// DO NOT TOUCH
// YOU'LL BREAK THE MAGIC
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}


// This record class serves as the base class for all transformable objects
// Polymorphism is leveraged to allow for differing behaviors for each class
public abstract record Transformable
{
    public abstract GameObject gameObject
    {
        get;
    }

    public abstract Vector3 GetStandardValue();

    public abstract void SetStandardValue(Vector3 newStandardValue);

    public abstract Vector3 GetValue();

    public abstract void SetValue(Vector3 newValue);

    public abstract void SetGridManager(GridManager gridManager);

    // public void DestroyTransformable()
    // {
    //     UnityEngine.Object.Destroy(gameObject);
    // }
    
    public record MVectorManager(VectorManager VectorManager) : Transformable()
    {
        public override GameObject gameObject => VectorManager.gameObject;
        
        public override Vector3 GetStandardValue() => VectorManager.standardValue;

        public override void SetStandardValue(Vector3 newStandardValue) => VectorManager.SetNewStandardValue(newStandardValue);

        public override Vector3 GetValue() => VectorManager.value;

        public override void SetValue(Vector3 newValue) => VectorManager.SetNewValue(newValue);

        public override void SetGridManager(GridManager gridManager) => VectorManager.gridManager = gridManager;
    };
    public record MPointManager(PointManager PointManager) : Transformable()
    {
        public override GameObject gameObject => PointManager.gameObject;
        
        public override Vector3 GetStandardValue() => PointManager.standardValue;

        public override void SetStandardValue(Vector3 newStandardValue) => PointManager.SetNewStandardValue(newStandardValue);

        public override Vector3 GetValue() => PointManager.value;

        public override void SetValue(Vector3 newValue) => PointManager.SetNewValue(newValue);

        public override void SetGridManager(GridManager gridManager) => PointManager.RefreshGridManager(gridManager);
    };

    public record MPointSnapConstraint(PointSnapConstraint PointSnapConstraint) : Transformable()
    {
        public override GameObject gameObject => PointSnapConstraint.gameObject;

        public override Vector3 GetStandardValue() => PointSnapConstraint.followStandardValue;

        public override void SetStandardValue(Vector3 newStandardValue) =>
            PointSnapConstraint.SetFollowStandardValue(newStandardValue);

        public override Vector3 GetValue() => PointSnapConstraint.followValue;

        public override void SetValue(Vector3 newValue) => PointSnapConstraint.SetFollowValue(newValue);

        public override void SetGridManager(GridManager gridManager)
        {
        }
    };
    public record NotTransformable(GameObject GameObject) : Transformable()
    {
        public override GameObject gameObject => GameObject.gameObject;
        
        //Ambiguity / deliberation: euclideanGridSize cannot be accounted for...
        public override Vector3 GetStandardValue() => gameObject.transform.localPosition;

        public override void SetStandardValue(Vector3 newStandardValue) =>
            gameObject.transform.localPosition = newStandardValue;

        public override Vector3 GetValue()
        {
            throw new NotImplementedException();
        }

        public override void SetValue(Vector3 newValue)
        {
            throw new NotImplementedException();
        }

        public override void SetGridManager(GridManager gridManager)
        {
        }
    };

    public record MPartialGridManager(GameObject GameObject, GridManager GridManager) : NotTransformable(GameObject);


    public static readonly Type[] validTypes = new[] {typeof(VectorManager), typeof(PointManager), typeof(PointSnapConstraint)};
    
    private Transformable(){}
}


// This class manages grids, including its basis, which gameobject serves as the GridlineParent, and which gameobject serves
// as the VectorParent. Grids have a euclideanGridScale, which reflects how many centimeters long one unit of the grid is, when
// the basis is the "standard euclidean" basis. Grids also contain a tMatrix, which translates the positions of the basis vectors
// into an actual matrix. (Since we currently work with 2d grids, only the first 2 columns of the matrix are ever modified)
// When linearMap is true, all objects in toTransform will be attempted to be tranformed with the current value of tMatrix.
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
    [HideInInspector] public float gridLength = 100; // Represents how long each gridline is in centimeters
    [HideInInspector] public float euclideanGridScale = 2.5f;

    private VectorManager[] _vectorManagers;
    private Transformable[] _transformables;
    private int dimensions = 2;

    private void Start()
    {
        Reset();
    }

    // This method scans gameObjects for attached managers and constraints to see if they are "transformable" by the grid.
    public static List<Transformable> createTransformable(GameObject gameObject)
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

        PointSnapConstraint pointSnapConstraint = gameObject.GetComponent<PointSnapConstraint>();
        if (pointSnapConstraint != null)
        {
            transformables.Add(new Transformable.MPointSnapConstraint(pointSnapConstraint));
        }

        PointManager pointManager = gameObject.GetComponent<PointManager>();
        if (pointManager != null)
        {
            transformables.Add(new Transformable.MPointManager(pointManager));
        }

        GridManager gridManager = gameObject.GetComponent<GridManager>();
        if (gridManager != null)
        {
            transformables.Add(new Transformable.MPartialGridManager(gridManager.gameObject, gridManager));
        }

        if (transformables.Count == 0)
        { 
            // throw new Exception("Bruh. No valid types.");
            transformables.Add(new Transformable.NotTransformable(gameObject));
        }
        
        return transformables;
    }

    public void Reset()
    {
        // Make sure that all vectors and points in children have their gridManager set to this grid
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

    // Toggles whether the grid is transformable. When not transformable, points should be able be snap-moved,
    // and moving basis vectors should be disabled. When transformable, the opposite should be true.
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

    // Toggles whether the grid transformation should affect objects in toTransform
    public void ToggleLinearMapStatus()
    {
        linearMap = !linearMap;
    }

    // Updates the position of some gridline according to the current tMatrix and the gridline's
    // own stored positions
    public void SetGridlinePosition(Transform gridline, bool skipRS = true)
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

        if (skipRS)
            return;
                
        // Rotation
        Vector3 pointingDimVec = _vectorManagers[gridlineManager.pointingDim].standardValue;
        gridline.localRotation = Quaternion.FromToRotation(Vector3.up, pointingDimVec);
                
        // Scale
        Vector3 localScale = gridline.localScale;
        localScale.y = gridLength / 2 * pointingDimVec.magnitude;
        gridline.localScale = localScale;
    }

    private void Update()
    {
        if (transformable)
        {
            // Update tMatrix with the current positions of basis vectors
            for(int i = 0; i < dimensions; i++)
            {
                Vector3 basisValue = _vectorManagers[i].standardValue;
                tMatrix[0, i] = basisValue[0];
                tMatrix[1, i] = basisValue[1];
                tMatrix[2, i] = basisValue[2];
            }

            // Update the positions of all gridlines
            foreach (Transform gridline in GridlineParent.transform)
            {
                SetGridlinePosition(gridline, skipRS: false);
            }

            if (linearMap)
            {
                foreach (Transformable transformable in _transformables)
                {
                    switch (transformable)
                    {
                        case Transformable.MVectorManager mVectorManager:
                            VectorManager vectorManager = mVectorManager.VectorManager;
                            // Only update vector if it has no other constraints
                            if (vectorManager.activeConstraints.Count == 0)
                            {
                                vectorManager.SetNewValue(vectorManager.value);
                            }
                            break;
                        case Transformable.MPointManager mPointManager:
                            PointManager pointManager = mPointManager.PointManager;
                            // Only update point if it has no other constraints
                            if (pointManager.activeConstraints.Count == 0)
                            {
                                Debug.Log("Point active!");
                                pointManager.SetNewValue(pointManager.value);   
                            }
                            break;
                        case Transformable.MPointSnapConstraint mPointSnapConstraint:
                            PointSnapConstraint pointSnapConstraint = mPointSnapConstraint.PointSnapConstraint;
                            // Updates position of point's constraint so that the point will be subsequently updated also
                            pointSnapConstraint.SetFollowValue(pointSnapConstraint.followValue);
                            break;
                    }
                }
            }
        }
    }

    public VectorManager[] GetVectorManagers() => _vectorManagers;

    // public void 
}