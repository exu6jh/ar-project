using System;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class PointManager : MonoBehaviour
{
    public GridManager gridManager;

    public TMP_Text text;

    public Vector3 value;

    private Vector3 textPos;

    private void Start()
    {
        textPos = text.transform.localPosition;
    }
    
    private void Update()
    {
        value = transform.localPosition / gridManager.euclideanGridScale;
        
        text.text = $"({value.x:0},{value.y:0})";

        textPos.x = 1.75f * ((value.x < 0) ? -1 : 1);
        textPos.y = 1.00f * ((value.y < 0) ? -1 : 1);
        text.transform.localPosition = textPos;
        
        // Move text code below to Update
    }

    public void SetNewPosition(Vector3 newPosition)
    {
        value = newPosition;
        
        text.text = $"({newPosition.x:0},{newPosition.y:0})";

        textPos.x = 1.75f * ((newPosition.x < 0) ? -1 : 1);
        textPos.y = 1.00f * ((newPosition.y < 0) ? -1 : 1);
        text.transform.localPosition = textPos;
        
        newPosition *= gridManager.euclideanGridScale;
        transform.localPosition = newPosition;
    }
}