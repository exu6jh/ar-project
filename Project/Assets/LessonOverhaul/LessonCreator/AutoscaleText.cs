using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutoscaleText : MonoBehaviour
{
    public float scaleFactor;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_Text>().fontSize = scaleFactor * Screen.width;
    }
}
