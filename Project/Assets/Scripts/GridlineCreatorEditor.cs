#if (UNITY_EDITOR)

using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridlineCreator))]
public class GridlineCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create New Gridline"))
        {
            GridlineCreator gridlineCreator = (GridlineCreator) target;

            // worldPositionStays is set to false to keep scale / thickness (but this can also easily be prog-set)
            GameObject newGridline = PrefabUtility.InstantiatePrefab(gridlineCreator.gridlinePrefab) as GameObject;
            Transform newTransform = newGridline.transform;
            newTransform.SetParent(gridlineCreator.gridManager.GridlineParent.transform, false);
            
            
            GridlineManager gridlineManager = newGridline.GetComponent<GridlineManager>();
            gridlineManager.movement = (from gridMovement in gridlineCreator.newGridMovements
                                        select gridMovement.Clone()).ToArray();
            
            // Set rotation
            // Does not handle general case, only 2d case right now
            GridMovement onlyGridMovement = gridlineManager.movement[0];
            if (onlyGridMovement.dimension == 1)
            {
                newTransform.rotation = Quaternion.FromToRotation(Vector3.up, Vector3.right);
            }
            
            // Set position
            StringBuilder gridlineName = new StringBuilder("Grid Line", 20);
            Vector3 position = Vector3.zero;
            foreach (GridMovement gridMovement in gridlineManager.movement)
            {
                position[gridMovement.dimension] = gridMovement.offset * gridlineCreator.gridManager.euclideanGridScale;
                gridlineName.AppendFormat(" ({0},{1})", gridMovement.dimension, gridMovement.offset);
            }

            newTransform.localPosition = position;
            newGridline.name = gridlineName.ToString();
        }
    }
}

#endif // (UNITY_EDITOR)