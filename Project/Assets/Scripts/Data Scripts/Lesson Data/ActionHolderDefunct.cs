using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionHolderDefunct : MonoBehaviour
{
  public float time;
  public string command;

  public ActionHolderDefunct(float time, string command)
  {
    this.time = time;
    this.command = command;
  }
}
