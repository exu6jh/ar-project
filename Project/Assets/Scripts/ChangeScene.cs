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
        if (!Globals.activeSession.GoToPreviousScene())
        {
            Globals.activeSession.GoToMenuScene();
        }
    }
    
    public void NextSessionScene()
    {
        if (!Globals.activeSession.GoToNextScene())
        {
            Globals.activeSession.GoToMenuScene();
        }
    }

    public void EnterSessionScene()
    {
        Globals.activeSession.EnterSessionScene();
    }

    public void FirstSessionScene()
    {
        Globals.activeSession.GoToFirstScene();
    }

    public void SessionMenuScene()
    {
        Globals.activeSession.GoToMenuScene();
    }

    public void ExitReview()
    {
        Globals.activeSession.GetNestedActiveScene().closestAncestor<ReviewScene>()?.resetActiveReview();
    }
}
