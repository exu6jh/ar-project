using UnityEngine;
using UnityEngine.SceneManagement;

// This class defines the various methods and parameters related to changing
// unity scenes during the game.
public class ChangeScene : MonoBehaviour
{
    public static void GoToScene(SCENES scene) => SceneManager.LoadScene((int)scene);
    
    
    // The GoToScene methods change the scene to that which is passed into it
    public void GoToScene(GetScene getScene) => GoToScene(getScene.sceneToLoad);

}
