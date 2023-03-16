using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LessonData : Data
{
  public bool Completed = false;

  public List<InteractionData> Interactions;

  public List<GazeData> Gaze;

  public List<QuizData> Quiz;

}
