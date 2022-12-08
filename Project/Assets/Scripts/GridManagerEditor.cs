﻿#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{

    private float newGridScale = 5;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GridManager gridManager = (GridManager) target;
        
        EditorGUILayout.FloatField("Current Grid Scale", gridManager.euclideanGridScale);
        
        newGridScale = EditorGUILayout.FloatField("New Grid Scale", newGridScale);

        if (GUILayout.Button("Set New Grid Scale"))
        {
            gridManager.euclideanGridScale = newGridScale;

            foreach (Transform gridline in gridManager.GridlineParent.transform)
            {
                GridlineManager gridlineManager = gridline.GetComponent<GridlineManager>();
                Vector3 position = Vector3.zero;
                foreach (GridMovement gridMovement in gridlineManager.movement)
                {
                    position[gridMovement.dimension] = gridMovement.offset * gridManager.euclideanGridScale;
                }

                gridline.localPosition = position;
            }

            // No longer necessary because of VectorEndpointConstraints
            // The below will be necessary for all non-constrained Vectors tho
            // Haha oops it is necessary, forgot the difference between the different parameters
            // to control scaling...
            
            foreach (Transform basisVector in gridManager.BasisVectorParent.transform)
            {
                BasisVectorManager basisVectorManager = basisVector.GetComponent<BasisVectorManager>();
            
                Transform cylinder = basisVectorManager.TransformCylinder.transform.GetChild(0);
            
                Vector3 localPosition = cylinder.localPosition;
                Vector3 localScale = cylinder.localScale;
            
                localPosition.y = gridManager.euclideanGridScale / 2;
                localScale.y = gridManager.euclideanGridScale / 2;
            
                cylinder.localPosition = localPosition;
                cylinder.localScale = localScale;
            
                Transform tCone = basisVectorManager.TransformCone.transform;
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
    }
}

#endif // (UNITY_EDITOR)