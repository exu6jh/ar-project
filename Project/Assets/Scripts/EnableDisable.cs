using UnityEngine;

// This class is used in the GameSetup unity scene to enable and disable 
// various game objects within the screen. This is used since there are three 
// primary ‘sub-scenes’ within this scene: general game setup menu, calibration 
// menu, and pre/post test menu. This class provides the utilization to 
// effectively swap between these ‘sub-menus’ by enabling and disabling the 
// appropriate game objects within the scene.
// Author: Everett Xu
public class EnableDisable : MonoBehaviour
{

    public static bool done = false;

    // This is an array of unity scene game objects to enable when a certain
    // action or button press occurs
    public GameObject[] toEnable;

    // This is an array of unity scene game objects to disable when a certain
    // action or button press occurs
    public GameObject[] toDisable;

    public bool autoEnable;

    private void Start()
    {
        if (autoEnable && done == false)
        {
            EnableAll();
            done = true;
        }
    }

    // This method iterates through the toEnable array and activates that
    // unity scene game object. This is the method that gets attached to a
    // specific button/action.
    public void EnableAll() {
        foreach(GameObject g in this.toEnable) {
            g.SetActive(true);
        }
    }

    // This method iterates through the toDisable array and activates that
    // unity scene game object. This is the method that gets attached to a
    // specific button/action.
    public void DisableAll() {
        foreach(GameObject g in this.toDisable) {
            g.SetActive(false);
        }
    }
}