// Scenes available as specified in build settings

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SCENES
{
    MENU,
    LESSON,
    TWO_GRIDS,
    REVIEW,
    QUIZ_INSTRUCTIONS,
    QUIZ_1,
    QUIZ_2,
    QUIZ_3,
    QUIZ_4,
    QUIZ_5,
    QUIZ_6,
    // QUIZ_7,
    QUIZ_8,
    QUIZ_9,
    QUIZ_10,
    QUIZ_SUBMIT,
    SANDBOX,
    _3D_LIGHT_TEST,
    CUSTOM_LESSON,
    CREATE_MENU,
    CREATE,
    TEMPLATE,
    QUIZ,
    QUIZ_TEST
}

// Lessons avilable
public enum LESSONS
{
    LESSON_1,
    LESSON_2,
    CUSTOM_1,
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

public class DiscreteQnState : QuizQnState
{
    public int value;

    public DiscreteQnState()
    {
        this.value = 0;
    }

    public DiscreteQnState(int value)
    {
        this.value = value;
    }

    public override QuizQnState duplicate()
    {
        return new DiscreteQnState();
    }

    public override bool Equals(QuizQnState qnState)
    {
        if (qnState is DiscreteQnState cState)
        {
            return value == cState.value;
        }
        else
        {
            return false;
        }
    }
}

public class VectorQnState : QuizQnState
{
    public Vector3 value;

    public VectorQnState()
    {
        this.value = new Vector3(1, 1);
    }

    public VectorQnState(Vector3 value)
    {
        this.value = value;
    }

    public override QuizQnState duplicate()
    {
        return new VectorQnState();
    }

    public override bool Equals(QuizQnState qnState)
    {
        if (qnState is VectorQnState cState)
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
    public float value;

    public SliderQnState()
    {
        this.value = 0.5f;
    }

    public SliderQnState(float value)
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
            // return Math.Abs(value - cState.value) < 1e-8;
            return Math.Abs(value - cState.value) < 1e-1;
        }

        else
        {
            return false;
        }
    }
}

public class TwoSliderQnState : QuizQnState
{
    public float value1, value2;

    public TwoSliderQnState()
    {
        // (this.value1, this.value2) = (0.5f, 0.5f);
        this.value1 = 0.5f;
        this.value2 = 0.5f;
    }

    public TwoSliderQnState(float value1, float value2)
    {
        // (this.value1, this.value2) = (value1, value2);
        this.value1 = value1;
        this.value2 = value2;
    }

    public override QuizQnState duplicate()
    {
        return new TwoSliderQnState();
    }

    public override bool Equals(QuizQnState qnState)
    {
        if (qnState is TwoSliderQnState cState)
        {
            return Math.Abs(value1 - cState.value1) < 1e-1 && Math.Abs(value2 - cState.value2) < 1e-1;
        }

        else
        {
            return false;
        }
    }
}

public class CompositeQnState : QuizQnState
{
    public List<QuizQnState> value;

    public CompositeQnState(List<QuizQnState> value)
    {
        this.value = value;
    }

    public override QuizQnState duplicate()
    {
        return new CompositeQnState((from state in value
            select state.duplicate()).ToList());
    }

