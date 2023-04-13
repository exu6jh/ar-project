using System;
using UnityEngine;


// This class represents the position of a gridline in one particular dimension
// This position is made explicit and immutable so that changes
// to the basis of a grid can easily to changes in the position and
// rotation of a Gridline
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
        return new(dimension, offset);
    }
}

// This class contains information about a gridline, including its position in
// multiple dimensions, as well as the dimension that the line should point along
public class GridlineManager : MonoBehaviour
{
    public GridMovement[] movement;
    public int pointingDim;
}