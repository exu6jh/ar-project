// Scenes available as specified in build settings

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SCENES
{
    MENU,
    LESSON,
    TWO_GRIDS,
    REVIEW,
    QUIZ,
    TEMPLATE
}

// Lessons avilable
public enum LESSONS
{
    LESSON_1,
    LESSON_2,
    // LESSON_3,
    // LESSON_4,
}

public abstract class SessionScene
{
    public string publicName;
    public Session session;

    protected SessionScene(string publicName)
    {
        this.publicName = publicName;
    }

    public abstract void GoToScene();
}

public class ArbitraryScene : SessionScene
{
    public SCENES scene;

    public ArbitraryScene(string publicName, SCENES scene) : base(publicName)
    {
        this.scene = scene;
    }

    public override void GoToScene()
    {
        ChangeScene.GoToScene(scene);
    }
}

public class LessonScene : SessionScene
{
    public string fileName;

    public LessonScene(string fileName) : base(fileName)
    {
        this.fileName = fileName;
    }

    public LessonScene(string publicName, string fileName) : base(publicName)
    {
        this.fileName = fileName;
    }

    public override void GoToScene()
    {
        ChangeScene.SetLesson(fileName);
        ChangeScene.GoToScene(SCENES.LESSON);
    }
}

public class SandboxScene : ArbitraryScene
{
    public SandboxScene(string publicName, SCENES scene) : base(publicName, scene)
    {
    }
}

public class QuizScene : ArbitraryScene
{
    public QuizScene(string publicName, SCENES scene) : base(publicName, scene)
    {
    }
}

public class ReviewScene : SessionScene
{
    public List<SessionScene> reviewScenes;

    public ReviewScene(string publicName) : base(publicName)
    {
    }

    public ReviewScene(string publicName, List<SessionScene> reviewScenes) : base(publicName)
    {
        this.reviewScenes = reviewScenes;
    }

    public override void GoToScene()
    {
        this.session.review = true;
        this.session.SetReviewScenes(reviewScenes);
        ChangeScene.GoToScene(SCENES.REVIEW);
    }
}

public class SessionSceneListBuilder
{
    private List<SessionScene> scenes = new List<SessionScene>();

    public SessionSceneListBuilder AddScene(SessionScene newScene)
    {
        Debug.Log($"Adding scene {newScene.publicName}");
        scenes.Add(newScene);
        return this;
    }

    public SessionSceneListBuilder AddReviewScene(string publicName, int[] sceneIndices)
    {
        scenes.Add(new ReviewScene(publicName, (from index in sceneIndices
            select scenes[index]).ToList()));
        return this;
    }

    public SessionSceneListBuilder AddReviewScene(string publicName, List<int> sceneIndices)
    {
        scenes.Add(new ReviewScene(publicName, (from index in sceneIndices
            select scenes[index]).ToList()));
        return this;
    }

    public List<SessionScene> getList() => scenes;
}

public class Session
{
    public string name;
    private List<SessionScene> scenes;
    public int scenePosition;
    public List<SessionScene> reviewScenes;
    public bool review;
    
    public Session(string name, SessionSceneListBuilder builder)
    {
        this.name = name;
        this.scenes = builder.getList();
        scenePosition = 0;
        review = false;

        foreach (SessionScene scene in scenes)
        {
            scene.session = this;
        }
    }

    public Session(string name, List<SessionScene> scenes)
    {
        this.name = name;
        this.scenes = scenes;
        scenePosition = 0;
        review = false;

        foreach (SessionScene scene in scenes)
        {
            scene.session = this;
        }
    }

    public void GoToSceneAt(int scenePos) => scenes[scenePos].GoToScene();

    public void GoToPreviousScene()
    {
        scenePosition--;
        if (scenePosition < 0)
        {
            scenePosition = scenes.Count - 1;
            ChangeScene.GoToScene(SCENES.MENU);
        }
        else
        {
            GoToSceneAt(scenePosition);
        }
    }

    public void GoToNextScene()
    {
        scenePosition++;
        if (scenePosition >= scenes.Count)
        {
            scenePosition = 0;
            ChangeScene.GoToScene(SCENES.MENU);
        }
        else
        {
            GoToSceneAt(scenePosition);
        }
    }

    public void GoToFirstScene()
    {
        scenePosition = 0;
        GoToSceneAt(scenePosition);
    }

    public void SetReviewScenes(List<SessionScene> reviewScenes)
    {
        this.reviewScenes = reviewScenes;
    }
}

public static class Globals
{
    public static string lesson;

    public static List<Session> defaultSessions;

    public static Session activeSession;

    // Convert LESSONS to lesson text file name
    public static string LessonEnumToString(LESSONS lessons)
    {
        switch (lessons)
        {
            case LESSONS.LESSON_1:
                return "lesson1.txt";
            case LESSONS.LESSON_2:
                return "lesson2.txt";
            default:
                return "lesson1.txt";
        }
    }

    static Globals()
    {
        Debug.Log("Hi, initializing globals");
        defaultSessions = new List<Session>()
        {
            new ("Lesson 1", new SessionSceneListBuilder()
                .AddScene(new LessonScene("lesson1.txt"))
                .AddScene(new SandboxScene("sandbox1", SCENES.TWO_GRIDS)) // For now, needs to be changed...
                .AddReviewScene("review1", new[] {0, 1})
                .AddScene(new QuizScene("quiz1", SCENES.QUIZ))
            )
        };

        activeSession = defaultSessions[0];
        
        // DEBUG
        // activeSession.scenePosition = 1;    
    }
}