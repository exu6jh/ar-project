using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionHolder : MonoBehaviour
{
  public float time;
  public string command;

  public ActionHolder(float time, string command)
  {
    this.time = time;
    this.command = command;
  }
}
