using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Events;

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

     public UnityEvent<CompositeQnState> broadcastComposite;
     
     private CompositeQnState state;

     private void Start()
     {
          state = (Globals.activeSession.GetNestedActiveScene() as QuizQnScene)?.qnState as CompositeQnState;
          if (state == null)
          {
               Debug.Log("Oops!");
          }
          for (int i = 0; i < state.value.Count; i++)
          {
               switch(state.value[i], stateUsers[i])
               {
                    case (VectorQnState vectorQnState, VectorStateUser vectorStateUser):
                         vectorStateUser.vectorManager.SetNewStandardValue(vectorQnState.value);
                         break;
                    case (SliderQnState sliderQnState, SliderStateUser sliderStateUser):
                         sliderStateUser.pinchSlider.SliderValue = sliderQnState.value;
                         break;
                    default:
                         Debug.Log("oops! Type mismatch in quizComposite!");
                         break;
               }
          }
     }

     public void updateQnState()
     {
          if (state != null)
          {
               for (int i = 0; i < state.value.Count; i++)
               {
                    switch(state.value[i], stateUsers[i])
                    {
                         case (VectorQnState vectorQnState, VectorStateUser vectorStateUser):
                              vectorQnState.value = vectorStateUser.vectorManager.standardValue;
                              break;
                         case (SliderQnState sliderQnState, SliderStateUser sliderStateUser):
                              sliderQnState.value = sliderStateUser.pinchSlider.SliderValue;
                              break;
                    }
               }
               
               broadcastComposite.Invoke(state);
          }
     }
}