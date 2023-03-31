﻿using System;
using UnityEngine;

public class ReviewEnabler : MonoBehaviour
{
    public bool enableOnReviewOrFlip;
    private void Start()
    {
        // if (Globals.activeSession.review != enableOnReviewOrFlip)
        // {
        //     gameObject.SetActive(false);
        // }
        
        if ((Globals.activeSession.GetNestedActiveScene().closestAncestor<ReviewScene>() != null) != enableOnReviewOrFlip)
        {
            gameObject.SetActive(false);
        }
    }
}