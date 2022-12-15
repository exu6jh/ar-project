using UnityEngine;

// This class serves as a wrapper to hold the next scene to load, associated
// with various buttons in a scene. This works directly with the ChangeScene.cs 
// script to facilitate the changing of scenes.
public class GetScene : MonoBehaviour
{
    // The associated scene to be loaded
    public SCENES sceneToLoad;
}