    public override bool Equals(QuizQnState qnState)
    {
        if (qnState is CompositeQnState cState && value.Count == cState.value.Count)
        {
            return Enumerable.Zip(value, cState.value, (state, qnState) => state.Equals(qnState))
                .All(b => b);
            // return Enumerable.Zip(value, cState.value, (state, qnState) => (state, qnState))
            //     .All(((QuizQnState state, QuizQnState qnState) _) => _.state.Equals(_.qnState));
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
            grades.Add(new QuizQnGrade(quizQnNames[i], i, quizKey[i].Equals(quizQnStates[i])));
            Debug.Log($"{i}, {(quizKey[i] is SliderQnState a ? a.value.ToString("N10") : "not a slider!")}, {(quizQnStates[i] is SliderQnState b ? b.value.ToString("N10") : "not a slider!")}");
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

    public SessionScene parentScene()
    {
        return session?.Parent;
    }
    
    public T closestAncestor<T>() where T : SessionScene
    {
        if (this is T tScene)
        {
            return tScene;
        }
        else
        {
            return parentScene()?.closestAncestor<T>();
        }
    }
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

public class QuizIntroScene : ArbitraryScene, AbstractSession
{
    // public List<QuizQnState> quizKey;
    // public List<string> quizQnNames;
    public QuizState quizState;
    public Session quizSession;
    private bool quizzing;
    
    // public QuizIntroScene(string publicName, SCENES scene) : base(publicName, scene)
    // {
    // }

    public QuizIntroScene(string publicName, SCENES scene, QuizState quizState, List<SessionScene> quizScenes) : base(publicName, scene)
    {
        this.quizState = quizState;
        this.quizSession = new Session(publicName, scene, this, quizScenes);
        Debug.Log(quizScenes);
        foreach (SessionScene scenea in quizScenes)
        {
            Debug.Log(scenea.publicName);
        }
    }

    // public override void GoToScene()
    // {
    //     session.activeQuizStates = quizState;
    //     base.GoToScene();
    // }
    
    public bool GoToPreviousScene() => quizzing && (quizSession?.GoToPreviousScene() ?? false);

    public bool GoToNextScene() => quizzing && (quizSession?.GoToNextScene() ?? false);

    public bool GoToMenuScene() => quizzing && (quizSession?.GoToMenuScene() ?? false);
    
    public void GoToMyMenuScene()
    {
        throw new NotImplementedException();
    }

    public void EnterSessionScene()
    {
        quizzing = true;
        quizSession?.EnterSessionScene();
    }

    public SessionScene GetNestedActiveScene() => quizzing ? quizSession?.GetNestedActiveScene() : this;
}

public class QuizQnScene : ArbitraryScene
{
    public string gradeName;
    public QuizQnState qnState;
    public string quizQnText;
    public int quizQnNum;
    
    // public QuizQnScene(string publicName, SCENES scene, string quizText) : base(publicName, scene)
    // {
    //     this.quizQnText = quizText;
    // }

    public QuizQnScene(string gradeName, SCENES scene, string quizText, int quizNum) : base(gradeName, scene)
    {
        this.gradeName = gradeName;
        this.quizQnText = quizText;
        this.quizQnNum = quizNum;
    }

    public QuizQnScene(string publicName, string gradeName, SCENES scene, string quizText, int quizNum) : base(publicName, scene)
    {
        this.gradeName = gradeName;
        this.quizQnText = quizText;
        this.quizQnNum = quizNum;
    }

    // public override void GoToScene()
    // {
    //     // session.activeQuizStates[quizNum] = QuizQnState;
    //     session.activeQuizQnText = quizQnText;
    //     session.activeQuizQnNum = quizQnNum;
    //     base.GoToScene();
    // }
}

public class QuizSubmitScene : ArbitraryScene
{
    public QuizSubmitScene(string publicName, SCENES scene) : base(publicName, scene)
    {
    }
}

public class ReviewScene : SessionScene, AbstractSession
{
    private List<SessionScene> reviewScenes;
    // public SessionScene activeReviewScene;
    private Session reviewSession;
    private bool reviewing;

    public ReviewScene(string publicName, List<SessionScene> reviewScenes) : base(publicName)
    {
        this.reviewScenes = reviewScenes;
    }

    public override void GoToScene()
    {
        // this.session.review = true;
        // this.session.SetReviewScenes(reviewScenes);
        reviewing = false;
        this.reviewSession = new Session(publicName, SCENES.REVIEW, this, reviewScenes);
        // this.session.SetActiveScene(this);
        ChangeScene.GoToScene(SCENES.REVIEW);
    }

    public string GetNameOf(int scenePos)
    {
        return reviewScenes[scenePos].publicName;
    }

    public void GoToReviewSceneAt(int scenePos)
    {
        reviewing = true;
        reviewSession.GoToSceneAt(scenePos);
    }

    public void resetActiveReview()
    {
        reviewing = false;
        foreach (SessionScene scene in reviewScenes)
        {
            scene.session = this.session;
        }
    }
    
    public bool GoToPreviousScene() => reviewing && (reviewSession?.GoToPreviousScene() ?? false);

    public bool GoToNextScene() => reviewing && (reviewSession?.GoToNextScene() ?? false);

    public bool GoToMenuScene() => reviewing && (reviewSession?.GoToMenuScene() ?? false);
    public void GoToMyMenuScene()
    {
        if (reviewing)
            reviewSession?.GoToMyMenuScene();
        else
            this.session.GoToMyMenuScene();
    }

    public void EnterSessionScene()
    {
        reviewing = true;
        reviewSession?.EnterSessionScene();
    }

    public SessionScene GetNestedActiveScene() => reviewing ? reviewSession?.GetNestedActiveScene() : this;
}

public class SessionSceneListBuilder
{
    private List<SessionScene> scenes = new List<SessionScene>();
    private List<SessionScene> quizScenes = new List<SessionScene>();
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
        quizScenes.Add(new QuizQnScene($"Question {quizCounter}", qnName, qnScene, qnText, quizCounter++));
        quizKey.Add(qnState);
        return this;
    }

    public SessionSceneListBuilder AddQuizQnCount(string qnName, SCENES qnScene, QuizQnState qnState, string qnText)
    {
        quizScenes.Add(new QuizQnScene($"Question {quizCounter}", $"{quizCounter}-{qnName}", qnScene, qnText, quizCounter++));
        quizKey.Add(qnState);
        return this;
    }

    public SessionSceneListBuilder AddQuizOtherScene(SessionScene newScene)
    {
        quizScenes.Add(newScene);
        return this;
    }

    public SessionSceneListBuilder AddWholeQuiz(string quizName, SCENES scene)
    {
        Func<IEnumerable<QuizQnScene>> quizFilter = () => 
            from qnScene in quizScenes
            where qnScene is QuizQnScene
            select qnScene as QuizQnScene;
        
        QuizState quizState = new QuizState(quizKey,
            (from qnScene in quizFilter()
                select qnScene.gradeName).ToList());
        
        List<SessionScene> reviewScenes = Enumerable.Zip(
              quizFilter()
            , quizState.quizQnStates
            , (qnScene, state) =>
        {
            qnScene.qnState = state;
            return qnScene as SessionScene;
        }).ToList();

        quizScenes.Add(new ReviewScene(quizName + "review", reviewScenes));
        quizScenes.Add(new QuizSubmitScene(quizName + "submit", SCENES.QUIZ_SUBMIT));
        
        scenes.Add(new QuizIntroScene(quizName, scene, quizState, quizScenes));
        quizScenes = new List<SessionScene>();
        quizKey = new List<QuizQnState>();
        quizCounter = 1;
        // return this;
        
        Debug.Log(scenes[scenes.Count-1]);
        Debug.Log(((QuizIntroScene) scenes[scenes.Count-1]).quizSession);
        Debug.Log($"quizName after creation: {quizName}");
        return this;
    }

    public List<SessionScene> getList() => scenes;
}

public interface AbstractSession
{
    public bool GoToPreviousScene();

    public bool GoToNextScene();

    public void EnterSessionScene();

    public bool GoToMenuScene();

    public void GoToMyMenuScene();

    public SessionScene GetNestedActiveScene();
}

public class Session : AbstractSession
{
    public string name;
    public List<SessionScene> scenes; // Change it back later...now for hacks below
    private int scenePosition;
    private SCENES menuScene;
    private SessionScene parent;

    public SessionScene Parent => parent;
    
    // Eventually, ALL of the below should be replaced with just public SessionScene activeScene!
    public SessionScene activeScene;
    
    // public List<SessionScene> reviewScenes;
    // public bool review;

    public Session(string name, SCENES menuScene, SessionScene parent, List<SessionScene> scenes)
    {
        this.name = name;
        this.scenes = scenes;
        scenePosition = 0;
        this.menuScene = menuScene;
        this.parent = parent;
        
        
        // review = false;

        foreach (SessionScene scene in scenes)
        {
            scene.session = this;
        }
    }

    // public static Session New<T>(string name, SCENES menuScene, List<T> scenes) where T : SessionScene
    // {
    //     return new Session(name, menuScene, (from scene in scenes select scene as SessionScene).ToList());
    // } 

    public Session(string name, SCENES menuScene, List<SessionScene> scenes) : this(name, menuScene, null, scenes) {}

    public Session(string name, SessionSceneListBuilder builder) : this(name, SCENES.MENU, null, builder.getList()) {}

    public Session(string name, List<SessionScene> scenes) : this(name, SCENES.MENU, null, scenes) {}

    public void GoToSceneAt(int scenePos)
    {
        // Debug.Log($"Session name: {name}");
        // Debug.Log($"scenePos: {scenePos}");
        // Debug.Log($"Count: {scenes.Count}");
        DataManager.Instance.AddNewDataEntry(scenes[scenePos].publicName);
        SetActiveScene(scenes[scenePos]);
        scenes[scenePos].GoToScene();
    } 

    public bool GoToPreviousScene()
    {
        if ((activeScene as AbstractSession)?.GoToPreviousScene() ?? false)
        {
            return true;
        }

        // Debug.Log($"current active scene: {activeScene?.publicName ?? "none"}");
        // Debug.Log($"oldScenePosition: {scenePosition}");
        
        scenePosition--;
        if (scenePosition < 0)
        {
            activeScene = null;
            scenePosition = 0;
            // DataManager.Instance.AddNewDataEntry("Menu");
            // ChangeScene.GoToScene(menuScene);
            return false;
        }
        else
        {
            GoToSceneAt(scenePosition);
            return true;
        }
    }

    public bool GoToNextScene()
    {
        if ((activeScene as AbstractSession)?.GoToNextScene() ?? false)
        {
            return true;
        }

        // Debug.Log($"current active scene: {activeScene?.publicName ?? "none"}");
        // Debug.Log($"oldScenePosition: {scenePosition}");

        scenePosition++;
        if (scenePosition >= scenes.Count)
        {
            activeScene = null;
            scenePosition = 0;
            // DataManager.Instance.AddNewDataEntry("Menu");
            // ChangeScene.GoToScene(menuScene);
            return false;
        }
        else
        {
            GoToSceneAt(scenePosition);
            return true;
        }
    }

    public void EnterSessionScene()
    {
        if (activeScene is AbstractSession session)
        {
            session.EnterSessionScene();
        }
        else
        {
            GoToFirstScene();
        }
    }

    public void GoToFirstScene()
    {
        // Debug.Log(Globals.activeSession);
        scenePosition = 0;
        GoToSceneAt(scenePosition);
    }

    public bool GoToMenuScene()
    {
        if (!((activeScene as AbstractSession)?.GoToMenuScene() ?? false))
        {
            GoToMyMenuScene();
        }
        return true;
    }

    public void GoToMyMenuScene()
    {

        // Debug.Log($"current active scene: {activeScene?.publicName ?? "none"}");
        // Debug.Log($"oldScenePosition: {scenePosition}");
        
        activeScene = null;
        scenePosition = 0;
        // DataManager.Instance.AddNewDataEntry("Menu");
        // ChangeScene.GoToScene(menuScene);
        if (parent != null)
        {
            DataManager.Instance.AddNewDataEntry(parent.publicName);
            parent.GoToScene();
        }
        else
        {
            DataManager.Instance.AddNewDataEntry("Menu");
            ChangeScene.GoToScene(SCENES.MENU);
        }
    }

    public void SetReviewScenes(List<SessionScene> reviewScenes)
    {
        // this.reviewScenes = reviewScenes;
    }

    public void SetActiveScene(SessionScene newActiveScene)
    {
        this.activeScene = newActiveScene;
    }

    public SessionScene GetNestedActiveScene()
    {
        return (activeScene as AbstractSession)?.GetNestedActiveScene() ?? activeScene;
    }
}

public static class Globals
{
    public static string lesson;

    public static string lessonCreate;

    public static bool isJSON = true;

    public static List<Session> defaultSessions;

    public static Session activeSession;

    // Convert LESSONS to lesson text file name
    public static string LessonEnumToString(LESSONS lessons)
    {
        switch (lessons)
        {
            case LESSONS.LESSON_1:
                // return "lesson1.txt";
                return "lesson1";
            case LESSONS.LESSON_2:
                // return "lesson2.txt";
                return "lesson2";
            default:
                return "lesson1";
        }
    }

    static Globals()
    {
        Debug.Log("Hi, initializing globals");
        defaultSessions = new List<Session>()
        {
            new ("Test", new SessionSceneListBuilder()
                // .AddScene(new SandboxScene("sandbox1", SCENES.TWO_GRIDS))
                .AddScene(new LessonScene("lesson1.txt"))
                .AddScene(new SandboxScene("sandbox1", SCENES.SANDBOX)) // For now, needs to be changed...
                .AddReviewScene("review1", new[] {0, 1})
                .AddQuizQn("y-component", SCENES.QUIZ, new SliderQnState(0.8f), 
                    "What is the y-component of the green vector?")
                .AddQuizQn("y-component-boog", SCENES.QUIZ_TEST, new SliderQnState(0.1f), 
                    "What is the x-component of the blue vector?")
                .AddWholeQuiz("quiz!", SCENES.QUIZ_INSTRUCTIONS)
                // .AddScene(new QuizIntroScene("quizIntro", SCENES.QUIZ))
                // .AddScene(new QuizQnScene("quiz1", SCENES.QUIZ, new SliderQnState(), "What is your favorite color?"))
            ),
            new("Lesson 1", new SessionSceneListBuilder()
                .AddScene(new LessonScene("video", "lesson1"))
                .AddScene(new SandboxScene("sandbox", SCENES.SANDBOX)) // For now, needs to be changed...
                .AddReviewScene("review1", new[] {0, 1})
                .AddQuizQnCount("encoding", SCENES.QUIZ_1, new DiscreteQnState(3),
                    "What can a vector encode?")
                .AddQuizQnCount("uses", SCENES.QUIZ_2, new DiscreteQnState(1),
                    "What can a vector represent?")
                .AddQuizQnCount("create", SCENES.QUIZ_3, new VectorQnState(new Vector3(4, 5)),
                    "Create a vector with\ncomponents (4, 5)")
                .AddQuizQnCount("what-x-comp", SCENES.QUIZ_4, new SliderQnState(0.9f),
                    "What is the x component\nof the vector?")
                .AddQuizQnCount("what-y-comp", SCENES.QUIZ_5, new SliderQnState(0.2f),
                    "What is the y component\nof this vector?")
                .AddQuizQnCount("which-y-comp", SCENES.QUIZ_6, new DiscreteQnState(4),
                    "Which of the following vectors have y component 1?")
                // .AddQuizQnCount("what-comps", SCENES.QUIZ_7, new CompositeQnState(new List<QuizQnState>()
                //     {
                //         new SliderQnState(1), new SliderQnState(0.3f)
                //     }), "What are the components of the below vector?")
                // .AddQuizQnCount("what-comps", SCENES.QUIZ_7, new TwoSliderQnState(1, 0.3f), "What are the components of the below vector?")
                .AddQuizQnCount("twice-hor-1", SCENES.QUIZ_8, new VectorQnState(new Vector3(1, 2)), 
                    "Modify the vector so that\nthe vertical component\nis twice that of the\nhorizontal component")
                .AddQuizQnCount("twice-hor-2", SCENES.QUIZ_9, new VectorQnState(new Vector3(3, 6)),
                    "Modify the vector so that\nthe vertical component\nis twice that of the\nhorizontal component")
                .AddQuizQnCount("paul", SCENES.QUIZ_10, new VectorQnState(new Vector3(2, 1)),
                    "Paul is moving from Point A to Point B in 3 seconds.\nCreate a vector that represents Paul's average" +
                    " velocity;\nor the distance that Paul would move in 1 second.")
                .AddWholeQuiz("quiz", SCENES.QUIZ_INSTRUCTIONS)
            ),
        };

        activeSession = defaultSessions[1];

        // QuizIntroScene a = activeSession.scenes[1] as QuizIntroScene;
        QuizIntroScene a = activeSession.scenes[3] as QuizIntroScene;
        ((VectorQnState) a.quizState.quizQnStates[6]).value = new Vector3(1, 5);
        ((VectorQnState) a.quizState.quizQnStates[7]).value = new Vector3(3, 5);
        // ((VectorQnState) a.quizState.quizQnStates[7]).value = new Vector3(1, 5);
        // ((VectorQnState) a.quizState.quizQnStates[8]).value = new Vector3(3, 5);

        // DEBUG
        // Debug.Log(Globals.activeSession);
        // activeSession.scenePosition = 1;    
    }
}