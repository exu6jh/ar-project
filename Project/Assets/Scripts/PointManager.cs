using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        textPos.x = 1.75f * ((value.x < 0) ? -1 : 1);
        textPos.y = 1.00f * ((value.y < 0) ? -1 : 1);
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