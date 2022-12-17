#if (UNITY_EDITOR)

using UnityEngine;

// This class allows for the easy creation of new gridlines while working in the Unity Editor.
// The corresponding GridlineCreatorEditor class adds in UI (button) and functionality to make sure
// that new gridlines fit in with the current settings of the Grid.
public class GridlineCreator : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject gridlinePrefab;
    public GridMovement[] newGridMovements;
    public int newPointingDim;
    
}

#endif // (UNITY_EDITOR)