using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoDestory : MonoBehaviour
{
    void Awake (){
        DontDestroyOnLoad(this.gameObject);
    }
}
