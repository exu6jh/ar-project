using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAndDeactivate : MonoBehaviour
{
    public GameObject[] toBeActivated;
    public GameObject[] toBeDeactivated;

    public void Activate() {
        for(int i = 0; i < toBeActivated.Length; i++) {
            toBeActivated[i].SetActive(true);
        }
    }

    public void Deactivate() {
        for(int i = 0; i < toBeDeactivated.Length; i++) {
            toBeDeactivated[i].SetActive(false);
        }
    }
}
