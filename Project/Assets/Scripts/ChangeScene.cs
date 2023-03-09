using UnityEngine;
using UnityEngine.SceneManagement;

// This class defines the various methods and parameters related to changing
// unity scenes during the game.
public class ChangeScene : MonoBehaviour
{
    // A number of methods to change the scene to that which is passed into it
    public static void GoToScene(SCENES scene) => SceneManager.LoadSceneAsync((int)scene);
    public void GoToScene(GetScene getScene) => GoToScene(getScene.sceneToLoad);

    // A number of methods to set the global lesson variable.
    public static void SetLesson(string text) => Globals.lesson = text;
    public void SetLesson(LESSONS lessons) => SetLesson(Globals.LessonEnumToString(lessons));
    public void SetLesson(GetScene getScene) => SetLesson(getScene.lessonToLoad);


    public void PrevSessionScene()
    {
        Globals.activeSession.GoToPreviousScene();
    }
    
    public void NextSessionScene()
    {
        Globals.activeSession.GoToNextScene();
    }

    public void FirstSessionScene()
    {
        Globals.activeSession.GoToFirstScene();
    }

    public void ExitReview()
    {
        Globals.activeSession.review = false;
    }
}
