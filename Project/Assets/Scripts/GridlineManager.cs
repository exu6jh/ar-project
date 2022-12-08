using System;
using UnityEngine;

[Serializable]
public class GridMovement
{
    public int dimension;
    public int offset; // coordinate
}

public class GridlineManager : MonoBehaviour
{
    public GridMovement[] movement;
}