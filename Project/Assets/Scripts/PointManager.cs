using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


// This class manages points. It updates its standardValue (with standard basis) and value (according to the current
// gridManager tMatrix), as well as updates the text label with the current value. In the future, there could be a toggle
// For points to display their components in either standardValue or value.
[ExecuteInEditMode]
public class PointManager : MonoBehaviour
{
    public GridManager gridManager;
    
    public readonly HashSet<MonoBehaviour> activeConstraints = new HashSet<MonoBehaviour>();

    public TMP_Text text;

    public Vector3 standardValue;
    public Vector3 value;

    private Vector3 textPos;

    private void Start()
    {
        textPos = text.transform.localPosition;
    }
    
    private void Update()
    {
        standardValue = transform.localPosition / gridManager.euclideanGridScale;
        value = gridManager.tMatrix.inverse * standardValue;
        
        text.text = $"({value.x:0.##},{value.y:0.##})";

        textPos.x = 5f * ((value.x < 0) ? -1 : 1);
        textPos.y = 10f * ((value.y < 0) ? -1 : 1);
        text.transform.localPosition = textPos;
        
        // Move text code below to Update
    }

    public void GetStandardValue()
    {
        // transform.localPosition / gridManager.euclideanGridScale;
    }

    public void SetNewStandardValue(Vector3 newStandardValue)
    {
        standardValue = newStandardValue;
        value = gridManager.tMatrix.inverse * newStandardValue;
        
        newStandardValue *= gridManager.euclideanGridScale;
        transform.localPosition = newStandardValue;
    }

    public void SetNewValue(Vector3 newValue) => SetNewStandardValue(gridManager.tMatrix * newValue);
}