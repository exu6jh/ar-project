using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldActionHolder : MonoBehaviour
{
    public float time;
    public string command;

    public OldActionHolder(float time, string command) {
        this.time = time;
        this.command = command;
    }
}
