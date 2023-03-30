// Scenes available as specified in build settings

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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

// public abstract record QuizQnState
// {
//     
// }
//
// public record VectorQuizQnState(Vector3 value) : QuizQnState()
// {
//     
// }
//
// public record SliderQnState(double value) : QuizQnState()
// {
//     
// }

public abstract class QuizQnState
{
    public abstract QuizQnState duplicate();

    public abstract bool Equals(QuizQnState qnState);
}

public class VectorQuizQnState : QuizQnState
{
    public Vector3 value;

    public VectorQuizQnState()
    {
        this.value = new Vector3(1, 1);
    }

    public VectorQuizQnState(Vector3 value)
    {
        this.value = value;
    }

    public override QuizQnState duplicate()
    {
        return new VectorQuizQnState();
    }

    public override bool Equals(QuizQnState qnState)
    {
        if (qnState is VectorQuizQnState cState)
        {
            // VectorQuizQnState cState = qnState as VectorQuizQnState;
            
            // For some reason, unity overloads == with approx. equals...
            return value == cState.value;
        }
        else
        {
            return false;
        }
    }
}

public class SliderQnState : QuizQnState
{
    public double value;

    public SliderQnState()
    {
        this.value = 0.2;
    }

    public SliderQnState(double value)
    {
        this.value = value;
    }

    public override QuizQnState duplicate()
    {
        return new SliderQnState();
    }

    public override bool Equals(QuizQnState qnState)
    {
        if (qnState is SliderQnState cState)
        {
            // VectorQuizQnState cState = qnState as VectorQuizQnState;

            return Math.Abs(value - cState.value) < 1e-8;
        }

        else
        {
            return false;
        }
    }
}

public record QuizQnGrade
{
    public string questionName;
    public int questionNumber;
    public bool correct;

    public QuizQnGrade(string questionName, int questionNumber, bool correct)
    {
        this.questionName = questionName;
        this.questionNumber = questionNumber;
        this.correct = correct;
    }
}

public class QuizState
{
    public List<QuizQnState> quizKey;
    public List<QuizQnState> quizQnStates;
    public List<string> quizQnNames;
    
    

    public QuizState(List<QuizQnState> quizKey, List<string> quizQnNames)
    {
        this.quizKey = quizKey;

        this.quizQnStates = (from keyState in this.quizKey
            select keyState.duplicate()).ToList();

        this.quizQnNames = quizQnNames;
    }

    // public void setStateAt(int pos, QuizQnState qnState)
    // {
    //     quizStates[pos] = qnState;
    // }

    public QuizQnState getStateAt(int pos)
    {
        return quizQnStates[pos];
    }

    
    public List<QuizQnGrade> grade()
    { 
        List<QuizQnGrade> grades = new List<QuizQnGrade>(quizKey.Count);

        for (int i = 0; i < quizKey.Count; i++)
        {
            grades[i] = new QuizQnGrade(quizQnNames[i], i, quizKey[i].Equals(quizQnStates[i]));
        }
        
        // So ugly...
        // grades = Enumerable.Zip(quizKey, quizQnStates,
        //     (state, qnState) => new QuizQnGrade(state.Equals(qnState))).ToList();
        //
        // foreach (QuizQnGrade grade in grades)
        // {
        //     
        // }

        return grades;
    }
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
        // this.session.SetActiveScene(this);
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

public class QuizIntroScene : ArbitraryScene
{
    // public List<QuizQnState> quizKey;
    // public List<string> quizQnNames;
    public QuizState quizState;
    
    public QuizIntroScene(string publicName, SCENES scene) : base(publicName, scene)
    {
    }

    public QuizIntroScene(string publicName, SCENES scene, QuizState quizState) : base(publicName, scene)
    {
        this.quizState = quizState;
    }

