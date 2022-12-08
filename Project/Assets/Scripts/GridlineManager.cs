using System;
using UnityEngine;

[Serializable]
public class GridMovement
{
    public int dimension;
    public int offset; // coordinate

    public GridMovement(int dimension, int offset)
    {
        this.dimension = dimension;
        this.offset = offset;
    }

    public GridMovement Clone()
    {
        return new GridMovement(dimension, offset);
    }
}

public class GridlineManager : MonoBehaviour
{
    public GridMovement[] movement;
}