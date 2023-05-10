using System;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    public TextMesh questionText;
    public TextMesh numberText;

    private void Start()
    {
        QuizQnScene curScene = Globals.activeSession.GetNestedActiveScene() as QuizQnScene;
        questionText.text = curScene?.quizQnText ?? "Oops! We couldn't load the text!";
        numberText.text = $"Question {curScene?.quizQnNum ?? 0}";
    }
}