﻿using UnityEngine;
using UnityEngine.Serialization;

public class GridManager : MonoBehaviour
{
    public GameObject GridlineParent;
    public GameObject BasisVectorParent;
    public GameObject origin;

    [HideInInspector] public float euclideanGridScale = 2.5f;
}