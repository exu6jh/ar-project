using UnityEngine;
using UnityEngine.SceneManagement;

// This class defines the various methods and parameters related to changing
// unity scenes during the game.
public class ChangeScene : MonoBehaviour
{
    // A number of methods to change the scene to that which is passed into it
    public static void GoToScene(SCENES scene) => SceneManager.LoadScene((int)scene);
    public void GoToScene(GetScene getScene) => GoToScene(getScene.sceneToLoad);

    // A number of methods to set the global lesson variable.
    public void SetLesson(string text) => Globals.lesson = text;
    public void SetLesson(LESSONS lessons) => SetLesson(Globals.LessonEnumToString(lessons));
    public void SetLesson(GetScene getScene) => SetLesson(getScene.lessonToLoad);
}
