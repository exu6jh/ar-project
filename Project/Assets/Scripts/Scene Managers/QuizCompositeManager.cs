using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

[System.Serializable]
public abstract class QuizStateUser
{
}

[System.Serializable]
public class SliderStateUser : QuizStateUser
{
     public PinchSlider pinchSlider;
}

[System.Serializable]
public class VectorStateUser : QuizStateUser
{
     public VectorManager vectorManager;
}

public class QuizCompositeManager : MonoBehaviour
{
     [SerializeReference]
     public List<QuizStateUser> stateUsers;
     
     private CompositeQnState state;

     private void Start()
     {
          state = (Globals.activeSession.GetNestedActiveScene() as QuizQnScene)?.qnState as CompositeQnState;
          if (state == null)
          {
               Debug.Log("Oops!");
          }
          // vectorManager.SetNewStandardValue(state?.value ?? standard);
     }

     public void updateQnState()
     {
          if (state != null)
          {
               // state.value = vectorManager.standardValue;
          }
     }
}