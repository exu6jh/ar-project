using System;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    public TextMesh questionText;

    private void Start()
    {
        questionText.text = (Globals.activeSession.GetNestedActiveScene() as QuizQnScene)?.quizQnText ?? "Oops! We couldn't load the text!";
    }
}