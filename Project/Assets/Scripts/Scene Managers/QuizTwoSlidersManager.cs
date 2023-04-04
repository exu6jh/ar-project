using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Events;

public class QuizTwoSlidersManager : MonoBehaviour
{
     public PinchSlider pinchSlider1, pinchSlider2;
     private TwoSliderQnState state;
     
     public UnityEvent<TwoSliderQnState> broadcastComposite;

     private void Start()
     {
          state = (Globals.activeSession.GetNestedActiveScene() as QuizQnScene)?.qnState as TwoSliderQnState;
          if (state == null)
          {
               Debug.Log("Oops!");
          }
          pinchSlider1.SliderValue = state?.value1 ?? 0.0f;
          pinchSlider2.SliderValue = state?.value2 ?? 0.0f;
     }

     public void updateQnState()
     {
          if (state != null)
          {
               state.value1 = pinchSlider1.SliderValue;
               state.value2 = pinchSlider2.SliderValue;    
               broadcastComposite.Invoke(state);
          }
     }
}