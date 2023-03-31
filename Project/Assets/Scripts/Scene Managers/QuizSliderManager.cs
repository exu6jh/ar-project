using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class QuizSliderManager : MonoBehaviour
{
     public PinchSlider pinchSlider;
     private SliderQnState state;

     private void Start()
     {
          state = (Globals.activeSession.GetNestedActiveScene() as QuizQnScene)?.qnState as SliderQnState;
          if (state == null)
          {
               Debug.Log("Oops!");
          }
          pinchSlider.SliderValue = state?.value ?? 0.0f;
     }

     public void updateQnState()
     {
          if (state != null)
          {
               state.value = pinchSlider.SliderValue;
          }
     }
}