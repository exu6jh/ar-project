#if (UNITY_EDITOR)
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private float gridLength = 100; // cm
    private float newGridScale = 5; // cm

    private void Awake()
    {
        GridManager gridManager = (GridManager) target;
        gridManager.gridLength = gridManager.GridlineParent.GetComponent<RectTransform>().sizeDelta.x;
        gridLength = gridManager.gridLength;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GridManager gridManager = (GridManager) target;

        // EditorGUILayout.ObjectField("T Matrix (debug only)", (UnityEngine.Object) gridManager.tMatrix, typeof(Matrix4x4), true, null);

        EditorGUILayout.FloatField("Current Grid Length", gridManager.gridLength);
        
        gridLength = EditorGUILayout.FloatField("New Grid Length", gridLength);

        if (GUILayout.Button("Set New Grid Length"))
        {
            // gridManager.Reset();

            
            Vector2 newSizeDelta = new Vector2(gridLength, gridLength);
            
            gridManager.GetComponent<RectTransform>().sizeDelta = newSizeDelta * 10;
            gridManager.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = newSizeDelta;
            gridManager.GridlineParent.GetComponent<RectTransform>().sizeDelta = newSizeDelta;
            gridManager.VectorParent.GetComponent<RectTransform>().sizeDelta = newSizeDelta;

            foreach (Transform gridline in gridManager.GridlineParent.transform)
            {
                Vector3 localScale = gridline.localScale;
                localScale.y = gridLength / 2;
                gridline.localScale = localScale;
            }
            
            gridManager.gridLength = gridLength;
            
        }
        
        EditorGUILayout.FloatField("Current Grid Scale", gridManager.euclideanGridScale);
        
        newGridScale = EditorGUILayout.FloatField("New Grid Scale", newGridScale);

        if (GUILayout.Button("Set New Grid Scale"))
        {
            // gridManager.Reset();
            
            gridManager.euclideanGridScale = newGridScale;

            foreach (Transform gridline in gridManager.GridlineParent.transform)
            {
                gridManager.SetGridlinePosition(gridline);
            }

            // No longer necessary because of VectorEndpointConstraints
            // The below will be necessary for all non-constrained Vectors tho
            // Haha oops it is necessary, forgot the difference between the different parameters
            // to control scaling...
            
            foreach (Transform vector in gridManager.VectorParent.transform)
            {
                VectorManager vectorManager = vector.GetComponent<VectorManager>();
            
                Transform cylinder = vectorManager.TransformCylinder.transform.GetChild(0);
            
                Vector3 localPosition = cylinder.localPosition;
                Vector3 localScale = cylinder.localScale;
            
                localPosition.y = gridManager.euclideanGridScale / 2;
                localScale.y = gridManager.euclideanGridScale / 2;
            
                cylinder.localPosition = localPosition;
                cylinder.localScale = localScale;
            
                Transform tCone = vectorManager.TransformCone.transform;
                Transform cone = tCone.GetChild(0);
            
                Vector3 tConeLocalScale = tCone.localScale;
                Vector3 coneLocalScale = cone.localScale;
            
                tConeLocalScale.y = gridManager.euclideanGridScale;
                // coneLocalScale.y = 0.5f / gridManager.euclideanGridScale;
                coneLocalScale.y = coneLocalScale.x / 1.2f / gridManager.euclideanGridScale;
            
                tCone.localScale = tConeLocalScale;
                cone.localScale = coneLocalScale;
            }
        }

        if (GUILayout.Button(gridManager.transformable ? "Disable Transformable" : "Enable Transformable"))
        {
            gridManager.ToggleTransformableStatus();
        }

        if (GUILayout.Button(gridManager.linearMap ? "Disable Linear Map" : "Enable Linear Map"))
        {
            gridManager.ToggleLinearMapStatus();
        }

        if (GUILayout.Button("Reset"))
        {
            gridManager.Reset();
        }
    }
}

#endif // (UNITY_EDITOR)