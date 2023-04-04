using System.Collections.Generic;
using UnityEngine;

public class QuizSubmitManager : MonoBehaviour
{
    public void Submit()
    {
        foreach (QuizQnGrade grade in (Globals.activeSession.GetNestedActiveScene().parentScene() as QuizIntroScene)?.quizState?.grade() ?? new List<QuizQnGrade>())
        {
            // DON'T COMMIT!!! TEST ONLY
            // DataManager.Instance.AddQuizData(grade.questionName, grade.correct);
            Debug.Log($"Question: {grade.questionName}, Correct: {grade.correct}");
        }
        
    }
}