    public override void GoToScene()
    {
        session.activeQuizStates = quizState;
        base.GoToScene();
    }
}

public class QuizQnScene : ArbitraryScene
{
    public QuizQnState qnState;
    public string quizQnText;
    public int quizQnNum;
    
    public QuizQnScene(string publicName, SCENES scene, string quizText) : base(publicName, scene)
    {
        this.quizQnText = quizText;
    }

    public QuizQnScene(string publicName, SCENES scene, string quizText, int quizNum) : base(publicName, scene)
    {
        this.quizQnText = quizText;
        this.quizQnNum = quizNum;
    }

    public override void GoToScene()
    {
        // session.activeQuizStates[quizNum] = QuizQnState;
        session.activeQuizQnText = quizQnText;
        session.activeQuizQnNum = quizQnNum;
        base.GoToScene();
    }
}

public class QuizSubmitScene : ArbitraryScene
{
    public QuizSubmitScene(string publicName, SCENES scene) : base(publicName, scene)
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
        // this.session.SetActiveScene(this);
        ChangeScene.GoToScene(SCENES.REVIEW);
    }
}

public class SessionSceneListBuilder
{
    private List<SessionScene> scenes = new List<SessionScene>();
    private List<QuizQnScene> quizQnScenes = new List<QuizQnScene>();
    private List<QuizQnState> quizKey = new List<QuizQnState>();
    private int quizCounter = 1;

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

    public SessionSceneListBuilder AddQuizQn(string qnName, SCENES qnScene, QuizQnState qnState, string qnText)
    {
        quizQnScenes.Add(new QuizQnScene(qnName, qnScene, qnText, quizCounter++));
        quizKey.Add(qnState);
        return this;
    }

    public SessionSceneListBuilder AddWholeQuiz(string quizName, SCENES scene)
    {
        QuizState quizState = new QuizState(quizKey,
            (from qnScene in quizQnScenes
                select qnScene.publicName).ToList());
        scenes.Add(new QuizIntroScene(quizName, scene, quizState));
        Enumerable.Zip(quizQnScenes, quizState.quizQnStates, (qnScene, state) =>
        {
            qnScene.qnState = state;
            scenes.Add(qnScene);
            return 0;
        });
        quizQnScenes.Clear();
        quizKey = new List<QuizQnState>();
        return this;
    }

    public List<SessionScene> getList() => scenes;
}

public class Session
{
    public string name;
    private List<SessionScene> scenes;
    public int scenePosition;
    
    // Eventually, ALL of the below should be replaced with just public SessionScene activeScene!
    public SessionScene activeScene;
    
    public List<SessionScene> reviewScenes;
    public bool review;
    
    // public QuizQnState activeQuizState;
    public string activeQuizQnText;
    public QuizState activeQuizStates;
    public int activeQuizQnNum;
    
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

    public void GoToSceneAt(int scenePos)
    {
        DataManager.Instance.AddNewDataEntry(scenes[scenePos].publicName);
        scenes[scenePos].GoToScene();
    } 

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
        // Debug.Log(Globals.activeSession);
        scenePosition = 0;
        GoToSceneAt(scenePosition);
    }

    public void SetReviewScenes(List<SessionScene> reviewScenes)
    {
        this.reviewScenes = reviewScenes;
    }

    public void SetActiveScene(SessionScene newActiveScene)
    {
        this.activeScene = newActiveScene;
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
                .AddQuizQn("y-component", SCENES.QUIZ, new SliderQnState(), "What is your favorite color?")
                .AddWholeQuiz("quiz!", SCENES.MENU)
                // .AddScene(new QuizIntroScene("quizIntro", SCENES.QUIZ))
                // .AddScene(new QuizQnScene("quiz1", SCENES.QUIZ, new SliderQnState(), "What is your favorite color?"))
            )
        };

        activeSession = defaultSessions[0];
        
        // DEBUG
        // Debug.Log(Globals.activeSession);
        // activeSession.scenePosition = 1;    
    }
}